using LIB_RPC.Abstractions;

namespace LIB_RPC.API
{
    /// <summary>
    /// Server API implementation for gRPC remote channel service.
    /// Provides methods to start/stop server, broadcast messages, and push files to clients.
    /// </summary>
    public class GrpcServerApi : IServerApi
    {
        private ServerHost? _host;
        private GrpcLogger? _logger;
        private GrpcConfig _config;

        /// <summary>
        /// Event raised when a log line is produced.
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
        /// Event raised when file upload from client starts.
        /// </summary>
        public event Action<string>? OnFileUploadStarted;

        /// <summary>
        /// Event raised when file upload from client completes successfully.
        /// </summary>
        public event Action<string>? OnFileUploadCompleted;

        /// <summary>
        /// Event raised when file upload from client fails.
        /// </summary>
        public event Action<string, string>? OnFileUploadFailed;

        /// <summary>
        /// Event raised when server starts pushing a file to clients.
        /// </summary>
        public event Action<string>? OnFilePushStarted;

        /// <summary>
        /// Event raised when server completes pushing a file to all clients.
        /// </summary>
        public event Action<string>? OnFilePushCompleted;

        /// <summary>
        /// Event raised when server fails to push file to one or more clients.
        /// </summary>
        public event Action<string, string>? OnFilePushFailed;

        /// <summary>
        /// Event raised when broadcast message is sent successfully.
        /// </summary>
        public event Action<string, int>? OnBroadcastSent;

        /// <summary>
        /// Event raised when broadcast message fails.
        /// </summary>
        public event Action<string, string>? OnBroadcastFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcServerApi"/> class with default configuration.
        /// </summary>
        public GrpcServerApi()
        {
            _config = new GrpcConfig();
            _logger = new GrpcLogger(_config);
            _logger.OnLine += line => OnLog?.Invoke(line);
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public GrpcConfig Config => _config;

        /// <summary>
        /// Updates the server configuration with new host and port.
        /// </summary>
        /// <param name="host">The host address to bind to.</param>
        /// <param name="port">The port number to listen on.</param>
        public void UpdateConfig(string? host, int? port)
        {
            // GrpcConfig properties are init-only, so create a new instance
            var newConfig = new GrpcConfig
            {
                Host = !string.IsNullOrWhiteSpace(host) ? host!.Trim() : _config.Host,
                Port = port ?? _config.Port,
                Password = _config.Password,
                MaxChunkSizeBytes = _config.MaxChunkSizeBytes,
                StorageRoot = _config.StorageRoot,
                EnableConsoleLog = _config.EnableConsoleLog,
                LogFilePath = _config.LogFilePath
            };

            // Replace config and recreate logger so it uses the updated paths/settings
            _config = newConfig;
            try
            {
                _logger?.Dispose();
            }
            catch
            {
            }

            _logger = new GrpcLogger(_config);
            _logger.OnLine += line => OnLog?.Invoke(line);
        }

        /// <summary>
        /// Starts the gRPC server asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartAsync()
        {
            if (_host != null) return;
            
            try
            {
                _host = new ServerHost(_config, _logger!);
                _host.FileAdded += (s, path) => OnFileAdded?.Invoke(path);
                _host.FileUploadStarted += (s, path) => OnFileUploadStarted?.Invoke(path);
                _host.FileUploadCompleted += (s, path) => OnFileUploadCompleted?.Invoke(path);
                _host.FileUploadFailed += (s, args) => OnFileUploadFailed?.Invoke(args.Item1, args.Item2);
                _host.FilePushStarted += (s, path) => OnFilePushStarted?.Invoke(path);
                _host.FilePushCompleted += (s, path) => OnFilePushCompleted?.Invoke(path);
                _host.FilePushFailed += (s, args) => OnFilePushFailed?.Invoke(args.Item1, args.Item2);
                _host.BroadcastSent += (s, args) => OnBroadcastSent?.Invoke(args.Item1, args.Item2);
                _host.BroadcastFailed += (s, args) => OnBroadcastFailed?.Invoke(args.Item1, args.Item2);
                _host.ClientConnected += (s, clientId) => OnClientConnected?.Invoke(clientId);
                _host.ClientDisconnected += (s, clientId) => OnClientDisconnected?.Invoke(clientId);
                
                await _host.StartAsync();
                OnServerStarted?.Invoke();
                OnLog?.Invoke("Server started successfully");
            }
            catch (Exception ex)
            {
                OnServerStartFailed?.Invoke(ex.Message);
                OnLog?.Invoke($"Server start failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stops the gRPC server asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StopAsync()
        {
            if (_host == null) return;
            await _host.StopAsync();
            _host = null;
            OnServerStopped?.Invoke();
            OnLog?.Invoke("Server stopped");
        }

        /// <summary>
        /// Broadcasts a JSON message to all connected clients.
        /// </summary>
        /// <param name="type">The message type/category.</param>
        /// <param name="body">The JSON message body.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task BroadcastJsonAsync(string type, string body)
        {
            if (_host == null) throw new InvalidOperationException("Host not started");
            await _host.BroadcastJsonAsync(type, body);
        }

        /// <summary>
        /// Broadcasts a JSON message to all connected clients with acknowledgment and retry support.
        /// </summary>
        /// <param name="type">The message type/category.</param>
        /// <param name="body">The JSON message body.</param>
        /// <param name="retryCount">Number of retries (0 = no retry).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Tuple with success status, number of clients reached, and error message.</returns>
        public async Task<(bool Success, int ClientsReached, string Error)> BroadcastWithAckAsync(string type, string body, int retryCount = 0, CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Host not started");
            return await _host.BroadcastWithAckAsync(type, body, retryCount, ct);
        }

        /// <summary>
        /// Pushes a file to all connected clients.
        /// </summary>
        /// <param name="path">The path to the file to push.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PushFileAsync(string path)
        {
            if (_host == null) throw new InvalidOperationException("Host not started");
            await _host.PushFileAsync(path);
        }

        /// <summary>
        /// Pushes a file to all connected clients with acknowledgment and retry support.
        /// </summary>
        /// <param name="path">The path to the file to push.</param>
        /// <param name="retryCount">Number of retries (0 = no retry).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Tuple with success status, number of clients reached, and error message.</returns>
        public async Task<(bool Success, int ClientsReached, string Error)> PushFileWithAckAsync(string path, int retryCount = 0, CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Host not started");
            return await _host.PushFileWithAckAsync(path, retryCount, ct);
        }

        /// <summary>
        /// Gets the list of files in the server storage.
        /// </summary>
        /// <returns>Array of file names.</returns>
        public string[] GetFiles()
        {
            Directory.CreateDirectory(_config.StorageRoot);
            return Directory.EnumerateFiles(_config.StorageRoot).Select(Path.GetFileName).Where(n => n != null)
                .ToArray()!;
        }

        /// <summary>
        /// Disposes the server resources asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disposal operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host = null;
            }
        }
    }
}