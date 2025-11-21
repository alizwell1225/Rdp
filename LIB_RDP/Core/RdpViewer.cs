using LIB_RDP.Models;
using LIB_RDP.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace LIB_RDP.Core;

public class RdpViewer : Panel
{
    private static readonly string _debugLogPath = Path.Combine(Path.GetTempPath(), "Log","RdpViewer-debug.log");
    private readonly int _bottomHoverThreshold = 48; // px from bottom to trigger hover
    private readonly Font _messageFont;
    private readonly Timer _refreshTimer;
    private readonly StringFormat _stringFormat;

    private int _animationFrame;

    // track the embedded ActiveX/client control so we can attach mouse handlers
    private Control _axClientControl;
    private RdpConnection _connection;
    private bool _isConnected;
    private bool _isHovered;
    private DockStyle _originalDock;
    private Point _originalLocation;
    private Size _originalSize;
    private FormRDPShow _rdpForm;
    private RdpStatusOverlay _statusOverlay;

    public RdpViewer()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        _refreshTimer = new Timer { Interval = 100 };
        _refreshTimer.Tick += RefreshTimer_Tick;

        _messageFont = new Font("微軟正黑體", 12f, FontStyle.Bold);
        _stringFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        BorderStyle = BorderStyle.FixedSingle;

        SizeChanged += RdpViewer_SizeChanged;
        MouseEnter += RdpViewer_MouseEnter;
        MouseLeave += RdpViewer_MouseLeave;
        MouseDown += RdpViewer_MouseDown;
        MouseMove += RdpViewer_MouseMove;

        // RdpViewer no longer creates a bottom menu; host (FormRDP) will create its own
    }

    public RdpConnection GetRdpConnection()
    {
        return _connection;
    }

    public bool IsFullScreen { get; private set; }

    // public event raised when viewer requests fullscreen toggle (Ctrl+Click)
    public event EventHandler FullScreenRequested;

    // expose bottom hover state to host so host can show/hide its own menu
    public event Action<bool> BottomHoverChanged;

    private void LogDebug(string msg)
    {
        try
        {
            var id = Name ?? GetHashCode().ToString();
            var log = $"{DateTime.Now:O} [RdpViewer:{id}] {msg}{Environment.NewLine}";
            Debug.WriteLine(log);
            File.AppendAllText(_debugLogPath, log);
        }
        catch
        {
        }
    }

    private void RdpViewer_SizeChanged(object sender, EventArgs e)
    {
        if (_connection?.IsConnected == true)
            try
            {
                var axRdpClient9 = _connection.GetRdpClient();
                if (axRdpClient9 != null && !axRdpClient9.IsDisposed)
                {
                    axRdpClient9.Size = ClientSize;

                    _connection.Configure(new RdpConfig
                    {
                        ScreenWidth = ClientSize.Width,
                        ScreenHeight = ClientSize.Height,
                        ColorDepth = 32
                    });

                    axRdpClient9.Invalidate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"更新 RDP 大小時發生錯誤：{ex.Message}");
            }

        Invalidate();
    }

    public void SetConnection(RdpConnection connection)
    {
        if (_connection != null)
        {
            _refreshTimer.Stop();
            _connection.ConnectionStateChanged -= HandleConnectionStateChanged;
            _connection.Disconnect();

            // detach mouse handler from previous ActiveX/client control if present
            if (_axClientControl != null)
            {
                try
                {
                    _axClientControl.MouseDown -= RdpViewer_MouseDown;
                }
                catch
                {
                }

                _axClientControl = null;
            }
        }

        _connection = connection;

        if (_connection != null)
        {
            _connection.ConnectionStateChanged += HandleConnectionStateChanged;
            var axRdpClient9 = _connection.GetRdpClient();
            try
            {
                axRdpClient9.Dock = DockStyle.Fill;
                Controls.Clear();
                Controls.Add(axRdpClient9);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            // attach mouse handler to the embedded client so clicks on the child control
            // are detected (Ctrl+Click should toggle fullscreen). Store reference so
            // we can detach later when swapping connections.
            try
            {
                axRdpClient9.MouseDown += RdpViewer_MouseDown;
                // also attach MouseMove so we can detect hovering near bottom when ActiveX covers the panel
                try
                {
                    axRdpClient9.MouseMove += RdpViewer_MouseMove;
                }
                catch
                {
                }

                _axClientControl = axRdpClient9;
                LogDebug("Attached MouseDown to active client control.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"無法綁定子控制項 MouseDown: {ex.Message}");
                LogDebug($"Attach child MouseDown failed: {ex.Message}");
            }

            // 確保 ActiveX 控制項已建立並有 handle，否則 DrawImage / GetScreenshot 可能失敗
            try
            {
                axRdpClient9.CreateControl();
                axRdpClient9.BringToFront();
                axRdpClient9.Visible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RdpViewer SetConnection create control failed: {ex.Message}");
                LogDebug($"CreateControl/BringToFront failed: {ex.Message}");
            }

            _connection.Configure(new RdpConfig
            {
                ScreenWidth = ClientSize.Width,
                ScreenHeight = ClientSize.Height,
                ColorDepth = 32
            });

            _refreshTimer.Start();
        }
        else
        {
            RestoreDefaultView();
        }

        Invalidate();
    }

    private void RdpForm_ConnectionStateChanged(object sender, RdpStateEventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<object, RdpStateEventArgs>(RdpForm_ConnectionStateChanged), sender, e);
            return;
        }

        switch (e.State)
        {
            case RdpState.Connected:
                _isConnected = true;
                break;
            case RdpState.Connecting:
            case RdpState.Retrying:
                // 連線中或重試中,保持等待狀態
                break;
            case RdpState.Disconnected:
                _isConnected = false;
                CleanupRdpClient();
                RestoreDefaultView();
                break;
            case RdpState.Error:
                _isConnected = false;
                CleanupRdpClient();
                RestoreDefaultView();
                MessageBox.Show("連線發生錯誤");
                break;
        }
    }

    private void CleanupRdpClient()
    {
        _refreshTimer.Stop();

        if (_rdpForm != null)
        {
            _rdpForm.ConnectionStateChanged -= RdpForm_ConnectionStateChanged;
            _rdpForm.Close();
            _rdpForm.Dispose();
            _rdpForm = null;
        }

        _isConnected = false;

        // ensure any attached child mouse handlers are removed
        if (_axClientControl != null)
        {
            try
            {
                _axClientControl.MouseDown -= RdpViewer_MouseDown;
            }
            catch
            {
            }

            try
            {
                _axClientControl.MouseMove -= RdpViewer_MouseMove;
            }
            catch
            {
            }

            LogDebug("Detached MouseDown/MouseMove from active client control.");
            _axClientControl = null;
        }
    }

    private void RefreshTimer_Tick(object sender, EventArgs e)
    {
        _animationFrame++;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer.Dispose();
            _messageFont.Dispose();
            _stringFormat.Dispose();
            CleanupRdpClient();
            SetConnection(null);
            // bottom menu moved to the host (FormRDP). No local cleanup required here.
        }

        base.Dispose(disposing);
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        if (_connection?.IsConnected == true && _rdpForm != null) _rdpForm.ToggleFullScreen();
    }

    private void RestoreDefaultView()
    {
        Controls.Clear();
        var label = new Label
        {
            Text = "未連線",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            BackColor = Color.Black
        };
        Controls.Add(label);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        if (_connection == null)
        {
            DrawMessage("尚未建立連線", Color.FromArgb(80, 80, 80), e.Graphics);
            return;
        }

        try
        {
            using (var screenshot = _connection.GetScreenshot())
            {
                if (screenshot != null)
                    e.Graphics.DrawImage(screenshot, ClientRectangle);
                else
                    switch (_connection.State)
                    {
                        case RdpState.Connecting:
                            var dots = new string('.', _animationFrame % 4);
                            DrawMessage($"正在連線中{dots}", ThemeManager.PrimaryColor, e.Graphics);
                            break;
                        case RdpState.Retrying:
                            var retryDots = new string('.', _animationFrame % 4);
                            DrawMessage($"正在重試連線 ({_connection.RetryCount}/{3}){retryDots}", ThemeManager.WarningColor,
                                e.Graphics);
                            break;
                        case RdpState.Error:
                            DrawMessage("連線發生錯誤", ThemeManager.ErrorColor, e.Graphics);
                            break;
                        case RdpState.Disconnected:
                            DrawMessage("連線已中斷", ThemeManager.WarningColor, e.Graphics);
                            break;
                        default:
                            DrawMessage("連線狀態異常", ThemeManager.ErrorColor, e.Graphics);
                            break;
                    }
            }
        }
        catch (Exception ex)
        {
            DrawMessage($"錯誤：{ex.Message}", ThemeManager.ErrorColor, e.Graphics);
        }

        // 繪製 hover 邊框
        if (_isHovered && !IsFullScreen)
            using (var pen = new Pen(ThemeManager.PrimaryColor, 3))
            {
                var borderRect = new Rectangle(1, 1, Width - 3, Height - 3);
                e.Graphics.DrawRectangle(pen, borderRect);
            }
    }

    private void DrawMessage(string message, Color color, Graphics g)
    {
        using (var bgBrush = new SolidBrush(Color.FromArgb(220, ThemeManager.SurfaceColor)))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        using (var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
        {
            var shadowRect = new RectangleF(
                ClientRectangle.X + 2,
                ClientRectangle.Y + 2,
                ClientRectangle.Width,
                ClientRectangle.Height
            );
            g.DrawString(message, _messageFont, shadowBrush, shadowRect, _stringFormat);
        }

        using (var textBrush = new SolidBrush(color))
        {
            g.DrawString(message, _messageFont, textBrush, ClientRectangle, _stringFormat);
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        Invalidate();
    }

    private void RdpViewer_MouseEnter(object sender, EventArgs e)
    {
        _isHovered = true;
        Invalidate();
    }

    private void RdpViewer_MouseLeave(object sender, EventArgs e)
    {
        _isHovered = false;
        Invalidate();
    }

    private void RdpViewer_MouseDown(object sender, MouseEventArgs e)
    {
        //判定是否按著 Ctrl，且為左鍵
        try
        {
            LogDebug($"MouseDown event (button={e.Button}, modifiers={ModifierKeys}, sender={sender?.GetType().Name})");
        }
        catch
        {
        }

        if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
        {
            LogDebug("Detected Ctrl+Left click — raising FullScreenRequested");
            // 觸發全螢幕要求，由外部訂閱處理切換
            FullScreenRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RdpViewer_MouseMove(object sender, MouseEventArgs e)
    {
        // normalize to viewer client coords so child control or parent both work
        try
        {
            LogDebug($"MouseMove event (sender={sender?.GetType().Name}, x={e?.X}, y={e?.Y})");
        }
        catch
        {
        }

        var pt = PointToClient(Cursor.Position);
        var nearBottom = pt.Y >= Math.Max(0, ClientSize.Height - _bottomHoverThreshold);

        try
        {
            BottomHoverChanged?.Invoke(nearBottom);
        }
        catch
        {
        }
    }

    private void HideBottomMenu()
    {
        // RdpViewer no longer manages a bottom panel; host should hide when appropriate
    }

    public RdpConnection GetConnection()
    {
        return _connection;
    }

    /// <summary>
    ///     Attach a status overlay (left-top transparent label). If null will detach existing.
    /// </summary>
    public void AttachStatusOverlay(RdpStatusOverlay overlay)
    {
        // detach existing
        if (_statusOverlay != null)
        {
            try
            {
                _statusOverlay.Detach();
            }
            catch
            {
            }

            _statusOverlay = null;
        }

        if (overlay != null)
        {
            _statusOverlay = overlay;
            _statusOverlay.AttachTo(this);
            _statusOverlay.BringToFront();
            try
            {
                LogDebug($"Attached status overlay: '{overlay.Text ?? "(empty)"}'");
            }
            catch
            {
            }
        }
    }

    private void HandleConnectionStateChanged(object sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new EventHandler(HandleConnectionStateChanged), sender, e);
            return;
        }

        if (_connection != null)
        {
            if (_connection.State == RdpState.Connected)
            {
                _refreshTimer.Start();
            }
            else
            {
                _refreshTimer.Stop();
                if (_connection.State == RdpState.Disconnected) RestoreDefaultView();
            }
        }

        Invalidate();
    }

    public void SaveCurrentState()
    {
        _originalSize = Size;
        _originalLocation = Location;
        _originalDock = Dock;
        IsFullScreen = true;
    }

    public void RestoreOriginalState()
    {
        Dock = _originalDock;
        Size = _originalSize;
        Location = _originalLocation;
        IsFullScreen = false;
    }

    // Provide a method for external callers to request fullscreen.
    // Event must be raised from within the declaring type; external callers should use this helper.
    public void RequestFullScreen()
    {
        FullScreenRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetMax(int w, int h)
    {
        try
        {
            if (_connection == null)
                return;
            if (_connection.GetConnectionStats().IsConnected == false )
                return;
            w = Math.Min(w, 4096);
            h = Math.Min(h, 2160);

            var axRdpClient9 = _connection.GetRdpClient();
            if (axRdpClient9.Connected == 1)
                axRdpClient_OnConnected(null, null);
        }
        catch (AxHost.InvalidActiveXStateException ex)
        {
            Debug.WriteLine($"無法設置桌面尺寸 - 控制項尚未準備好: {ex.Message}");
        }
    }

    private void axRdpClient_OnConnected(object sender, EventArgs e)
    {
        Task.Delay(500).ContinueWith(_ =>
        {
            try
            {
                this.Invoke((Action)(() =>
                {
                    SetMax(1920, 1080);
                }));
            }
            catch (Exception exception)
            {
            }

        });
    }


    public void SetScreenSize()
    {
        SetMax(_connection.GetRdpConfig().ScreenWidth, _connection.GetRdpConfig().ScreenHeight);
    }
}