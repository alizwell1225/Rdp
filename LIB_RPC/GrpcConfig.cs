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
        public string LogFilePath { get; init; } = Path.Combine(AppContext.BaseDirectory, "rdp-grpc.log");

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
            var logDir = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrWhiteSpace(logDir)) Directory.CreateDirectory(logDir);
        }
    }
}
