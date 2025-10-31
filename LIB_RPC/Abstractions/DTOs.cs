namespace LIB_RPC.Abstractions
{
    /// <summary>
    /// Data transfer object for JSON messages, isolating UI from proto types.
    /// </summary>
    public sealed record JsonMessage
    {
        /// <summary>
        /// Gets or initializes the unique identifier for this message.
        /// </summary>
        public string Id { get; init; } = string.Empty;
        
        /// <summary>
        /// Gets or initializes the message type/category.
        /// </summary>
        public string Type { get; init; } = string.Empty;
        
        /// <summary>
        /// Gets or initializes the JSON payload.
        /// </summary>
        public string Json { get; init; } = string.Empty;
        
        /// <summary>
        /// Gets or initializes the Unix timestamp in milliseconds.
        /// </summary>
        public long Timestamp { get; init; }
    }

    /// <summary>
    /// Data transfer object for JSON acknowledgments.
    /// </summary>
    public sealed record JsonAcknowledgment
    {
        /// <summary>
        /// Gets or initializes the correlation ID from the request.
        /// </summary>
        public string Id { get; init; } = string.Empty;
        
        /// <summary>
        /// Gets or initializes a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; init; }
        
        /// <summary>
        /// Gets or initializes the error message if operation failed.
        /// </summary>
        public string Error { get; init; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for file transfer results.
    /// </summary>
    public sealed record FileTransferResult
    {
        /// <summary>
        /// Gets or initializes the file path.
        /// </summary>
        public string Path { get; init; } = string.Empty;
        
        /// <summary>
        /// Gets or initializes a value indicating whether the transfer succeeded.
        /// </summary>
        public bool Success { get; init; }
        
        /// <summary>
        /// Gets or initializes the error message if transfer failed.
        /// </summary>
        public string Error { get; init; } = string.Empty;
    }
}
