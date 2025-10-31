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

        /// <summary>
        /// Broadcasts a JSON message to all connected clients.
        /// </summary>
        Task BroadcastJsonAsync(string type, string body);

        /// <summary>
        /// Pushes a file to all connected clients.
        /// </summary>
        Task PushFileAsync(string path);

        /// <summary>
        /// Gets the list of files in the server storage.
        /// </summary>
        string[] GetFiles();
    }
}
