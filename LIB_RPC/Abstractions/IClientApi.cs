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
        event Action<string>? OnReceivedFileFromServer;

        /// <summary>
        /// Event raised when server file push encounters an error.
        /// </summary>
        event Action<string, string>? OnReceivedFileErrorFromServer;

        /// <summary>
        /// Event raised when server pushes a JSON message.
        /// </summary>
        event Action<JsonMessage>? OnReceivedJsonMessageFromServer;

        /// <summary>
        /// Event raised when server pushes byte data.
        /// </summary>
        event Action<string, byte[], string?>? OnReceivedByteDataFromServer;

        /// <summary>
        /// Event raised during byte transfer progress (type, bytesTransferred, totalBytes, percentage).
        /// </summary>
        event Action<string, long, long, double>? OnByteTransferProgress;

        /// <summary>
        /// Event raised when connection to server is established successfully.
        /// </summary>
        event Action? OnConnected;

        /// <summary>
        /// Event raised when disconnected from server.
        /// </summary>
        event Action? OnDisconnected;

        /// <summary>
        /// Event raised when connection attempt fails.
        /// </summary>
        event Action<string>? OnConnectionError;

        /// <summary>
        /// Event raised when file upload starts.
        /// </summary>
        event Action<string>? OnUploadStarted;

        /// <summary>
        /// Event raised when file upload completes successfully.
        /// </summary>
        event Action<string>? OnUploadCompleted;

        /// <summary>
        /// Event raised when file upload fails.
        /// </summary>
        event Action<string, string>? OnUploadFailed;

        /// <summary>
        /// Event raised when file download starts.
        /// </summary>
        event Action<string>? OnDownloadStarted;

        /// <summary>
        /// Event raised when file download completes successfully.
        /// </summary>
        event Action<string>? OnDownloadCompleted;

        /// <summary>
        /// Event raised when file download fails.
        /// </summary>
        event Action<string, string>? OnDownloadFailed;

        /// <summary>
        /// Event raised when screenshot capture starts.
        /// </summary>
        event Action? OnScreenshotStarted;

        /// <summary>
        /// Event raised when screenshot capture completes successfully.
        /// </summary>
        event Action<int>? OnScreenshotCompleted;

        /// <summary>
        /// Event raised when screenshot capture fails.
        /// </summary>
        event Action<string>? OnScreenshotFailed;

        /// <summary>
        /// Event raised when server file push starts.
        /// </summary>
        event Action<string>? OnServerFileStarted;

        /// <summary>
        /// Connects to the gRPC server.
        /// </summary>
        Task ConnectAsync(bool re,CancellationToken ct = default);

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

        /// <summary>
        /// Sends byte data to the server with acknowledgment.
        /// </summary>
        /// <param name="type">Data type identifier (e.g., "image", "file")</param>
        /// <param name="data">Raw byte data to send</param>
        /// <param name="metadata">Optional JSON metadata</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SendByteAsync(string type, byte[] data, string? metadata = null, CancellationToken ct = default);

        /// <summary>
        /// Sends byte data to the server without acknowledgment (fire and forget).
        /// </summary>
        /// <param name="type">Data type identifier (e.g., "image", "file")</param>
        /// <param name="data">Raw byte data to send</param>
        /// <param name="metadata">Optional JSON metadata</param>
        /// <param name="ct">Cancellation token</param>
        Task SendByteNoAckAsync(string type, byte[] data, string? metadata = null, CancellationToken ct = default);
    }
}
