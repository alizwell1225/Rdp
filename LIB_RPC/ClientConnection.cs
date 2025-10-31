using System.Collections.Concurrent;
using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using RdpGrpc.Proto;

namespace LIB_RPC
{
    /// <summary>
    /// Low-level client connection handling gRPC communication with the remote channel service.
    /// </summary>
    public sealed class ClientConnection : IAsyncDisposable
    {
        private readonly GrpcConfig _config;
        private readonly GrpcLogger _logger;
        private GrpcChannel? _channel;
        private RemoteChannel.RemoteChannelClient? _client;
        private AsyncDuplexStreamingCall<JsonEnvelope, JsonAck>? _jsonStream;
        private AsyncDuplexStreamingCall<JsonEnvelope, JsonEnvelope>? _jsonDuplex;
        private AsyncServerStreamingCall<FileChunk>? _filePushStream;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonAck>> _pending = new();

        /// <summary>
        /// Event raised when a JSON acknowledgment is received.
        /// </summary>
        public event Action<JsonAck>? OnJsonAck;
        
        /// <summary>
        /// Event raised when server pushes a JSON message via duplex stream.
        /// </summary>
        public event Action<JsonEnvelope>? OnServerJson;
        
        /// <summary>
        /// Event raised during file upload progress with path and percentage (0-100).
        /// </summary>
        public event Action<string, double>? OnUploadProgress;
        
        /// <summary>
        /// Event raised during file download progress with path and percentage (0-100).
        /// </summary>
        public event Action<string, double>? OnDownloadProgress;
        
        /// <summary>
        /// Event raised during screenshot capture progress with percentage (0-100).
        /// </summary>
        public event Action<double>? OnScreenshotProgress;
        
        /// <summary>
        /// Event raised during server file push progress with path and percentage.
        /// </summary>
        public event Action<string, double>? OnServerFileProgress;
        
        /// <summary>
        /// Event raised when server completes pushing a file with the saved path.
        /// </summary>
        public event Action<string>? OnServerFileCompleted;
        
        /// <summary>
        /// Event raised when server file push encounters an error with path and error message.
        /// </summary>
        public event Action<string, string>? OnServerFileError;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnection"/> class.
        /// </summary>
        /// <param name="config">The gRPC configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public ClientConnection(GrpcConfig config, GrpcLogger logger)
        {
            _config = config;
            _logger = logger;
        }

        private Metadata BuildAuth() => new() { { InternalAuthInterceptor.MetadataKey, Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.Password)) } };

        public async Task ConnectAsync()
        {
            if (_channel != null) return;
            _channel = GrpcChannel.ForAddress($"http://{_config.BaseAddress}");
            _client = new RemoteChannel.RemoteChannelClient(_channel);
            _jsonStream = _client.JsonStream(headers: BuildAuth());
            _jsonDuplex = _client.JsonDuplex(headers: BuildAuth());
            _filePushStream = _client.FilePush(new Google.Protobuf.WellKnownTypes.Empty(), headers: BuildAuth());
            var startedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = Task.Run(async () =>
            {
                startedTcs.TrySetResult();
                await ReceiveJsonAsync();
            });
            _ = Task.Run(async () => await ReceiveDuplexJsonAsync());
            _ = Task.Run(async () => await ReceiveFilePushAsync());
            await startedTcs.Task.ConfigureAwait(false);
            _logger.Info("Client connected and JSON stream opened");
        }

        private async Task ReceiveJsonAsync()
        {
            if (_jsonStream == null) return;
            try
            {
                while (await _jsonStream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var ack = _jsonStream.ResponseStream.Current;
                    if (_pending.TryRemove(ack.Id, out var tcs)) tcs.TrySetResult(ack);
                    OnJsonAck?.Invoke(ack);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"JSON receive loop error: {ex.Message}");
            }
        }

        public async Task<JsonAck> SendJsonAsync(string type, string json, CancellationToken ct = default)
        {
            if (_jsonStream == null) throw new InvalidOperationException("Not connected");
            var id = Guid.NewGuid().ToString("N");
            var env = new JsonEnvelope { Id = id, Type = type, Json = json, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
            var tcs = new TaskCompletionSource<JsonAck>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[id] = tcs;
            await _jsonStream.RequestStream.WriteAsync(env);
            using var reg = ct.Register(() => tcs.TrySetCanceled());
            return await tcs.Task.ConfigureAwait(false);
        }

        public async Task SendJsonFireAndForgetAsync(string type, string json, CancellationToken ct = default)
        {
            if (_jsonDuplex == null) throw new InvalidOperationException("Not connected (duplex)");
            var env = new JsonEnvelope { Id = Guid.NewGuid().ToString("N"), Type = type, Json = json, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
            await _jsonDuplex.RequestStream.WriteAsync(env, ct);
        }

        private async Task ReceiveDuplexJsonAsync()
        {
            if (_jsonDuplex == null) return;
            try
            {
                while (await _jsonDuplex.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var msg = _jsonDuplex.ResponseStream.Current;
                    OnServerJson?.Invoke(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Duplex receive loop error: {ex.Message}");
            }
        }

        public async Task<FileTransferStatus> UploadFileAsync(string filePath, CancellationToken ct = default)
        {
            if (_client == null) throw new InvalidOperationException("Not connected");
            using var call = _client.UploadFile(headers: BuildAuth());
            var bytes = await File.ReadAllBytesAsync(filePath, ct);
            var chunkSize = _config.MaxChunkSizeBytes;
            var total = (int)Math.Ceiling((double)bytes.Length / chunkSize);
            for (int i = 0; i < total; i++)
            {
                ct.ThrowIfCancellationRequested();
                var slice = bytes.AsMemory(i * chunkSize, Math.Min(chunkSize, bytes.Length - i * chunkSize));
                await call.RequestStream.WriteAsync(new FileChunk
                {
                    Path = Path.GetFileName(filePath),
                    Data = Google.Protobuf.ByteString.CopyFrom(slice.Span),
                    Index = i,
                    TotalChunks = total,
                    IsLast = i == total - 1
                });
                OnUploadProgress?.Invoke(filePath, (i + 1) * 100.0 / total);
            }
            await call.RequestStream.CompleteAsync();
            return await call.ResponseAsync.ConfigureAwait(false);
        }

        public async Task<string[]> ListFilesAsync(string directory = "", CancellationToken ct = default)
        {
            if (_client == null) throw new InvalidOperationException("Not connected");
            try
            {
                var resp = await _client.ListFilesAsync(new ListFilesRequest { Directory = directory }, headers: BuildAuth(), cancellationToken: ct);
                return resp.Files.ToArray();
            }
            catch (Exception ex)
            {
                _logger.Error($"ListFilesAsync error: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        public async Task DownloadFileAsync(string remotePath, string localPath, CancellationToken ct = default)
        {
            if (_client == null) throw new InvalidOperationException("Not connected");
            var call = _client.DownloadFile(new FileRequest { Path = remotePath }, headers: BuildAuth(), cancellationToken: ct);
            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
            await using var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None);
            int totalChunks = -1; int received = 0;
            while (await call.ResponseStream.MoveNext(ct))
            {
                var chunk = call.ResponseStream.Current;
                if (!string.IsNullOrEmpty(chunk.Error)) throw new IOException(chunk.Error);
                if (chunk.Data.Length > 0) await fs.WriteAsync(chunk.Data.Memory, ct);
                totalChunks = totalChunks < 0 ? chunk.TotalChunks : totalChunks;
                received++;
                if (totalChunks > 0)
                    OnDownloadProgress?.Invoke(remotePath, Math.Min(100.0, received * 100.0 / totalChunks));
            }
        }

        public async Task<byte[]> GetScreenshotAsync(CancellationToken ct = default)
        {
            if (_client == null) throw new InvalidOperationException("Not connected");
            var call = _client.Screenshot(new ScreenshotRequest { MonitorIndex = -1 }, headers: BuildAuth(), cancellationToken: ct);
            using var ms = new MemoryStream();
            int total = -1; int received = 0;
            while (await call.ResponseStream.MoveNext(ct))
            {
                var chunk = call.ResponseStream.Current;
                if (!string.IsNullOrEmpty(chunk.Error)) throw new IOException(chunk.Error);
                if (chunk.Data.Length > 0) await ms.WriteAsync(chunk.Data.Memory, ct);
                total = total < 0 ? chunk.TotalChunks : total;
                received++;
                if (total > 0)
                    OnScreenshotProgress?.Invoke(Math.Min(100.0, received * 100.0 / total));
            }
            return ms.ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                // file push stream has no request side; nothing to Complete, just dispose
                _filePushStream?.Dispose();
                if (_jsonStream != null)
                {
                    await _jsonStream.RequestStream.CompleteAsync();
                    _jsonStream.Dispose();
                }
                if (_jsonDuplex != null)
                {
                    await _jsonDuplex.RequestStream.CompleteAsync();
                    _jsonDuplex.Dispose();
                }
                if (_channel != null)
                {
                    await _channel.ShutdownAsync();
                    _channel.Dispose();
                }
            }
            catch { }
        }

        private async Task ReceiveFilePushAsync()
        {
            if (_filePushStream == null) return;
            try
            {
                string? currentPath = null;
                MemoryStream? ms = null;
                int total = -1; int received = 0;
                while (await _filePushStream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var chunk = _filePushStream.ResponseStream.Current;
                    if (!string.IsNullOrEmpty(chunk.Error))
                    {
                        OnServerFileError?.Invoke(chunk.Path, chunk.Error);
                        currentPath = null;
                        ms?.Dispose();
                        ms = null;
                        continue;
                    }
                    if (currentPath == null)
                    {
                        currentPath = chunk.Path;
                        ms = new MemoryStream();
                        total = chunk.TotalChunks;
                        received = 0;
                    }
                    if (chunk.Data.Length > 0)
                        await ms!.WriteAsync(chunk.Data.Memory);
                    received++;
                    if (total > 0)
                        OnServerFileProgress?.Invoke(currentPath, Math.Min(100.0, received * 100.0 / total));
                    if (chunk.IsLast)
                    {
                        // Save to storage root
                        var saveDir = _config.StorageRoot;
                        Directory.CreateDirectory(saveDir);
                        var savePath = Path.Combine(saveDir, currentPath);
                        ms!.Seek(0, SeekOrigin.Begin);
                        await File.WriteAllBytesAsync(savePath, ms.ToArray());
                        OnServerFileCompleted?.Invoke(savePath);
                        ms.Dispose();
                        ms = null;
                        currentPath = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"File push stream error: {ex.Message}");
            }
        }
    }
}
