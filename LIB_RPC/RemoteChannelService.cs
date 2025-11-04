using System.Collections.Concurrent;
using Grpc.Core;
using RdpGrpc.Proto;
using LIB_RPC.Abstractions;

namespace LIB_RPC
{
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
        
        // Allow multiple clients concurrently for duplex and file-push subscriptions

        private sealed record DuplexClient(string Id, IServerStreamWriter<JsonEnvelope> Writer, object WriteLock);

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

        // New duplex JSON channel allowing server to push messages back at any time
        public override async Task JsonDuplex(IAsyncStreamReader<JsonEnvelope> requestStream, IServerStreamWriter<JsonEnvelope> responseStream, ServerCallContext context)
        {
            var id = Guid.NewGuid().ToString("N");
            var client = new DuplexClient(id, responseStream, new object());
            // Multiple duplex clients are allowed concurrently
            _duplexClients[id] = client;
            _logger.Info($"Duplex client connected {id}, totalDuplex={_duplexClients.Count}");
            ClientConnected?.Invoke(this, id);
            
            try
            {
                await foreach (var msg in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    _logger.Info($"[Duplex IN] id={msg.Id} type={msg.Type} bytes={msg.Json?.Length}");
                    // (Optional) echo back acknowledgement style message
                    // Here we just log; user code can query later if needed.
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Duplex stream error {id}: {ex.Message}");
            }
            finally
            {
                _duplexClients.TryRemove(id, out _);
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
                    // Use proper async lock to prevent deadlocks
                    await Task.Run(async () =>
                    {
                        lock (dc.WriteLock)
                        {
                            // Perform the actual write inside the lock but use Task.Run to avoid blocking
                            dc.Writer.WriteAsync(envelope).GetAwaiter().GetResult();
                        }
                    }, ct);
                    successCount++;
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
                            _logger.Warn($"[BroadcastWithAck] Failed to send to client {kvp.Key}: {ex.Message}");
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                _logger.Info($"[BroadcastWithAck] Sent to {successCount}/{clients.Count} clients");

                return new BroadcastResponse
                {
                    Success = successCount > 0,
                    ClientsReached = successCount,
                    Error = successCount == 0 ? "All clients failed to receive" : string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"[BroadcastWithAck] Error: {ex.Message}");
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

                var fileBytes = await File.ReadAllBytesAsync(target);
                var fileName = Path.GetFileName(request.FilePath);
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
                            _logger.Warn($"[PushFileWithAck] Failed to send to client {kvp.Key}: {ex.Message}");
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                _logger.Info($"[PushFileWithAck] Sent file '{fileName}' to {successCount}/{clients.Count} clients");

                return new PushFileResponse
                {
                    Success = successCount > 0,
                    ClientsReached = successCount,
                    Error = successCount == 0 ? "All clients failed to receive file" : string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"[PushFileWithAck] Error: {ex.Message}");
                return new PushFileResponse
                {
                    Success = false,
                    ClientsReached = 0,
                    Error = ex.Message
                };
            }
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
    }
}
