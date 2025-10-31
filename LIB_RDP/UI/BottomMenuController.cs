using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LIB_RDP.Core;
using LIB_RDP.Models;

namespace LIB_RDP.UI
{
    // Helper controller that manages a floating bottom menu inside a RdpViewer.
    // Responsibilities:
    // - create a bottom panel with standard buttons (Fullscreen, Screenshot, Disconnect)
    // - auto-hide when mouse leaves and show when viewer signals BottomHoverChanged
    // - expose Dispose to clean up timers and event subscriptions
    public class BottomMenuController : IDisposable
    {
        private readonly RdpViewer _viewer;
        private readonly Panel _panel;
        private readonly Timer _hideTimer;
        private readonly Button _btnFull;
        private readonly Button _btnShot;
        private readonly Button _btnClose;
        private readonly string _debugLogPath;
        private bool _disposed;

        public Control Panel => _panel;

        public BottomMenuController(RdpViewer viewer, int height = 40)
        {
            _viewer = viewer ?? throw new ArgumentNullException(nameof(viewer));
            _debugLogPath = Path.Combine(Path.GetTempPath(), "FormRDP-bottommenu.log");

            _panel = new Panel
            {
                Name = $"bottomMenu_{viewer.Name}",
                Height = height,
                Dock = DockStyle.Bottom,
                Visible = false,
                BackColor = Color.FromArgb(200, Color.Black)
            };

            // Use FlowLayoutPanel style behaviour by adding buttons with Dock.Left except close right
            _btnFull = CreateButton("FullScreen");
            _btnShot = CreateButton("Screenshot");
            _btnClose = CreateButton("Disconnect");
            _btnClose.Dock = DockStyle.Right;

            // add buttons in logical order (left: shot, full; right: close)
            _panel.Controls.Add(_btnShot);
            _panel.Controls.Add(_btnFull);
            _panel.Controls.Add(_btnClose);

            _hideTimer = new Timer { Interval = 1500 };
            _hideTimer.Tick += (s, e) =>
            {
                try
                {
                    _panel.Visible = false;
                    _hideTimer.Stop();
                    File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [BottomMenu] Hide for {_viewer.Name}\r\n");
                }
                catch { }
            };

            // wire actions
            _btnFull.Click += BtnFull_Click;
            _btnShot.Click += BtnShot_Click;
            _btnClose.Click += BtnClose_Click;

            // attach status overlay default (hidden until text set) - host may set overlay later
            var overlay = new RdpStatusOverlay
            {
                ForeColor = ThemeManager.ForegroundColor,
                Font = new Font("微軟正黑體", 9f, FontStyle.Regular)
            };
            _viewer.AttachStatusOverlay(overlay);

            // subscribe to viewer hover event
            _viewer.BottomHoverChanged += Viewer_BottomHoverChanged;
            _viewer.FullScreenRequested += Viewer_FullScreenRequested;

            // put panel into viewer controls so it floats above content
            _viewer.Controls.Add(_panel);
        }

        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 100,
                Dock = DockStyle.Left,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 0, 0)
            };
        }

        private void Viewer_BottomHoverChanged(bool isNearBottom)
        {
            try
            {
                if (isNearBottom)
                {
                    _panel.Visible = true;
                    _panel.BringToFront();
                    _hideTimer.Stop();
                    _hideTimer.Start();
                    File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [BottomMenu] Show for {_viewer.Name}\r\n");
                }
                else
                {
                    _hideTimer.Stop();
                    _hideTimer.Start();
                }
            }
            catch { }
        }

        private void Viewer_FullScreenRequested(object sender, EventArgs e)
        {
            try
            {
                File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [BottomMenu] FullScreenRequested from {_viewer.Name}\r\n");
            }
            catch { }

            // toggle via host - raise viewer event handled by host; ensure UI toggle executed on UI thread
            if (_viewer.InvokeRequired)
                _viewer.BeginInvoke(new Action(() => ((Form)_viewer.FindForm())?.Focus()));
        }

        private void BtnFull_Click(object sender, EventArgs e)
        {
            try
            {
                File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [BottomMenu] FullScreen clicked for {_viewer.Name}\r\n");
            }
            catch { }

            // request fullscreen on viewer so host can handle it (viewer triggers event on ctrl+click too)
            // cannot invoke the event from outside the declaring type, call helper method on viewer
            _viewer.RequestFullScreen();
            RestartHideTimer();
        }

        private void BtnShot_Click(object sender, EventArgs e)
        {
            try
            {
                var conn = _viewer.GetConnection();
                var shot = conn?.GetScreenshot();
                if (shot != null)
                {
                    var path = Path.Combine(Path.GetTempPath(), $"rdp-snap-{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    shot.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                    File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [BottomMenu] Screenshot saved: {path}\r\n");
                }
            }
            catch (Exception ex)
            {
                try { File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [BottomMenu] Screenshot failed: {ex.Message}\r\n"); } catch { }
            }
            RestartHideTimer();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            try
            {
                File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [BottomMenu] Disconnect clicked for {_viewer.Name}\r\n");
            }
            catch { }

            try
            {
                var conn = _viewer.GetConnection();
                conn?.Disconnect();
            }
            catch { }
            RestartHideTimer();
        }

        private void RestartHideTimer()
        {
            try
            {
                _hideTimer.Stop();
                _hideTimer.Start();
            }
            catch { }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _viewer.BottomHoverChanged -= Viewer_BottomHoverChanged; } catch { }
            try { _viewer.FullScreenRequested -= Viewer_FullScreenRequested; } catch { }
            try { _hideTimer.Stop(); _hideTimer.Dispose(); } catch { }
            try { _btnFull.Click -= BtnFull_Click; _btnShot.Click -= BtnShot_Click; _btnClose.Click -= BtnClose_Click; } catch { }
            try { _panel?.Dispose(); } catch { }
        }
    }
}
