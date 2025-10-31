using System;
using System.Threading;
using System.Threading.Tasks;
using LIB_RPC.Abstractions;

namespace LIB_RPC.API
{
    /// <summary>
    /// Lightweight API wrapper around ClientConnection + GrpcConfig + GrpcLogger
    /// Exposes connect/upload/download/list/send functionality and forwards progress/log events.
    /// </summary>
    public sealed class GrpcClientApi : IClientApi
    {
        private GrpcConfig _config;
        private readonly GrpcLogger _logger;
        private ClientConnection? _conn;

        /// <summary>
        /// Event raised when a log line is produced.
        /// </summary>
        public event Action<string>? OnLog;
        
        /// <summary>
        /// Event raised during file upload progress.
        /// </summary>
        public event Action<string, double>? OnUploadProgress;
        
        /// <summary>
        /// Event raised during file download progress.
        /// </summary>
        public event Action<string, double>? OnDownloadProgress;
        
        /// <summary>
        /// Event raised during screenshot capture progress.
        /// </summary>
        public event Action<double>? OnScreenshotProgress;
        
        /// <summary>
        /// Event raised when server completes pushing a file.
        /// </summary>
        public event Action<string>? OnServerFileCompleted;
        
        /// <summary>
        /// Event raised when server file push encounters an error.
        /// </summary>
        public event Action<string, string>? OnServerFileError;
        
        /// <summary>
        /// Event raised when server pushes a JSON message.
        /// </summary>
        public event Action<JsonMessage>? OnServerJson;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcClientApi"/> class.
        /// </summary>
        /// <param name="config">Optional configuration. If null, default configuration is used.</param>
        public GrpcClientApi(GrpcConfig? config = null)
        {
            _config = config ?? new GrpcConfig();
            _logger = new GrpcLogger(_config);
            _logger.OnLine += line => OnLog?.Invoke(line);
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public GrpcConfig Config => _config;

        /// <summary>
        /// Updates the configuration with a new instance.
        /// </summary>
        /// <param name="config">The new configuration.</param>
        public void UpdateConfig(GrpcConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Connects to the gRPC server asynchronously.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            if (_conn != null) return;
            _conn = new ClientConnection(_config, _logger);
            // forward events
            _conn.OnUploadProgress += (p, pct) => OnUploadProgress?.Invoke(p, pct);
            _conn.OnDownloadProgress += (p, pct) => OnDownloadProgress?.Invoke(p, pct);
            _conn.OnScreenshotProgress += pct => OnScreenshotProgress?.Invoke(pct);
            _conn.OnServerFileCompleted += p => OnServerFileCompleted?.Invoke(p);
            _conn.OnServerFileError += (p, e) => OnServerFileError?.Invoke(p, e);
            _conn.OnServerJson += env => OnServerJson?.Invoke(new JsonMessage
            {
                Id = env.Id,
                Type = env.Type,
                Json = env.Json,
                Timestamp = env.Timestamp
            });
            await _conn.ConnectAsync();
        }

        /// <summary>
        /// Disconnects from the gRPC server asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DisconnectAsync()
        {
            if (_conn == null) return;
            await _conn.DisposeAsync();
            _conn = null;
        }

        /// <summary>
        /// Sends a JSON message and waits for acknowledgment.
        /// </summary>
        /// <param name="type">The message type/category.</param>
        /// <param name="json">The JSON payload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with acknowledgment result.</returns>
        public async Task<JsonAcknowledgment> SendJsonAsync(string type, string json, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            var ack = await _conn.SendJsonAsync(type, json, ct);
            return new JsonAcknowledgment
            {
                Id = ack.Id,
                Success = ack.Success,
                Error = ack.Error
            };
        }

        /// <summary>
        /// Sends a JSON message without waiting for acknowledgment (fire and forget).
        /// </summary>
        /// <param name="type">The message type/category.</param>
        /// <param name="json">The JSON payload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendJsonFireAndForgetAsync(string type, string json, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            await _conn.SendJsonFireAndForgetAsync(type, json, ct);
        }

        /// <summary>
        /// Uploads a file to the server.
        /// </summary>
        /// <param name="filePath">The path to the file to upload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with transfer result.</returns>
        public async Task<FileTransferResult> UploadFileAsync(string filePath, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            var status = await _conn.UploadFileAsync(filePath, ct);
            return new FileTransferResult
            {
                Path = status.Path,
                Success = status.Success,
                Error = status.Error
            };
        }

        /// <summary>
        /// Downloads a file from the server.
        /// </summary>
        /// <param name="remotePath">The remote file path on the server.</param>
        /// <param name="localPath">The local path to save the file.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DownloadFileAsync(string remotePath, string localPath, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            await _conn.DownloadFileAsync(remotePath, localPath, ct);
        }

        /// <summary>
        /// Lists available files on the server.
        /// </summary>
        /// <param name="directory">Optional directory path relative to storage root.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with array of file names.</returns>
        public async Task<string[]> ListFilesAsync(string directory = "", CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            return await _conn.ListFilesAsync(directory, ct);
        }

        /// <summary>
        /// Gets a screenshot from the server as PNG bytes.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with PNG image bytes.</returns>
        public async Task<byte[]> GetScreenshotAsync(CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            return await _conn.GetScreenshotAsync(ct);
        }

        /// <summary>
        /// Disposes the client resources asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disposal operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_conn != null)
            {
                await _conn.DisposeAsync();
                _conn = null;
            }
        }
    }
}
