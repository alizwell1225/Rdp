using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RdpGrpc.Proto;
using LIB_RPC.Abstractions;
using LIB_RPC.Optimizations;

namespace LIB_RPC
{
    /// <summary>
    /// OPTIMIZED: Uses BufferPool and CompressionHelper for better performance
    /// </summary>
    public sealed class RemoteChannelService : RemoteChannel.RemoteChannelBase
    {
        private readonly GrpcConfig _config;
        private readonly GrpcLogger _logger;
        private readonly IScreenCapture _screenCapture;
        private readonly ConcurrentDictionary<string, DuplexClient> _duplexClients = new();
        private readonly ConcurrentDictionary<string, IServerStreamWriter<FileChunk>> _filePushClients = new();
        
        // Event raised when a file has been uploaded successfully to the server storage
        public event EventHandler<string>? FileUploaded;
        
        // Event raised when file upload starts
        public event EventHandler<string>? FileUploadStarted;
        
        // Event raised when file upload completes
        public event EventHandler<string>? FileUploadCompleted;
        
        // Event raised when file upload fails
        public event EventHandler<(string Path, string Error)>? FileUploadFailed;
        
        // Event raised when client connects (duplex or file push)
        public event EventHandler<string>? ClientConnected;
        
        // Event raised when client disconnects
        public event EventHandler<string>? ClientDisconnected;
        
        // Event raised when client sends byte data to server (type, data, metadata)
        public event EventHandler<(string Type, byte[] Data, string? Metadata)>? ClientByteDataReceived;
        
        // Event raised when client sends JSON message to server (id, type, json, timestamp)
        public event EventHandler<(string Id, string Type, string Json, long Timestamp)>? ClientJsonReceived;
        
        // Allow multiple clients concurrently for duplex and file-push subscriptions

        private sealed record DuplexClient(string Id, IServerStreamWriter<JsonEnvelope> Writer, SemaphoreSlim WriteLock);
        private bool UseStoragePathFile => _config.CheckStorageRootHaveFile;
        public RemoteChannelService(GrpcConfig config, GrpcLogger logger, IScreenCapture screenCapture)
        {
            _config = config;
            _logger = logger;
            _screenCapture = screenCapture;
            _config.EnsureFolders();
        }

        // Helper methods to get client counts for validation
        public int GetDuplexClientCount() => _duplexClients.Count;
        public int GetFilePushClientCount() => _filePushClients.Count;

        // (Previously there was logic to restrict to a single active client; removed to allow multiple clients.)

        public override async Task JsonStream(IAsyncStreamReader<JsonEnvelope> requestStream, IServerStreamWriter<JsonAck> responseStream, ServerCallContext context)
        {
            await foreach (var msg in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _logger.Info($"JSON received type={msg.Type} id={msg.Id} bytes={msg.Json?.Length}" );
                var ack = new JsonAck { Id = msg.Id, Success = true };
                await responseStream.WriteAsync(ack);
            }
        }

        //��y�覡
        // New duplex JSON channel allowing server to push messages back at any time
        public override async Task JsonDuplex(IAsyncStreamReader<JsonEnvelope> requestStream, IServerStreamWriter<JsonEnvelope> responseStream, ServerCallContext context)
        {
            var id = Guid.NewGuid().ToString("N");
            var client = new DuplexClient(id, responseStream, new SemaphoreSlim(1, 1));
            // Multiple duplex clients are allowed concurrently
            _duplexClients[id] = client;
            _logger.Info($"Duplex client connected {id}, totalDuplex={_duplexClients.Count}");
            ClientConnected?.Invoke(this, id);
            
            try
            {
                await foreach (var msg in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    _logger.Info($"[Duplex IN] id={msg.Id} type={msg.Type} bytes={msg.Json?.Length}");
                    
                    // Trigger event for all JSON messages received
                    ClientJsonReceived?.Invoke(this, (msg.Id, msg.Type, msg.Json, msg.Timestamp));
                    
                    // Handle device info requests
                    if (msg.Type == "device_info_request")
                    {
                        try
                        {
                            var deviceInfo = GetDeviceInformation();
                            var responseJson = System.Text.Json.JsonSerializer.Serialize(deviceInfo);
                            var response = new JsonEnvelope
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Type = "device_info_response",
                                Json = responseJson,
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            };
                            
                            await client.WriteLock.WaitAsync(context.CancellationToken);
                            try
                            {
                                await responseStream.WriteAsync(response);
                            }
                            finally
                            {
                                client.WriteLock.Release();
                            }
                            
                            _logger.Info($"[Duplex OUT] Sent device info response");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Failed to handle device_info_request: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Duplex stream error {id}: {ex.Message}");
            }
            finally
            {
                _duplexClients.TryRemove(id, out _);
                client.WriteLock.Dispose();
                _logger.Info($"Duplex client disconnected {id}, totalDuplex={_duplexClients.Count}");
                ClientDisconnected?.Invoke(this, id);
            }
        }

        public async Task<int> BroadcastJsonAsync(string type, string json, CancellationToken ct = default)
        {
            var envelope = new JsonEnvelope
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                Json = json,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            int successCount = 0;
            var clients = _duplexClients.ToArray();
            
            foreach (var kvp in clients)
            {
                var dc = kvp.Value;
                try
                {
                    // Use async-safe semaphore for locking
                    await dc.WriteLock.WaitAsync(ct);
                    try
                    {
                        await dc.Writer.WriteAsync(envelope);
                        successCount++;
                    }
                    finally
                    {
                        dc.WriteLock.Release();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Broadcast to {dc.Id} failed: {ex.Message}; removing");
                    _duplexClients.TryRemove(dc.Id, out _);
                }
                ct.ThrowIfCancellationRequested();
            }
            return successCount;
        }

        //�o�覡�O��y�覡
        // FilePush subscription: client opens a stream and we keep its writer for future pushes
        public override async Task FilePush(Google.Protobuf.WellKnownTypes.Empty request, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
        {
            var id = Guid.NewGuid().ToString("N");
            // Allow multiple FilePush subscribers concurrently
            _filePushClients[id] = responseStream;
            _logger.Info($"FilePush subscriber {id} connected, totalFilePush={_filePushClients.Count}");
            ClientConnected?.Invoke(this, id);
            
            try
            {
                // Keep the call open until cancellation
                var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                context.CancellationToken.Register(() => tcs.TrySetResult());
                await tcs.Task;
            }
            finally
            {
                _filePushClients.TryRemove(id, out _);
                _logger.Info($"FilePush subscriber {id} disconnected, totalFilePush={_filePushClients.Count}");
                ClientDisconnected?.Invoke(this, id);
            }
        }

        public async Task BroadcastFileAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            
            // Check if there are any clients to receive the file
            var totalClients = _filePushClients.Count;
            if (totalClients == 0)
            {
                _logger.Warn($"No clients connected to receive file broadcast: {filePath}");
                return;
            }
            
            var name = Path.GetFileName(filePath);
            var bytes = await File.ReadAllBytesAsync(filePath, ct);
            var chunkSize = _config.MaxChunkSizeBytes;
            var total = (int)Math.Ceiling((double)bytes.Length / chunkSize);
            _logger.Info($"Broadcasting file '{name}' size={bytes.Length} chunks={total} to {totalClients} clients");
            
            var successfulClients = 0;
            for (int i = 0; i < total; i++)
            {
                ct.ThrowIfCancellationRequested();
                var slice = bytes.AsMemory(i * chunkSize, Math.Min(chunkSize, bytes.Length - i * chunkSize));
                var chunk = new FileChunk
                {
                    Path = name,
                    Data = Google.Protobuf.ByteString.CopyFrom(slice.Span),
                    Index = i,
                    TotalChunks = total,
                    IsLast = i == total - 1
                };
                
                // Track which clients successfully received this chunk
                var clientsSnapshot = _filePushClients.ToArray();
                foreach (var kv in clientsSnapshot)
                {
                    try
                    {
                        await kv.Value.WriteAsync(chunk);
                        if (i == total - 1) successfulClients++; // Count on last chunk
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"Push file chunk to {kv.Key} failed: {ex.Message}; removing");
                        _filePushClients.TryRemove(kv.Key, out _);
                    }
                }
            }
            
            _logger.Info($"File broadcast completed: {name}, succeeded={successfulClients}/{totalClients}");
        }

        public override async Task<FileTransferStatus> UploadFile(IAsyncStreamReader<FileChunk> requestStream, ServerCallContext context)
        {
            string? path = null;
            FileStream? fs = null;
            var success = false;
            var uploadStarted = false;
            
            try
            {
                await foreach (var chunk in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    path ??= Path.Combine(_config.StorageRoot, Path.GetFileName(chunk.Path));
                    if (fs == null)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                        fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                        _logger.Info($"Upload start {path}");
                        
                        if (!uploadStarted)
                        {
                            FileUploadStarted?.Invoke(this, path);
                            uploadStarted = true;
                        }
                    }
                    if (chunk.Data.Length > 0) await fs.WriteAsync(chunk.Data.Memory);
                    if (chunk.IsLast)
                    {
                        _logger.Info($"Upload complete {path}");
                        success = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Upload fail {path}: {ex.Message}");
                
                if (!string.IsNullOrWhiteSpace(path))
                {
                    FileUploadFailed?.Invoke(this, (path, ex.Message));
                }
                
                return new FileTransferStatus { Path = path ?? string.Empty, Success = false, Error = ex.Message };
            }
            finally
            {
                if (fs != null) await fs.DisposeAsync();
                // If upload succeeded, raise events so UI/host can react
                if (success && !string.IsNullOrWhiteSpace(path))
                {
                    try
                    {
                        FileUploaded?.Invoke(this, path!);
                        FileUploadCompleted?.Invoke(this, path!);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"FileUploaded/FileUploadCompleted event handler threw: {ex.Message}");
                    }
                }
                else if (!success && !string.IsNullOrWhiteSpace(path) && uploadStarted)
                {
                    try
                    {
                        FileUploadFailed?.Invoke(this, (path!, "Upload incomplete"));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"FileUploadFailed event handler threw: {ex.Message}");
                    }
                }
            }
            return new FileTransferStatus { Path = path ?? string.Empty, Success = success };
        }

        public override async Task DownloadFile(FileRequest request, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
        {
            var target = Path.Combine(_config.StorageRoot, Path.GetFileName(request.Path));
            if (!File.Exists(target))
            {
                await responseStream.WriteAsync(new FileChunk { Path = request.Path, Error = "File not found", IsLast = true });
                return;
            }
            var bytes = await File.ReadAllBytesAsync(target, context.CancellationToken);
            var chunkSize = _config.MaxChunkSizeBytes;
            var totalChunks = (int)Math.Ceiling((double)bytes.Length / chunkSize);
            for (int i = 0; i < totalChunks; i++)
            {
                var slice = bytes.AsMemory(i * chunkSize, Math.Min(chunkSize, bytes.Length - i * chunkSize));
                await responseStream.WriteAsync(new FileChunk
                {
                    Path = request.Path,
                    Data = Google.Protobuf.ByteString.CopyFrom(slice.Span),
                    Index = i,
                    TotalChunks = totalChunks,
                    IsLast = i == totalChunks - 1
                });
            }
        }

        public override Task<ListFilesResponse> ListFiles(ListFilesRequest request, ServerCallContext context)
        {
            try
            {
                var dir = string.IsNullOrWhiteSpace(request.Directory) ? _config.StorageRoot : Path.Combine(_config.StorageRoot, request.Directory);
                if (!Directory.Exists(dir)) return Task.FromResult(new ListFilesResponse());
                var files = Directory.EnumerateFiles(dir).Select(Path.GetFileName).Where(n => n != null).ToArray()!;
                var resp = new ListFilesResponse();
                resp.Files.AddRange(files);
                return Task.FromResult(resp);
            }
            catch (Exception ex)
            {
                _logger.Error($"ListFiles error: {ex.Message}");
                return Task.FromResult(new ListFilesResponse());
            }
        }

        public override async Task Screenshot(ScreenshotRequest request, IServerStreamWriter<ScreenshotChunk> responseStream, ServerCallContext context)
        {
            try
            {
                var data = _screenCapture.CapturePrimaryPng();
                var chunkSize = _config.MaxChunkSizeBytes;
                var total = (int)Math.Ceiling((double)data.Length / chunkSize);
                for (int i = 0; i < total; i++)
                {
                    var slice = data.AsMemory(i * chunkSize, Math.Min(chunkSize, data.Length - i * chunkSize));
                    await responseStream.WriteAsync(new ScreenshotChunk
                    {
                        Index = i,
                        TotalChunks = total,
                        Data = Google.Protobuf.ByteString.CopyFrom(slice.Span),
                        IsLast = i == total - 1
                    });
                }
            }
            catch (Exception ex)
            {
                await responseStream.WriteAsync(new ScreenshotChunk { Error = ex.Message, IsLast = true });
            }
        }

        // screen capture moved to ScreenCapture utility

        /// <summary>
        /// Broadcast JSON with acknowledgment (Unary RPC)
        /// </summary>
        public override async Task<BroadcastResponse> BroadcastWithAck(
            BroadcastRequest request, 
            ServerCallContext context)
        {
            try
            {
                var clients = GetDuplexClientsSnapshot();
                if (clients.Count == 0)
                {
                    return new BroadcastResponse
                    {
                        Success = false,
                        ClientsReached = 0,
                        Error = "No clients connected"
                    };
                }

                var envelope = new JsonEnvelope
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Type = request.Type,
                    Json = request.Json,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                int successCount = 0;
                var tasks = new List<Task>();

                foreach (var kvp in clients)
                {
                    var dc = kvp.Value;
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            lock (dc.WriteLock)
                            {
                                dc.Writer.WriteAsync(envelope).GetAwaiter().GetResult();
                            }
                            Interlocked.Increment(ref successCount);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"[Server PushFileWithAck] Failed to send to client {kvp.Key}: {ex.Message}");
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                _logger.Info($"[Server PushFileWithAck] Sent to {successCount}/{clients.Count} clients");

                return new BroadcastResponse
                {
                    Success = successCount > 0,
                    ClientsReached = successCount,
                    Error = successCount == 0 ? "All clients failed to receive" : string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"[Server PushFileWithAck] Error: {ex.Message}");
                return new BroadcastResponse
                {
                    Success = false,
                    ClientsReached = 0,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Push file with acknowledgment (Unary RPC)
        /// </summary>
        public override async Task<PushFileResponse> PushFileWithAck(
            PushFileRequest request, 
            ServerCallContext context)
        {
            try
            {
                var sourePath = string.Empty;
                var clients = GetFilePushClientsSnapshot();
                if (clients.Count == 0)
                {
                    return new PushFileResponse
                    {
                        Success = false,
                        ClientsReached = 0,
                        Error = "No clients connected for file push"
                    };
                }
                byte[] fileBytes = null;
                if (UseStoragePathFile)
                {
                    var target = Path.Combine(_config.StorageRoot, Path.GetFileName(request.FilePath));
                    if (!File.Exists(target))
                    {
                        return new PushFileResponse
                        {
                            Success = false,
                            ClientsReached = 0,
                            Error = $"File not found: {request.FilePath}"
                        };
                    }
                    fileBytes = await File.ReadAllBytesAsync(target);
                }
                else
                {
                    var soure = Path.Combine(_config.GetUserStoragePath());
                    sourePath = soure;
                    if (!File.Exists(sourePath))
                    {
                        return new PushFileResponse
                        {
                            Success = false,
                            ClientsReached = 0,
                            Error = $"File not found: {request.FilePath}"
                        };
                    }
                    fileBytes = await File.ReadAllBytesAsync(sourePath);
                }
                var fileName = Path.GetFileName(sourePath);
                var chunkSize = _config.MaxChunkSizeBytes;
                var totalChunks = (int)Math.Ceiling((double)fileBytes.Length / chunkSize);

                int successCount = 0;
                var tasks = new List<Task>();

                foreach (var kvp in clients)
                {
                    var writer = kvp.Value;
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Send file in chunks
                            for (int i = 0; i < totalChunks; i++)
                            {
                                var slice = fileBytes.AsMemory(i * chunkSize, Math.Min(chunkSize, fileBytes.Length - i * chunkSize));
                                await writer.WriteAsync(new FileChunk
                                {
                                    Path = fileName,
                                    Data = Google.Protobuf.ByteString.CopyFrom(slice.Span),
                                    Index = i,
                                    TotalChunks = totalChunks,
                                    IsLast = i == totalChunks - 1
                                });
                            }
                            Interlocked.Increment(ref successCount);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"[Server PushFileWithAck] Failed to send to client {kvp.Key}: {ex.Message}");
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                _logger.Info($"[Server PushFileWithAck] Sent file '{fileName}' to {successCount}/{clients.Count} clients");

                return new PushFileResponse
                {
                    Success = successCount > 0,
                    ClientsReached = successCount,
                    Error = successCount == 0 ? "All clients failed to receive file" : string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"[Server PushFileWithAck] Error: {ex.Message}");
                return new PushFileResponse
                {
                    Success = false,
                    ClientsReached = 0,
                    Error = ex.Message
                };
            }
        }

        // NEW: Send byte data with acknowledgment
        public override async Task<ByteAck> SendByte(ByteData request, ServerCallContext context)
        {
            try
            {
                _logger.Info($"[SendByte] Received {request.Data.Length} bytes, type={request.Type}, id={request.Id}");
                
                // Trigger event for byte data reception
                ClientByteDataReceived?.Invoke(this, (request.Type, request.Data.ToByteArray(), request.Metadata));
                
                return new ByteAck
                {
                    Id = request.Id,
                    Success = true,
                    Error = string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"[SendByte] Error: {ex.Message}");
                return new ByteAck
                {
                    Id = request.Id,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        // NEW: Send byte data without acknowledgment
        public override async Task<Empty> SendByteNoAck(ByteData request, ServerCallContext context)
        {
            try
            {
                _logger.Info($"[SendByteNoAck] Received {request.Data.Length} bytes, type={request.Type}");
                
                // Trigger event for byte data reception
                ClientByteDataReceived?.Invoke(this, (request.Type, request.Data.ToByteArray(), request.Metadata));
                
                return new Empty();
            }
            catch (Exception ex)
            {
                _logger.Error($"[SendByteNoAck] Error: {ex.Message}");
                return new Empty();
            }
        }

        // NEW: Broadcast byte data to all clients with acknowledgment
        public override async Task<ByteBroadcastResponse> BroadcastByte(ByteData request, ServerCallContext context)
        {
            try
            {
                var clients = GetDuplexClientsSnapshot();
                
                if (clients.Count == 0)
                {
                    return new ByteBroadcastResponse
                    {
                        Success = false,
                        ClientsReached = 0,
                        Error = "No clients connected"
                    };
                }

                _logger.Info($"[BroadcastByte] Broadcasting {request.Data.Length} bytes to {clients.Count} clients, type={request.Type}");
                
                int successCount = 0;
                var tasks = new List<Task>();

                foreach (var kvp in clients)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Create a JSON envelope to send the byte data notification
                            var envelope = new JsonEnvelope
                            {
                                Id = request.Id ?? Guid.NewGuid().ToString("N"),
                                Type = "byte_data",
                                Json = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    type = request.Type,
                                    dataLength = request.Data.Length,
                                    metadata = request.Metadata
                                }),
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            };

                            lock (kvp.Value.WriteLock)
                            {
                                kvp.Value.Writer.WriteAsync(envelope).Wait();
                            }
                            Interlocked.Increment(ref successCount);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"[BroadcastByte] Failed to send to client {kvp.Key}: {ex.Message}");
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                _logger.Info($"[BroadcastByte] Broadcast complete: {successCount}/{clients.Count} clients reached");

                return new ByteBroadcastResponse
                {
                    Success = successCount > 0,
                    ClientsReached = successCount,
                    Error = successCount == 0 ? "All clients failed to receive data" : string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"[BroadcastByte] Error: {ex.Message}");
                return new ByteBroadcastResponse
                {
                    Success = false,
                    ClientsReached = 0,
                    Error = ex.Message
                };
            }
        }

        // NEW: Broadcast byte data to all clients without acknowledgment
        public override async Task<Empty> BroadcastByteNoAck(ByteData request, ServerCallContext context)
        {
            try
            {
                var clients = GetDuplexClientsSnapshot();
                
                _logger.Info($"[BroadcastByteNoAck] Broadcasting {request.Data.Length} bytes to {clients.Count} clients, type={request.Type}");
                
                var tasks = new List<Task>();

                foreach (var kvp in clients)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var envelope = new JsonEnvelope
                            {
                                Id = request.Id ?? Guid.NewGuid().ToString("N"),
                                Type = "byte_data",
                                Json = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    type = request.Type,
                                    dataLength = request.Data.Length,
                                    metadata = request.Metadata
                                }),
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            };

                            lock (kvp.Value.WriteLock)
                            {
                                kvp.Value.Writer.WriteAsync(envelope).Wait();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"[BroadcastByteNoAck] Failed to send to client {kvp.Key}: {ex.Message}");
                        }
                    }));
                }

                // Fire and forget - don't wait for completion
                _ = Task.WhenAll(tasks);

                return new Empty();
            }
            catch (Exception ex)
            {
                _logger.Error($"[BroadcastByteNoAck] Error: {ex.Message}");
                return new Empty();
            }
        }

        // NEW: Byte push subscription (server -> client streaming)
        private readonly ConcurrentDictionary<string, IServerStreamWriter<ByteData>> _bytePushClients = new();

        public override async Task BytePush(Empty request, IServerStreamWriter<ByteData> responseStream, ServerCallContext context)
        {
            var id = Guid.NewGuid().ToString("N");
            _bytePushClients[id] = responseStream;
            _logger.Info($"BytePush client subscribed {id}, total={_bytePushClients.Count}");
            ClientConnected?.Invoke(this, id);

            try
            {
                // Keep the stream open until client disconnects
                await Task.Delay(Timeout.Infinite, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.Info($"BytePush client {id} disconnected gracefully");
            }
            catch (Exception ex)
            {
                _logger.Warn($"BytePush client {id} error: {ex.Message}");
            }
            finally
            {
                _bytePushClients.TryRemove(id, out _);
                _logger.Info($"BytePush client {id} removed, remaining={_bytePushClients.Count}");
                ClientDisconnected?.Invoke(this, id);
            }
        }

        // Helper method to push byte data to all subscribed clients
        public async Task PushByteDataToClientsAsync(string type, byte[] data, string? metadata = null)
        {
            var clients = _bytePushClients.ToArray();
            if (clients.Length == 0)
            {
                _logger.Info($"[PushByteData] No clients subscribed");
                return;
            }

            _logger.Info($"[PushByteData] Pushing {data.Length} bytes to {clients.Length} clients, type={type}");

            var byteData = new ByteData
            {
                Type = type,
                Data = Google.Protobuf.ByteString.CopyFrom(data),
                Metadata = metadata ?? string.Empty,
                Id = Guid.NewGuid().ToString("N")
            };

            var tasks = clients.Select(async kvp =>
            {
                try
                {
                    await kvp.Value.WriteAsync(byteData);
                }
                catch (Exception ex)
                {
                    _logger.Warn($"[PushByteData] Failed to push to client {kvp.Key}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }

        // Helper method to get byte push client count
        public int GetBytePushClientCount()
        {
            return _bytePushClients.Count;
        }

        // Helper method to get duplex clients snapshot
        private Dictionary<string, DuplexClient> GetDuplexClientsSnapshot()
        {
            return _duplexClients.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // Helper method to get file push clients snapshot
        private Dictionary<string, IServerStreamWriter<FileChunk>> GetFilePushClientsSnapshot()
        {
            return _filePushClients.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        // Helper method to get device information (MAC addresses and StationIndex)
        private object GetDeviceInformation()
        {
            var macAddresses = new List<string>();
            
            try
            {
                // Get all network interfaces
                var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (var nic in interfaces)
                {
                    // Only include operational interfaces with valid MAC addresses
                    if (nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                        nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    {
                        var mac = nic.GetPhysicalAddress().ToString();
                        if (!string.IsNullOrEmpty(mac) && mac != "000000000000")
                        {
                            // Format MAC address with colons (e.g., AA:BB:CC:DD:EE:FF)
                            var formattedMac = string.Join(":", 
                                Enumerable.Range(0, mac.Length / 2)
                                    .Select(i => mac.Substring(i * 2, 2)));
                            macAddresses.Add(formattedMac);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get MAC addresses: {ex.Message}");
            }
            
            // Get StationIndex from config
            var stationIndex = _config.StationIndex ?? "-1";
            
            return new
            {
                MacAddresses = macAddresses,
                StationIndex = stationIndex,
                Success = true,
                Error = string.Empty
            };
        }
    }
}
