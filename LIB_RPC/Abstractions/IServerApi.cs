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

        void SetCancel();
        void InitToken();
        CancellationTokenSource GetTokenSource();
    }
}
