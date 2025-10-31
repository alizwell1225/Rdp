namespace LIB_RPC.API
{
    public class GrpcServerApi : IAsyncDisposable
    {
        private ServerHost? _host;
        private GrpcLogger? _logger;
        private GrpcConfig _config;

        public event Action<string>? OnLog;
        public event Action<string>? OnFileAdded;

        public GrpcServerApi()
        {
            _config = new GrpcConfig();
            _logger = new GrpcLogger(_config);
            _logger.OnLine += line => OnLog?.Invoke(line);
        }

        public GrpcConfig Config => _config;

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

        public async Task StartAsync()
        {
            if (_host != null) return;
            _host = new ServerHost(_config, _logger!);
            _host.FileAdded += (s, path) => OnFileAdded?.Invoke(path);
            await _host.StartAsync();
            OnLog?.Invoke("Server started");
        }

        public async Task StopAsync()
        {
            if (_host == null) return;
            await _host.StopAsync();
            _host = null;
            OnLog?.Invoke("Server stopped");
        }

        public async Task BroadcastJsonAsync(string type, string body)
        {
            if (_host == null) throw new InvalidOperationException("Host not started");
            await _host.BroadcastJsonAsync(type, body);
        }

        public async Task PushFileAsync(string path)
        {
            if (_host == null) throw new InvalidOperationException("Host not started");
            await _host.PushFileAsync(path);
        }

        public string[] GetFiles()
        {
            Directory.CreateDirectory(_config.StorageRoot);
            return Directory.EnumerateFiles(_config.StorageRoot).Select(Path.GetFileName).Where(n => n != null)
                .ToArray()!;
        }

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