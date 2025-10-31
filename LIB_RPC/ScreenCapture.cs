using LIB_RPC.Abstractions;

#if WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
#endif

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

#if WINDOWS
            try
            {
                // Get primary screen bounds
                var bounds = System.Windows.Forms.Screen.PrimaryScreen!.Bounds;
                
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
#else
            // When compiled on non-Windows, use reflection to try loading the types at runtime
            try
            {
                // Try to dynamically load Windows Forms types (they might be available if running on Windows)
                var screenType = Type.GetType("System.Windows.Forms.Screen, System.Windows.Forms");
                if (screenType == null)
                {
                    throw new PlatformNotSupportedException("System.Windows.Forms is not available. This library should be compiled on Windows for full screen capture support.");
                }

                var primaryScreenProperty = screenType.GetProperty("PrimaryScreen");
                var primaryScreen = primaryScreenProperty?.GetValue(null);
                if (primaryScreen == null)
                {
                    throw new InvalidOperationException("Could not access primary screen.");
                }

                var boundsProperty = primaryScreen.GetType().GetProperty("Bounds");
                var bounds = boundsProperty?.GetValue(primaryScreen);
                if (bounds == null)
                {
                    throw new InvalidOperationException("Could not get screen bounds.");
                }

                var widthProperty = bounds.GetType().GetProperty("Width");
                var heightProperty = bounds.GetType().GetProperty("Height");
                var locationProperty = bounds.GetType().GetProperty("Location");
                var sizeProperty = bounds.GetType().GetProperty("Size");

                int width = (int)(widthProperty?.GetValue(bounds) ?? 0);
                int height = (int)(heightProperty?.GetValue(bounds) ?? 0);
                var location = locationProperty?.GetValue(bounds);
                var size = sizeProperty?.GetValue(bounds);

                var bitmapType = Type.GetType("System.Drawing.Bitmap, System.Drawing.Common");
                if (bitmapType == null)
                {
                    throw new PlatformNotSupportedException("System.Drawing.Common is not available.");
                }

                var bitmapCtor = bitmapType.GetConstructor(new[] { typeof(int), typeof(int) });
                var bitmap = bitmapCtor?.Invoke(new object[] { width, height });
                if (bitmap == null)
                {
                    throw new InvalidOperationException("Could not create bitmap.");
                }

                try
                {
                    var graphicsType = Type.GetType("System.Drawing.Graphics, System.Drawing.Common");
                    var fromImageMethod = graphicsType?.GetMethod("FromImage", new[] { bitmapType });
                    var graphics = fromImageMethod?.Invoke(null, new[] { bitmap });
                    if (graphics == null)
                    {
                        throw new InvalidOperationException("Could not create graphics object.");
                    }

                    try
                    {
                        var pointType = Type.GetType("System.Drawing.Point, System.Drawing.Common");
                        var sizeType = Type.GetType("System.Drawing.Size, System.Drawing.Common");
                        var copyFromScreenMethod = graphics.GetType().GetMethod("CopyFromScreen", 
                            new[] { pointType, pointType, sizeType });
                        var pointCtor = pointType?.GetConstructor(new[] { typeof(int), typeof(int) });
                        var destPoint = pointCtor?.Invoke(new object[] { 0, 0 });
                        copyFromScreenMethod?.Invoke(graphics, new[] { location, destPoint, size });
                    }
                    finally
                    {
                        (graphics as IDisposable)?.Dispose();
                    }

                    using var ms = new MemoryStream();
                    var imageFormatType = Type.GetType("System.Drawing.Imaging.ImageFormat, System.Drawing.Common");
                    var pngProperty = imageFormatType?.GetProperty("Png");
                    var pngFormat = pngProperty?.GetValue(null);
                    var saveMethod = bitmap.GetType().GetMethod("Save", new[] { typeof(Stream), imageFormatType });
                    saveMethod?.Invoke(bitmap, new[] { ms, pngFormat });
                    return ms.ToArray();
                }
                finally
                {
                    (bitmap as IDisposable)?.Dispose();
                }
            }
            catch (Exception ex) when (ex is not PlatformNotSupportedException)
            {
                throw new InvalidOperationException($"Screen capture failed: {ex.Message}", ex);
            }
#endif
        }
    }
}
