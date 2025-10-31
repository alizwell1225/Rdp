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
            _host = new ServerHost(_config, _logger!);
            _host.FileAdded += (s, path) => OnFileAdded?.Invoke(path);
            await _host.StartAsync();
            OnLog?.Invoke("Server started");
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