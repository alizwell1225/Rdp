using LIB_RPC;
using LIB_RPC.Abstractions;
using LIB_RPC.API;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LIB_Define.RPC
{
    /// <summary>
    /// Helper class that wraps gRPC server API for easy integration in WinForms applications.
    /// Decouples the gRPC server logic from UI concerns.
    /// </summary>
    public class GrpcServerHelper : IDisposable
    {
        private readonly IServerApi _serverApi;
        private readonly string _configPath;
        private bool _isDisposed;

        /// <summary>
        /// Gets the current server configuration.
        /// </summary>
        public GrpcConfig Config => _serverApi.Config;

        /// <summary>
        /// Gets whether the server is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        #region Events
        
        /// <summary>
        /// Event raised when a log message is generated.
        /// </summary>
        public event Action<string>? OnLog;

        /// <summary>
        /// Event raised when a file is added to the server storage.
        /// </summary>
        public event Action<string>? OnFileAdded;

        /// <summary>
        /// Event raised when server starts successfully.
        /// </summary>
        public event Action? OnServerStarted;

        /// <summary>
        /// Event raised when server stops.
        /// </summary>
        public event Action? OnServerStopped;

        /// <summary>
        /// Event raised when server startup fails.
        /// </summary>
        public event Action<string>? OnServerStartFailed;

        /// <summary>
        /// Event raised when a client connects to the server.
        /// </summary>
        public event Action<string>? OnClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the server.
        /// </summary>
        public event Action<string>? OnClientDisconnected;

        /// <summary>
        /// Event raised when file upload from client completes.
        /// </summary>
        public event Action<string>? OnFileUploadCompleted;

        #endregion

        /// <summary>
        /// Initializes a new instance of the GrpcServerHelper class.
        /// </summary>
        /// <param name="configPath">Optional path to configuration file. If null, uses default location.</param>
        public GrpcServerHelper(string? configPath = null)
        {
            _configPath = configPath ?? Path.Combine(AppContext.BaseDirectory, "Config", "ServerConfig.json");
            _serverApi = new GrpcServerApi();
            
            // Wire up events
            _serverApi.OnLog += (msg) => OnLog?.Invoke(msg);
            _serverApi.OnFileAdded += (path) => OnFileAdded?.Invoke(path);
            _serverApi.OnServerStarted += () => 
            { 
                IsRunning = true; 
                OnServerStarted?.Invoke(); 
            };
            _serverApi.OnServerStopped += () => 
            { 
                IsRunning = false; 
                OnServerStopped?.Invoke(); 
            };
            _serverApi.OnServerStartFailed += (err) => OnServerStartFailed?.Invoke(err);
            _serverApi.OnClientConnected += (clientId) => OnClientConnected?.Invoke(clientId);
            _serverApi.OnClientDisconnected += (clientId) => OnClientDisconnected?.Invoke(clientId);
            _serverApi.OnFileUploadCompleted += (fileName) => OnFileUploadCompleted?.Invoke(fileName);
        }

        /// <summary>
        /// Loads server configuration from file.
        /// </summary>
        /// <returns>True if loaded successfully, false otherwise.</returns>
        public bool LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var config = GrpcConfig.Load(_configPath);
                    _serverApi.UpdateConfig(config.Host, config.Port);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Failed to load config: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves current server configuration to file.
        /// </summary>
        /// <returns>True if saved successfully, false otherwise.</returns>
        public bool SaveConfig()
        {
            try
            {
                Config.Save(_configPath);
                return true;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Failed to save config: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates server configuration.
        /// </summary>
        /// <param name="host">Server host address.</param>
        /// <param name="port">Server port number.</param>
        public void UpdateConfig(string host, int port)
        {
            _serverApi.UpdateConfig(host, port);
        }

        /// <summary>
        /// Starts the gRPC server.
        /// </summary>
        public async Task<bool> StartServerAsync()
        {
            try
            {
                await _serverApi.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Failed to start server: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stops the gRPC server.
        /// </summary>
        public async Task<bool> StopServerAsync()
        {
            try
            {
                await _serverApi.StopAsync();
                return true;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Failed to stop server: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Broadcasts a JSON message to all connected clients.
        /// </summary>
        /// <param name="messageType">Type/category of the message.</param>
        /// <param name="jsonBody">JSON content to broadcast.</param>
        /// <param name="useAckMode">Whether to use acknowledgment mode for reliable delivery.</param>
        /// <param name="retryCount">Number of retries for ACK mode (default: 0).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Tuple indicating success, number of clients reached, and error message if any.</returns>
        public async Task<(bool Success, int ClientsReached, string Error)> BroadcastJsonAsync(
            string messageType, 
            string jsonBody, 
            bool useAckMode = false,
            int retryCount = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (useAckMode)
                {
                    return await _serverApi.BroadcastWithAckAsync(messageType, jsonBody, retryCount, cancellationToken);
                }
                else
                {
                    await _serverApi.BroadcastJsonAsync(messageType, jsonBody);
                    return (true, -1, string.Empty); // -1 indicates no ACK mode
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Broadcast failed: {ex.Message}";
                OnLog?.Invoke(errorMsg);
                return (false, 0, errorMsg);
            }
        }

        /// <summary>
        /// Pushes a file to all connected clients.
        /// </summary>
        /// <param name="filePath">Full path to the file to push.</param>
        /// <param name="useAckMode">Whether to use acknowledgment mode for reliable delivery.</param>
        /// <param name="retryCount">Number of retries for ACK mode (default: 0).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Tuple indicating success, number of clients reached, and error message if any.</returns>
        public async Task<(bool Success, int ClientsReached, string Error)> PushFileAsync(
            string filePath, 
            bool useAckMode = false,
            int retryCount = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return (false, 0, "File does not exist");
                }

                if (useAckMode)
                {
                    return await _serverApi.PushFileWithAckAsync(filePath, retryCount, cancellationToken);
                }
                else
                {
                    await _serverApi.PushFileAsync(filePath);
                    return (true, -1, string.Empty); // -1 indicates no ACK mode
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Push file failed: {ex.Message}";
                OnLog?.Invoke(errorMsg);
                return (false, 0, errorMsg);
            }
        }

        /// <summary>
        /// Gets the list of files in the server storage directory.
        /// </summary>
        /// <returns>Array of file names.</returns>
        public string[] GetStorageFiles()
        {
            try
            {
                return _serverApi.GetFiles();
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Failed to get files: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Ensures all required directories exist.
        /// </summary>
        public void EnsureDirectories()
        {
            Config.EnsureFolders();
        }

        /// <summary>
        /// Broadcasts an image to all connected clients by file path.
        /// The image is loaded from the file, converted to base64, and sent to clients.
        /// Clients will receive this via ActionOnServerImage event with the actual image data.
        /// </summary>
        /// <param name="pictureType">Type of picture (Flow, Map, etc.)</param>
        /// <param name="imagePath">Full path to the image file.</param>
        /// <param name="useAckMode">Whether to use acknowledgment mode for reliable delivery.</param>
        /// <param name="retryCount">Number of retries for ACK mode (default: 0).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Tuple indicating success, number of clients reached, and error message if any.</returns>
        public async Task<(bool Success, int ClientsReached, string Error)> BroadcastImageByPathAsync(
            ShowPictureType pictureType,
            string imagePath,
            bool useAckMode = true,
            int retryCount = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    return (false, 0, "Image file not found");
                }

                // Load image from file and convert to base64
                using var image = Image.FromFile(imagePath);
                using var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var base64 = Convert.ToBase64String(ms.ToArray());

                var imageMsg = new ImageTransferMessage
                {
                    PictureType = pictureType,
                    ImageDataBase64 = base64,  // Send actual image data, not path
                    FileName = Path.GetFileName(imagePath)
                };

                var json = JsonSerializer.Serialize(imageMsg, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                var result = await BroadcastJsonAsync("image", json, useAckMode, retryCount, cancellationToken);
                
                if (result.Success)
                {
                    OnLog?.Invoke($"Broadcast image: {Path.GetFileName(imagePath)} (Type: {pictureType}, Size: {image.Width}x{image.Height})");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Broadcast image failed: {ex.Message}";
                OnLog?.Invoke(errorMsg);
                return (false, 0, errorMsg);
            }
        }

        /// <summary>
        /// Broadcasts an image to all connected clients by Image object.
        /// Clients will receive this via ActionOnServerImage event.
        /// </summary>
        /// <param name="pictureType">Type of picture (Flow, Map, etc.)</param>
        /// <param name="image">Image object to send.</param>
        /// <param name="fileName">Optional file name (default: image.png).</param>
        /// <param name="useAckMode">Whether to use acknowledgment mode for reliable delivery.</param>
        /// <param name="retryCount">Number of retries for ACK mode (default: 0).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Tuple indicating success, number of clients reached, and error message if any.</returns>
        public async Task<(bool Success, int ClientsReached, string Error)> BroadcastImageAsync(
            ShowPictureType pictureType,
            Image image,
            string fileName = "image.png",
            bool useAckMode = true,
            int retryCount = 0,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Convert image to base64
                using var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var base64 = Convert.ToBase64String(ms.ToArray());

                var imageMsg = new ImageTransferMessage
                {
                    PictureType = pictureType,
                    ImageDataBase64 = base64,
                    FileName = fileName
                };

                var json = JsonSerializer.Serialize(imageMsg, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                var result = await BroadcastJsonAsync("image", json, useAckMode, retryCount, cancellationToken);
                
                if (result.Success)
                {
                    OnLog?.Invoke($"Broadcast image: {fileName} (Type: {pictureType}, Size: {image.Width}x{image.Height})");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Broadcast image failed: {ex.Message}";
                OnLog?.Invoke(errorMsg);
                return (false, 0, errorMsg);
            }
        }

        /// <summary>
        /// Disposes of resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            
            if (IsRunning)
            {
                StopServerAsync().GetAwaiter().GetResult();
            }
            
            if (_serverApi is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
