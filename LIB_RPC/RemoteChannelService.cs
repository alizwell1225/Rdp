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
    // Allow multiple clients concurrently for duplex and file-push subscriptions

        private sealed record DuplexClient(string Id, IServerStreamWriter<JsonEnvelope> Writer, object WriteLock);

        public RemoteChannelService(GrpcConfig config, GrpcLogger logger, IScreenCapture screenCapture)
        {
            _config = config;
            _logger = logger;
            _screenCapture = screenCapture;
            _config.EnsureFolders();
        }

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
            }
        }

        public async Task BroadcastJsonAsync(string type, string json, CancellationToken ct = default)
        {
            var envelope = new JsonEnvelope
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                Json = json,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            foreach (var kvp in _duplexClients.ToArray())
            {
                var dc = kvp.Value;
                try
                {
                    lock (dc.WriteLock)
                    {
                        dc.Writer.WriteAsync(envelope).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Broadcast to {dc.Id} failed: {ex.Message}; removing");
                    _duplexClients.TryRemove(dc.Id, out _);
                }
                ct.ThrowIfCancellationRequested();
            }
        }

        // FilePush subscription: client opens a stream and we keep its writer for future pushes
        public override async Task FilePush(Google.Protobuf.WellKnownTypes.Empty request, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
        {
            var id = Guid.NewGuid().ToString("N");
            // Allow multiple FilePush subscribers concurrently
            _filePushClients[id] = responseStream;
            _logger.Info($"FilePush subscriber {id} connected, totalFilePush={_filePushClients.Count}");
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
            }
        }

        public async Task BroadcastFileAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            var name = Path.GetFileName(filePath);
            var bytes = await File.ReadAllBytesAsync(filePath, ct);
            var chunkSize = _config.MaxChunkSizeBytes;
            var total = (int)Math.Ceiling((double)bytes.Length / chunkSize);
            _logger.Info($"Broadcast file '{name}' size={bytes.Length} chunks={total} to {_filePushClients.Count} subscribers");
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
                foreach (var kv in _filePushClients.ToArray())
                {
                    try
                    {
                        await kv.Value.WriteAsync(chunk);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"Push file chunk to {kv.Key} failed: {ex.Message}; removing");
                        _filePushClients.TryRemove(kv.Key, out _);
                    }
                }
            }
        }

        public override async Task<FileTransferStatus> UploadFile(IAsyncStreamReader<FileChunk> requestStream, ServerCallContext context)
        {
            string? path = null;
            FileStream? fs = null;
            var success = false;
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
                return new FileTransferStatus { Path = path ?? string.Empty, Success = false, Error = ex.Message };
            }
            finally
            {
                if (fs != null) await fs.DisposeAsync();
                // If upload succeeded, raise FileUploaded event so UI/host can react
                if (success && !string.IsNullOrWhiteSpace(path))
                {
                    try
                    {
                        FileUploaded?.Invoke(this, path!);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"FileUploaded event handler threw: {ex.Message}");
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
    }
}
