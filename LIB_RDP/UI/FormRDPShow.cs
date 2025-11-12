using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LIB_RDP.Core;
using LIB_RDP.Models;

namespace LIB_RDP.UI
{
    public partial class FormRDPShow : Form
    {
        private          RdpConnection _connection;
        private          string        _hostName;
        private          bool          _isConnected;
        private readonly bool          _isFullScreen = false;
        private          string        _password;
        private          string        _userName;

        public FormRDPShow(RdpConnection connection)
        {
            InitializeComponent();
            _connection                        =  connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.ConnectionStateChanged += Connection_StateChanged;
            var axRdpClient9 = _connection.GetRdpClient();
            axRdpClient9.Dock = DockStyle.Fill;
            splitContainer1.Panel1.Controls.Add(axRdpClient9);
            Text = $"遠端連線 - {_connection.GetHostName()}";

            // 設定視窗的最小尺寸
            MinimumSize = new Size(1920, 1080);
            // 應用主題
            ThemeManager.ApplyTheme(this);
            BackColor = ThemeManager.BackgroundColor;
            splitContainer1.BackColor = ThemeManager.BackgroundColor;
            splitContainer1.Panel1.BackColor = ThemeManager.CardColor;
            splitContainer1.Panel2.BackColor = ThemeManager.SurfaceColor;

            // 美化按鈕
            ThemeManager.ApplyThemeToButton(bntClose, isPrimary: true);
            bntClose.Text = "關閉 (ESC)";
            
            // 隱藏未使用的 button1
            if (button1 != null)
            {
                button1.Visible = false;
            }

            // 註冊事件處理器
            ResizeEnd   += FormRDPShow_ResizeEnd;
            SizeChanged += FormRDPShow_SizeChanged;
            // 初始化時立即更新大小
            UpdateRdpClientSize();
        }

    public event EventHandler<RdpStateEventArgs> ConnectionStateChanged;

        private void Connection_StateChanged(object sender, EventArgs e)
        {
            if (_connection != null) OnConnectionStateChanged(_connection.State);
        }

        protected virtual void OnConnectionStateChanged(RdpState state)
        {
            var args = new RdpStateEventArgs(state, _connection?.ConnectionId);
            ConnectionStateChanged?.Invoke(this, args);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_connection != null)
            {
                _connection.ConnectionStateChanged -= Connection_StateChanged;
                _connection.Disconnect();
                _connection = null;
            }

            base.OnFormClosing(e);
        }

        private void bntClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape && _isFullScreen)
                ToggleFullScreen();
            else if (e.KeyCode == Keys.F11) ToggleFullScreen();
        }

        public void ToggleFullScreen()
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState     = FormWindowState.Normal;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState     = FormWindowState.Maximized;
            }
        }

        private void FormRDPShow_SizeChanged(object sender, EventArgs e)
        {
            UpdateRdpClientSize();
        }

        private void FormRDPShow_ResizeEnd(object sender, EventArgs e)
        {
            UpdateRdpClientSize();
        }

        private void UpdateRdpClientSize()
        {
            if (_connection == null) return;

            var axRdpClient9 = _connection.GetRdpClient();
            if (axRdpClient9 == null || !axRdpClient9.Created) return;

            try
            {
                // 確保在 UI 線程上執行
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(UpdateRdpClientSize));
                    return;
                }

                // 確保最小尺寸和最大尺寸
                var width  = Math.Max(800, Math.Min(splitContainer1.Panel1.ClientSize.Width, 4096));
                var height = Math.Max(600, Math.Min(splitContainer1.Panel1.ClientSize.Height, 2160));

                // 檢查控制項狀態
                if (!axRdpClient9.Enabled || axRdpClient9.IsDisposed) return;

                // 設置基本桌面尺寸
                try
                {
                    if (axRdpClient9.Connected == 1)
                    {
                        axRdpClient9.UpdateSessionDisplaySettings((uint) width, // 寬度
                            (uint) height,                                      // 高度
                            32,                                                 // 色深
                            0,                                                  // 標誌
                            0,                                                  // 方向
                            0,                                                  // DPI X
                            0                                                   // DPI Y
                        );
                    }
                    else
                    {
                        axRdpClient9.DesktopWidth  = width;
                        axRdpClient9.DesktopHeight = height;
                    }
                }
                catch (AxHost.InvalidActiveXStateException ex)
                {
                    Debug.WriteLine($"無法設置桌面尺寸 - 控制項尚未準備好: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"調整遠端桌面大小時發生錯誤：{ex.Message}");
            }
        }
    }
}
