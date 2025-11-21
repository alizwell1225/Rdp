using LIB_RPC.Abstractions;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using RdpGrpc.Proto;

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
        /// 日誌訊息事件
        /// Event raised when a log line is produced.
        /// </summary>
        public event Action<string>? OnLog;
        
        /// <summary>
        /// 檔案上傳進度事件（客戶端上傳到伺服器）
        /// Event raised during file upload progress (client uploading to server).
        /// </summary>
        public event Action<string, double>? OnUploadProgress;
        
        /// <summary>
        /// 檔案下載進度事件（從伺服器下載到客戶端）
        /// Event raised during file download progress (downloading from server to client).
        /// </summary>
        public event Action<string, double>? OnDownloadProgress;
        
        /// <summary>
        /// 螢幕截圖擷取進度事件
        /// Event raised during screenshot capture progress.
        /// </summary>
        public event Action<double>? OnScreenshotProgress;


        /// <summary>
        /// 從伺服器接收檔案完成事件
        /// Event raised when receiving file from server is completed.
        /// </summary>
        public event Action<string>? OnReceivedFileFromServer;
        
        /// <summary>
        /// 從伺服器接收檔案錯誤事件
        /// Event raised when receiving file from server encounters an error.
        /// </summary>
        public event Action<string, string>? OnReceivedFileErrorFromServer;
        
        /// <summary>
        /// 從伺服器接收 JSON 訊息事件
        /// Event raised when receiving JSON message from server.
        /// </summary>
        public event Action<JsonMessage>? OnReceivedJsonMessageFromServer;

        /// <summary>
        /// 從伺服器接收位元組資料事件（圖片、檔案等）
        /// Event raised when receiving byte data from server (images, files, etc.).
        /// </summary>
        public event Action<string, byte[], string?>? OnReceivedByteDataFromServer;

        /// <summary>
        /// Event raised during byte transfer progress (type, bytesTransferred, totalBytes, percentage).
        /// </summary>
        public event Action<string, long, long, double>? OnByteTransferProgress;

        /// <summary>
        /// Event raised when connection to server is established successfully.
        /// </summary>
        public event Action? OnConnected;

        /// <summary>
        /// Event raised when disconnected from server.
        /// </summary>
        public event Action? OnDisconnected;

        /// <summary>
        /// Event raised when connection attempt fails.
        /// </summary>
        public event Action<string>? OnConnectionError;

        /// <summary>
        /// Event raised when file upload starts.
        /// </summary>
        public event Action<string>? OnUploadStarted;

        /// <summary>
        /// Event raised when file upload completes successfully.
        /// </summary>
        public event Action<string>? OnUploadCompleted;

        /// <summary>
        /// Event raised when file upload fails.
        /// </summary>
        public event Action<string, string>? OnUploadFailed;

        /// <summary>
        /// Event raised when file download starts.
        /// </summary>
        public event Action<string>? OnDownloadStarted;

        /// <summary>
        /// Event raised when file download completes successfully.
        /// </summary>
        public event Action<string>? OnDownloadCompleted;

        /// <summary>
        /// Event raised when file download fails.
        /// </summary>
        public event Action<string, string>? OnDownloadFailed;

        /// <summary>
        /// Event raised when screenshot capture starts.
        /// </summary>
        public event Action? OnScreenshotStarted;

        /// <summary>
        /// Event raised when screenshot capture completes successfully.
        /// </summary>
        public event Action<int>? OnScreenshotCompleted;

        /// <summary>
        /// Event raised when screenshot capture fails.
        /// </summary>
        public event Action<string>? OnScreenshotFailed;

        /// <summary>
        /// Event raised when server file push starts.
        /// </summary>
        public event Action<string>? OnServerFileStarted;

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
        public async Task ConnectAsync(bool re = false, CancellationToken ct = default)
        {
            if (_conn != null && re == false)
            {
                return;
            }

            try
            {
                _conn = new ClientConnection(_config, _logger);
                // forward events
                _conn.OnUploadProgress += (p, pct) => OnUploadProgress?.Invoke(p, pct);
                _conn.OnDownloadProgress += (p, pct) => OnDownloadProgress?.Invoke(p, pct);
                _conn.OnScreenshotProgress += pct => OnScreenshotProgress?.Invoke(pct);
                _conn.OnServerFileCompleted += p => OnReceivedFileFromServer?.Invoke(p);
                _conn.OnServerFileError += (p, e) => OnReceivedFileErrorFromServer?.Invoke(p, e);
                _conn.OnServerFileProgress += (p, pct) => OnServerFileStarted?.Invoke(p);
                _conn.OnServerJson += env => OnReceivedJsonMessageFromServer?.Invoke(new JsonMessage
                {
                    Id = env.Id,
                    Type = env.Type,
                    Json = env.Json,
                    Timestamp = env.Timestamp
                });
                _conn.OnServerByteData += (type, data, metadata) => OnReceivedByteDataFromServer?.Invoke(type, data, metadata);
                _conn.OnConnected += ConnOnConnected;


                await _conn.ConnectAsync();
                _logger.Info("Client connected successfully");
            }
            catch (Exception ex)
            {
                _conn = null;
                OnConnectionError?.Invoke(ex.Message);
                _logger.Error($"Connection failed: {ex.Message}");
                throw;
            }
        }

        bool IsConnected = false;
        private void ConnOnConnected(bool flag)
        {
            //if (IsConnected == flag) return;
            IsConnected = flag;
            if (flag)
                OnConnected?.Invoke();
            else
                OnDisconnected?.Invoke();
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
            OnDisconnected?.Invoke();
            _logger.Info("Client disconnected");
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
            
            try
            {
                OnUploadStarted?.Invoke(filePath);
                _logger.Info($"Starting upload: {filePath}");
                
                var status = await _conn.UploadFileAsync(filePath, ct);
                
                var result = new FileTransferResult
                {
                    Path = status.Path,
                    Success = status.Success,
                    Error = status.Error
                };
                
                if (result.Success)
                {
                    OnUploadCompleted?.Invoke(filePath);
                    _logger.Info($"Upload completed: {filePath}");
                }
                else
                {
                    OnUploadFailed?.Invoke(filePath, result.Error);
                    _logger.Error($"Upload failed: {filePath} - {result.Error}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                OnUploadFailed?.Invoke(filePath, ex.Message);
                _logger.Error($"Upload exception: {filePath} - {ex.Message}");
                throw;
            }
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
            
            try
            {
                OnDownloadStarted?.Invoke(remotePath);
                _logger.Info($"Starting download: {remotePath} -> {localPath}");
                
                await _conn.DownloadFileAsync(remotePath, localPath, ct);
                
                OnDownloadCompleted?.Invoke(remotePath);
                _logger.Info($"Download completed: {remotePath}");
            }
            catch (Exception ex)
            {
                OnDownloadFailed?.Invoke(remotePath, ex.Message);
                _logger.Error($"Download failed: {remotePath} - {ex.Message}");
                throw;
            }
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
            
            try
            {
                OnScreenshotStarted?.Invoke();
                _logger.Info("Starting screenshot capture");
                
                var bytes = await _conn.GetScreenshotAsync(ct);
                
                OnScreenshotCompleted?.Invoke(bytes.Length);
                _logger.Info($"Screenshot completed: {bytes?.Length} bytes");
                
                return bytes;
            }
            catch (Exception ex)
            {
                OnScreenshotFailed?.Invoke(ex.Message);
                _logger.Error($"Screenshot failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends byte data to the server with acknowledgment.
        /// </summary>
        public async Task<bool> SendByteAsync(string type, byte[] data, string? metadata = null, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            
            try
            {
                var result = await _conn.SendByteAsync(type, data, metadata, ct);
                _logger.Info($"SendByte completed: type={type}, size={data.Length}, success={result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"SendByte failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends byte data to the server without acknowledgment (fire and forget).
        /// </summary>
        public async Task SendByteNoAckAsync(string type, byte[] data, string? metadata = null, CancellationToken ct = default)
        {
            if (_conn == null) throw new InvalidOperationException("Not connected");
            
            try
            {
                await _conn.SendByteNoAckAsync(type, data, metadata, ct);
                _logger.Info($"SendByteNoAck: type={type}, size={data.Length}");
            }
            catch (Exception ex)
            {
                _logger.Error($"SendByteNoAck failed: {ex.Message}");
                throw;
            }
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
