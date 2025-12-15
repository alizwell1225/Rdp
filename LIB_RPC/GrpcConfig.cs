using System.Text.Json;
using System.Text.Json.Serialization;

namespace LIB_RPC;

public sealed class GrpcConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 50051;
    public string BaseAddress => $"{Host}:{Port}";

    [JsonIgnore] private string Password { get; set; } = "changeme"; // simple shared secret metadata

    public int MaxChunkSizeBytes { get; set; } = 100 * 1024 * 1024; // 100MB default
    public bool EnableConsoleLog { get; set; } = true;
    public string StorageRoot { get; set; } = Path.Combine(AppContext.BaseDirectory, "Storage"); //基本預設儲存空間
    public string LogFilePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "Log", "grpc.log");

    /// <summary>
    ///     Maximum number of log entries per file before rotation (default: 10000)
    /// </summary>
    public int MaxLogEntriesPerFile { get; set; } = 10000;

    /// <summary>
    ///     Force abandon log writing on exception (default: false for safety)
    /// </summary>
    public bool ForceAbandonLogOnException { get; set; }

    /// <summary>
    ///     Maximum number of days to retain log files (default: 60 days)
    ///     Logs older than this will be automatically deleted
    /// </summary>
    public int MaxLogRetentionDays { get; set; } = 60;

    /// <summary>
    ///     Auto-delete received files after successful processing (default: false for safety)
    /// </summary>
    public bool AutoDeleteReceivedFiles { get; set; }

    //============================================================================================================

    public bool CheckStorageRootHaveFile { get; set; }

    /// <summary>
    ///     Custom download path for client received files (if null, uses StorageRoot)
    /// </summary>
    public string? ClientDownloadPath { get; private set; } //如果為空則使用 StorageRoot

    /// <summary>
    ///     Custom upload path for server received files (if null, uses StorageRoot)
    /// </summary>
    public string? ServerUploadPath { get; private set; } //如果為空則使用 StorageRoot

    public string? UserStoragePath { get; private set; } //如果為空則使用 StorageRoot
    /// <summary>
    ///     Station index (machine number) for device identification
    /// </summary>
    private string stationIndex = "-1";

    public string StationIndex
    {
        get
        {
            return stationIndex;
        }
        set
        {
            stationIndex = value;
            if (stationIndex != "-1")
            {
                StorageRoot = Path.Combine(AppContext.BaseDirectory, $"Storage_{stationIndex}");
                LogFilePath = Path.Combine(AppContext.BaseDirectory, "Log", $"grpc_{stationIndex}.log");
            }
        }
    }


    public string EncryptedString { get; set; }

    public void SetStationIndex(string idx = "-1")
    {
        StationIndex = idx;
    }

    public static GrpcConfig Load(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return new GrpcConfig();
        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<GrpcConfig>(json) ?? new GrpcConfig();

        // Normalize paths after loading
        config.StorageRoot = NormalizePath(config.StorageRoot);
        config.LogFilePath = NormalizePath(config.LogFilePath);
        if (!string.IsNullOrEmpty(config.ClientDownloadPath))
            config.ClientDownloadPath = NormalizePath(config.ClientDownloadPath);
        if (!string.IsNullOrEmpty(config.ServerUploadPath))
            config.ServerUploadPath = NormalizePath(config.ServerUploadPath);
        if (!string.IsNullOrEmpty(config.UserStoragePath))
            config.UserStoragePath = NormalizePath(config.UserStoragePath);

        return config;
    }


    public RpcSecureCredentials GetSecureCredentials()
    {
        if (string.IsNullOrEmpty(EncryptedString))
            return new RpcSecureCredentials(Password);
        return RpcSecureCredentials.FromEncryptedString(EncryptedString);
    }


    public void SetPassword(string password)
    {
        using (var credentials = new RpcSecureCredentials(password))
        {
            EncryptedString = credentials.ToEncryptedString();
        }
    }

    //save config to file
    public void Save(string path)
    {
        try
        {
            // Normalize paths before saving
            StorageRoot = NormalizePath(StorageRoot);
            LogFilePath = NormalizePath(LogFilePath);
            if (!string.IsNullOrEmpty(ClientDownloadPath))
                ClientDownloadPath = NormalizePath(ClientDownloadPath);
            if (!string.IsNullOrEmpty(ServerUploadPath))
                ServerUploadPath = NormalizePath(ServerUploadPath);
            if (!string.IsNullOrEmpty(UserStoragePath))
                UserStoragePath = NormalizePath(UserStoragePath);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            var folder = Directory.CreateDirectory(Path.HasExtension(path) ? Path.GetDirectoryName(path) : path);
            File.WriteAllText(path, json);
        }
        catch (Exception)
        {
        }
    }

    public void EnsureFolders()
    {
        Directory.CreateDirectory(ExpandPath(StorageRoot));
        if (!string.IsNullOrWhiteSpace(ClientDownloadPath))
            Directory.CreateDirectory(ExpandPath(ClientDownloadPath));
        if (!string.IsNullOrWhiteSpace(ServerUploadPath))
            Directory.CreateDirectory(ExpandPath(ServerUploadPath));
        var logDir = Path.GetDirectoryName(ExpandPath(LogFilePath));
        if (!string.IsNullOrWhiteSpace(logDir)) Directory.CreateDirectory(logDir);
    }

    /// <summary>
    ///     Get the effective client download directory
    /// </summary>
    public string GetClientDownloadPath()
    {
        return ClientDownloadPath ?? StorageRoot;
    }

    /// <summary>
    ///     Get the effective server upload directory
    /// </summary>
    public string GetServerUploadPath()
    {
        return ServerUploadPath ?? StorageRoot;
    }

    public string GetUserStoragePath()
    {
        return UserStoragePath ?? StorageRoot;
    }

    public void SaveHost(string? host)
    {
        Host = host ?? Host;
    }

    public void SavePort(int? port)
    {
        Port = port ?? Port;
    }

    public void SetUserStoragePath(string filePath)
    {
        UserStoragePath = filePath;
    }

    public void SetServerUploadPath(string filePath)
    {
        ServerUploadPath = filePath;
    }

    public void SetClientDownloadPath(string filePath)
    {
        ClientDownloadPath = filePath;
    }

    /// <summary>
    /// Normalize path to use backslashes and convert to relative path if under base directory
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Convert forward slashes to backslashes
        path = path.Replace('/', '\\');

        // Convert to relative path if under base directory
        string baseDir = AppContext.BaseDirectory.TrimEnd('\\');
        if (path.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
        {
            string relativePath = path.Substring(baseDir.Length).TrimStart('\\');
            return ".\\" + relativePath;
        }

        return path;
    }

    /// <summary>
    /// Expand relative path (.\\) to full path based on executable directory
    /// </summary>
    public static string ExpandPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Convert forward slashes to backslashes first
        path = path.Replace('/', '\\');

        // Expand relative path to full path
        if (path.StartsWith(".\\"))
        {
            string relativePart = path.Substring(2); // Remove ".\"
            return Path.Combine(AppContext.BaseDirectory, relativePart);
        }

        return path;
    }
}