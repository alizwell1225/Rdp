namespace LIB_RPC.Abstractions
{
    /// <summary>
    /// Interface for gRPC server operations, providing abstraction from implementation details.
    /// </summary>
    public interface IServerApi : IAsyncDisposable
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
        /// Event raised when a file is added to the server storage.
        /// </summary>
        event Action<string>? OnFileAdded;

        /// <summary>
        /// Event raised when server starts successfully.
        /// </summary>
        event Action? OnServerStarted;

        /// <summary>
        /// Event raised when server stops.
        /// </summary>
        event Action? OnServerStopped;

        /// <summary>
        /// Event raised when server startup fails.
        /// </summary>
        event Action<string>? OnServerStartFailed;

        /// <summary>
        /// Event raised when a client connects to the server.
        /// </summary>
        event Action<string>? OnClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the server.
        /// </summary>
        event Action<string>? OnClientDisconnected;

        /// <summary>
        /// Event raised when file upload from client starts.
        /// </summary>
        event Action<string>? OnFileUploadStarted;

        /// <summary>
        /// Event raised when file upload from client completes successfully.
        /// </summary>
        event Action<string>? OnFileUploadCompleted;

        /// <summary>
        /// Event raised when file upload from client fails.
        /// </summary>
        event Action<string, string>? OnFileUploadFailed;

        /// <summary>
        /// Event raised when server starts pushing a file to clients.
        /// </summary>
        event Action<string>? OnFilePushStarted;

        /// <summary>
        /// Event raised when server completes pushing a file to all clients.
        /// </summary>
        event Action<string>? OnFilePushCompleted;

        /// <summary>
        /// Event raised when server fails to push file to one or more clients.
        /// </summary>
        event Action<string, string>? OnFilePushFailed;

        /// <summary>
        /// Event raised when broadcast message is sent successfully.
        /// </summary>
        event Action<string, int>? OnBroadcastSent;

        /// <summary>
        /// Event raised when broadcast message fails.
        /// </summary>
        event Action<string, string>? OnBroadcastFailed;

        /// <summary>
        /// 位元組傳輸進度事件 (type, bytesTransferred, totalBytes, percentage)
        /// Event raised during byte transfer progress (type, bytesTransferred, totalBytes, percentage).
        /// </summary>
        event Action<string, long, long, double>? OnByteTransferProgress;

        /// <summary>
        /// 從客戶端接收位元組資料事件 (type, data, metadata) - 用於接收圖片、檔案等二進位資料
        /// Event raised when receiving byte data from client (type, data, metadata) - for receiving images, files, etc.
        /// </summary>
        event Action<string, byte[], string?>? OnReceivedByteDataFromClient;

        /// <summary>
        /// 從客戶端接收 JSON 訊息事件 (id, type, json, timestamp) - 用於接收 JSON 格式的資料
        /// Event raised when receiving JSON message from client (id, type, json, timestamp) - for receiving JSON data
        /// </summary>
        event Action<string, string, string, long>? OnReceivedJsonMessageFromClient;

        /// <summary>
        /// Updates the server configuration with new host and port.
        /// </summary>
        void UpdateConfig(string? host, int? port);

        /// <summary>
        /// Starts the gRPC server.
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops the gRPC server.
        /// </summary>
        Task StopAsync();

        Task StopAsync(CancellationTokenSource token);

        /// <summary>
        /// Broadcasts a JSON message to all connected clients.
        /// </summary>
        Task BroadcastJsonAsync(string type, string body);

        /// <summary>
        /// Broadcasts a JSON message to all connected clients with acknowledgment and retry support.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="body">JSON body content</param>
        /// <param name="retryCount">Number of retries (0 = no retry)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Tuple with success status, number of clients reached, and error message</returns>
        Task<(bool Success, int ClientsReached, string Error)> BroadcastWithAckAsync(string type, string body, int retryCount = 0, CancellationToken ct = default);

        /// <summary>
        /// Pushes a file to all connected clients.
        /// </summary>
        Task PushFileAsync(string path);

        /// <summary>
        /// Pushes a file to all connected clients with acknowledgment and retry support.
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="retryCount">Number of retries (0 = no retry)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Tuple with success status, number of clients reached, and error message</returns>
        Task<(bool Success, int ClientsReached, string Error)> PushFileWithAckAsync(string path, int retryCount = 0, CancellationToken ct = default);

        /// <summary>
        /// Gets the list of files in the server storage.
        /// </summary>
        string[] GetFiles();

        /// <summary>
        /// Sends byte data to a specific client or broadcasts to all clients with acknowledgment.
        /// </summary>
        /// <param name="type">Data type identifier (e.g., "image", "file")</param>
        /// <param name="data">Raw byte data to send</param>
        /// <param name="metadata">Optional JSON metadata</param>
        /// <param name="clientId">Target client ID, or null to broadcast to all clients</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Tuple with success status, number of clients reached, and error message</returns>
        Task<(bool Success, int ClientsReached, string Error)> SendByteAsync(string type, byte[] data, string? metadata = null, string? clientId = null, CancellationToken ct = default);

        /// <summary>
        /// Sends byte data to a specific client or broadcasts to all clients without acknowledgment (fire and forget).
        /// </summary>
        /// <param name="type">Data type identifier (e.g., "image", "file")</param>
        /// <param name="data">Raw byte data to send</param>
        /// <param name="metadata">Optional JSON metadata</param>
        /// <param name="clientId">Target client ID, or null to broadcast to all clients</param>
        /// <param name="ct">Cancellation token</param>
        Task SendByteNoAckAsync(string type, byte[] data, string? metadata = null, string? clientId = null, CancellationToken ct = default);

        void SetCancel();
        void InitToken();
        CancellationTokenSource GetTokenSource();
    }
}
