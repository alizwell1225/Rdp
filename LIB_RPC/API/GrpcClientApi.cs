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

        public event Action<string>? OnLog;
        public event Action<string, double>? OnUploadProgress;
        public event Action<string, double>? OnDownloadProgress;
        public event Action<double>? OnScreenshotProgress;
        public event Action<string>? OnServerFileCompleted;
        public event Action<string, string>? OnServerFileError;
        public event Action<JsonMessage>? OnServerJson;

        public GrpcClientApi(GrpcConfig? config = null)
        {
            _config = config ?? new GrpcConfig();
            _logger = new GrpcLogger(_config);
            _logger.OnLine += line => OnLog?.Invoke(line);
        }

        public GrpcConfig Config => _config;

        public void UpdateConfig(GrpcConfig config)
        {
            _config = config;
        }

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

        public async Task DisconnectAsync()
        {
            if (_conn == null) return;
            await _conn.DisposeAsync();
            _conn = null;
        }

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

        public async Task SendJsonFireAndForgetAsync(string type, string json, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            await _conn.SendJsonFireAndForgetAsync(type, json, ct);
        }

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

        public async Task DownloadFileAsync(string remotePath, string localPath, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            await _conn.DownloadFileAsync(remotePath, localPath, ct);
        }

        public async Task<string[]> ListFilesAsync(string directory = "", CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            return await _conn.ListFilesAsync(directory, ct);
        }

        public async Task<byte[]> GetScreenshotAsync(CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            return await _conn.GetScreenshotAsync(ct);
        }

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
