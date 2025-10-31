namespace LIB_RPC.Abstractions
{
    /// <summary>
    /// Data transfer object for JSON messages, isolating UI from proto types.
    /// </summary>
    public sealed record JsonMessage
    {
        public string Id { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Json { get; init; } = string.Empty;
        public long Timestamp { get; init; }
    }

    /// <summary>
    /// Data transfer object for JSON acknowledgments.
    /// </summary>
    public sealed record JsonAcknowledgment
    {
        public string Id { get; init; } = string.Empty;
        public bool Success { get; init; }
        public string Error { get; init; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for file transfer results.
    /// </summary>
    public sealed record FileTransferResult
    {
        public string Path { get; init; } = string.Empty;
        public bool Success { get; init; }
        public string Error { get; init; } = string.Empty;
    }
}
