namespace LIB_RPC.Abstractions
{
    /// <summary>
    /// Interface for screen capture functionality, allowing different implementations.
    /// </summary>
    public interface IScreenCapture
    {
        /// <summary>
        /// Captures the primary screen as PNG bytes.
        /// </summary>
        /// <returns>PNG image bytes</returns>
        byte[] CapturePrimaryPng();
    }
}
