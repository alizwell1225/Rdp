using LIB_RPC.Abstractions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace LIB_RPC
{
    /// <summary>
    /// Windows-specific screen capture implementation.
    /// Uses System.Drawing and System.Windows.Forms for screen capture.
    /// </summary>
    public class ScreenCapture : IScreenCapture
    {
        public byte[] CapturePrimaryPng()
        {
            // Check if we're on Windows
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Screen capture is only supported on Windows platforms.");
            }

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
                
                // Save to PNG format in memory
                using var ms = new MemoryStream();
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
