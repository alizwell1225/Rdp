using System.Text.Json;

namespace LIB_RPC
{
    /// <summary>
    /// Builder for constructing GrpcConfig with fluent API and validation.
    /// </summary>
    public sealed class GrpcConfigBuilder
    {
        private string _host = "localhost";
        private int _port = 50051;
        private string _password = "changeme";
        private int _maxChunkSizeBytes = 64 * 1024;
        private string _storageRoot = Path.Combine(AppContext.BaseDirectory, "storage");
        private bool _enableConsoleLog = true;
        private string _logFilePath = Path.Combine(AppContext.BaseDirectory, "rdp-grpc.log");

        /// <summary>
        /// Sets the host address.
        /// </summary>
        public GrpcConfigBuilder WithHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host cannot be null or whitespace.", nameof(host));
            _host = host.Trim();
            return this;
        }

        /// <summary>
        /// Sets the port number.
        /// </summary>
        public GrpcConfigBuilder WithPort(int port)
        {
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
            _port = port;
            return this;
        }

        /// <summary>
        /// Sets the authentication password.
        /// </summary>
        public GrpcConfigBuilder WithPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
            _password = password;
            return this;
        }

        /// <summary>
        /// Sets the maximum chunk size for file transfers.
        /// </summary>
        public GrpcConfigBuilder WithMaxChunkSize(int bytes)
        {
            if (bytes < 1024 || bytes > 10 * 1024 * 1024)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Chunk size must be between 1KB and 10MB.");
            _maxChunkSizeBytes = bytes;
            return this;
        }

        /// <summary>
        /// Sets the storage root directory.
        /// </summary>
        public GrpcConfigBuilder WithStorageRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Storage root cannot be null or whitespace.", nameof(path));
            _storageRoot = path;
            return this;
        }

        /// <summary>
        /// Enables or disables console logging.
        /// </summary>
        public GrpcConfigBuilder WithConsoleLog(bool enable)
        {
            _enableConsoleLog = enable;
            return this;
        }

        /// <summary>
        /// Sets the log file path.
        /// </summary>
        public GrpcConfigBuilder WithLogFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Log file path cannot be null or whitespace.", nameof(path));
            _logFilePath = path;
            return this;
        }

        /// <summary>
        /// Loads configuration from a JSON file.
        /// Returns the builder unchanged if the file does not exist.
        /// </summary>
        public GrpcConfigBuilder FromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return this;
                
            if (!File.Exists(filePath))
            {
                // Log or notify that file is missing - consider logging in production
                return this;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<GrpcConfig>(json);
                if (config != null)
                {
                    _host = config.Host;
                    _port = config.Port;
                    _password = config.Password;
                    _maxChunkSizeBytes = config.MaxChunkSizeBytes;
                    _storageRoot = config.StorageRoot;
                    _enableConsoleLog = config.EnableConsoleLog;
                    _logFilePath = config.LogFilePath;
                }
            }
            catch (Exception)
            {
                // Silently continue with existing values on error
                // Consider logging or throwing in production scenarios
            }
            
            return this;
        }

        ///// <summary>
        ///// Builds the GrpcConfig instance.
        ///// </summary>
        //public GrpcConfig Build()
        //{
        //    return new GrpcConfig
        //    {
        //        Host = _host,
        //        Port = _port,
        //        Password = _password,
        //        MaxChunkSizeBytes = _maxChunkSizeBytes,
        //        StorageRoot = _storageRoot,
        //        EnableConsoleLog = _enableConsoleLog,
        //        LogFilePath = _logFilePath
        //    };
        //}
    }
}
