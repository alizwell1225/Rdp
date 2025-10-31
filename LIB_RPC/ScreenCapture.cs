using LIB_RPC.Abstractions;

#if NET6_0_OR_GREATER && WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace LIB_RPC
{
    /// <summary>
    /// Windows-specific screen capture implementation.
    /// Only available when targeting Windows platforms.
    /// </summary>
    public class ScreenCapture : IScreenCapture
    {
        public byte[] CapturePrimaryPng()
        {
#if NET6_0_OR_GREATER && WINDOWS
            var bounds = System.Windows.Forms.Screen.PrimaryScreen!.Bounds;
            using var bmp = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
#else
            throw new PlatformNotSupportedException("Screen capture is only supported on Windows platforms with Windows Forms.");
#endif
        }
    }
}
