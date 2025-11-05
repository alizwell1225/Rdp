using System.Text.Json;

namespace LIB_RPC
{
    public sealed class GrpcConfig
    {
        public string Host { get; init; } = "localhost";
        public int Port { get; init; } = 50051;
        public string BaseAddress => $"{Host}:{Port}";
        public string Password { get; init; } = "changeme"; // simple shared secret metadata
        public int MaxChunkSizeBytes { get; init; } = 64 * 1024; // 64KB default
        public string StorageRoot { get; init; } = Path.Combine(AppContext.BaseDirectory, "storage");
        public bool EnableConsoleLog { get; init; } = true;
        public string LogFilePath { get; init; } = Path.Combine(AppContext.BaseDirectory, "Log", "grpc.log");

        /// <summary>
        /// Maximum number of log entries per file before rotation (default: 20000)
        /// </summary>
        public int MaxLogEntriesPerFile { get; init; } = 10;
        
        /// <summary>
        /// Force abandon log writing on exception (default: false for safety)
        /// </summary>
        public bool ForceAbandonLogOnException { get; init; } = false;
        
        /// <summary>
        /// Auto-delete received files after successful processing (default: false for safety)
        /// </summary>
        public bool AutoDeleteReceivedFiles { get; init; } = false;
        
        /// <summary>
        /// Custom download path for client received files (if null, uses StorageRoot)
        /// </summary>
        public string? ClientDownloadPath { get; init; } = null;
        
        /// <summary>
        /// Custom upload path for server received files (if null, uses StorageRoot)
        /// </summary>
        public string? ServerUploadPath { get; init; } = null;

        public static GrpcConfig Load(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return new GrpcConfig();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GrpcConfig>(json) ?? new GrpcConfig();
        }

        public void EnsureFolders()
        {
            Directory.CreateDirectory(StorageRoot);
            if (!string.IsNullOrWhiteSpace(ClientDownloadPath))
                Directory.CreateDirectory(ClientDownloadPath);
            if (!string.IsNullOrWhiteSpace(ServerUploadPath))
                Directory.CreateDirectory(ServerUploadPath);
            var logDir = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrWhiteSpace(logDir)) Directory.CreateDirectory(logDir);
        }
        
        /// <summary>
        /// Get the effective client download directory
        /// </summary>
        public string GetClientDownloadPath() => ClientDownloadPath ?? StorageRoot;
        
        /// <summary>
        /// Get the effective server upload directory
        /// </summary>
        public string GetServerUploadPath() => ServerUploadPath ?? StorageRoot;
    }
}
