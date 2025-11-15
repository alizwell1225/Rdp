using LIB_RPC.Abstractions;
using LIB_RPC.Optimizations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace LIB_RPC
{
    /// <summary>
    /// Windows-specific screen capture implementation.
    /// Uses System.Drawing and System.Windows.Forms for screen capture.
    /// Thread-safe with lock to prevent concurrent capture issues.
    /// OPTIMIZED: Uses RecyclableMemoryStream to reduce GC pressure.
    /// </summary>
    public class ScreenCapture : IScreenCapture
    {
        private readonly object _captureLock = new object();

        public byte[] CapturePrimaryPng()
        {
            // Check if we're on Windows
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Screen capture is only supported on Windows platforms.");
            }

            // Use lock to prevent concurrent screen captures which can cause GDI+ errors
            lock (_captureLock)
            {
                try
                {
                    // Get primary screen bounds
                    var bounds = Screen.PrimaryScreen!.Bounds;
                    
                    // Create bitmap of screen size
                    using var bmp = new Bitmap(bounds.Width, bounds.Height);
                    
                    // Copy screen content to bitmap
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                    }
                    
                    // OPTIMIZED: Use RecyclableMemoryStream instead of MemoryStream
                    // Reduces memory allocations and GC pressure
                    // PNG compression varies, so use a reasonable initial capacity (e.g., 1MB)
                    using var ms = new RecyclableMemoryStream(1024 * 1024);
                    bmp.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
                catch (Exception ex) when (ex is not PlatformNotSupportedException)
                {
                    throw new InvalidOperationException($"Screen capture failed: {ex.Message}", ex);
                }
            }
        }
    }
}
