using System.Drawing.Drawing2D;
using System.Text.Json;

namespace LIB_RPC
{
    public sealed class GrpcConfig
    {
        public string Host { get;  set; } = "localhost";
        public int Port { get;  set; } = 50051;
        public string BaseAddress => $"{Host}:{Port}";
        public string Password { get; private set; } = "changeme"; // simple shared secret metadata
        public int MaxChunkSizeBytes { get;  set; } = 64 * 1024; // 64KB default
        public string StorageRoot { get;  set; } = Path.Combine(AppContext.BaseDirectory, "storage");
        public bool EnableConsoleLog { get;  set; } = true;
        public string LogFilePath { get;  set; } = Path.Combine(AppContext.BaseDirectory, "Log", "grpc.log");

        /// <summary>
        /// Maximum number of log entries per file before rotation (default: 10000)
        /// </summary>
        public int MaxLogEntriesPerFile { get; init; } = 10000;
        
        /// <summary>
        /// Force abandon log writing on exception (default: false for safety)
        /// </summary>
        public bool ForceAbandonLogOnException { get; init; } = false;
        
        /// <summary>
        /// Maximum number of days to retain log files (default: 60 days)
        /// Logs older than this will be automatically deleted
        /// </summary>
        public int MaxLogRetentionDays { get; init; } = 60;
        
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
        public bool CheckStorageRootHaveFile { get; set; } = false;

        public static GrpcConfig Load(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return new GrpcConfig();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GrpcConfig>(json) ?? new GrpcConfig();
        }

        //save config to file
        public void Save(string path)
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                var folder = Directory.CreateDirectory(Path.HasExtension(path) ?Path.GetDirectoryName(path): path);
                File.WriteAllText(path, json);
            }
            catch (Exception)
            {

            }
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

        public void SaveHost(string? host)
        {
            Host = host ?? Host;
        }

        public void SavePort(int? port)
        {
            Port = port ?? Port;
        }
    }
}
