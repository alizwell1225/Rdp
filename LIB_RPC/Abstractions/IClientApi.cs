namespace LIB_RPC.Abstractions
{
    /// <summary>
    /// Interface for gRPC client operations, providing abstraction from implementation details.
    /// </summary>
    public interface IClientApi : IAsyncDisposable
    {
        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        GrpcConfig Config { get; }

        /// <summary>
        /// Event raised when a log line is produced.
        /// </summary>
        event Action<string>? OnLog;

        /// <summary>
        /// Event raised during file upload progress.
        /// </summary>
        event Action<string, double>? OnUploadProgress;

        /// <summary>
        /// Event raised during file download progress.
        /// </summary>
        event Action<string, double>? OnDownloadProgress;

        /// <summary>
        /// Event raised during screenshot capture progress.
        /// </summary>
        event Action<double>? OnScreenshotProgress;

        /// <summary>
        /// Event raised when server completes pushing a file.
        /// </summary>
        event Action<string>? OnServerFileCompleted;

        /// <summary>
        /// Event raised when server file push encounters an error.
        /// </summary>
        event Action<string, string>? OnServerFileError;

        /// <summary>
        /// Event raised when server pushes a JSON message.
        /// </summary>
        event Action<JsonMessage>? OnServerJson;

        /// <summary>
        /// Connects to the gRPC server.
        /// </summary>
        Task ConnectAsync(CancellationToken ct = default);

        /// <summary>
        /// Disconnects from the gRPC server.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Sends a JSON message and waits for acknowledgment.
        /// </summary>
        Task<JsonAcknowledgment> SendJsonAsync(string type, string json, CancellationToken ct = default);

        /// <summary>
        /// Sends a JSON message without waiting for acknowledgment (fire and forget).
        /// </summary>
        Task SendJsonFireAndForgetAsync(string type, string json, CancellationToken ct = default);

        /// <summary>
        /// Uploads a file to the server.
        /// </summary>
        Task<FileTransferResult> UploadFileAsync(string filePath, CancellationToken ct = default);

        /// <summary>
        /// Downloads a file from the server.
        /// </summary>
        Task DownloadFileAsync(string remotePath, string localPath, CancellationToken ct = default);

        /// <summary>
        /// Lists available files on the server.
        /// </summary>
        Task<string[]> ListFilesAsync(string directory = "", CancellationToken ct = default);

        /// <summary>
        /// Gets a screenshot from the server as PNG bytes.
        /// </summary>
        Task<byte[]> GetScreenshotAsync(CancellationToken ct = default);
    }
}
