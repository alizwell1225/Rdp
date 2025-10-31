using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LIB_RDP.Core;
using LIB_RDP.Models;
using LIB_RDP.UI;

namespace RDP_DEMO;

public partial class FormRDP_Proxy : Form, IFormRDP
{
    private static readonly string excuPath = Application.StartupPath;
    private static readonly string _debugLogPath = Path.Combine(excuPath, "RdpViewer_debug.log");
    private readonly RdpLogger _logger;
    private readonly MenuManager _menuManager;
    private readonly MenuStrip _menuStrip = new();

    private readonly RdpManager _rdpManager;
    private readonly Dictionary<string, Uc_Viewer> _viewers;
    private int _autoHideDelay = 5000;
    private bool _autoHideEnabled;
    private Timer _autoHideTimer;
    
    // Proxy settings
    private ProxySettings _proxySettings;

    public FormRDP_Proxy()
    {
        InitializeComponent();
        _logger = RdpLogger.Instance;
        _menuManager = new MenuManager(this, _menuStrip);
        InitializeAutoHideTimer();
        _rdpManager = new RdpManager();
        _viewers = new Dictionary<string, Uc_Viewer>();
        _proxySettings = new ProxySettings();
        InitializeDataGridView();
        SetupEventHandlers();
        AddProxyMenu();
        ThemeManager.ApplyTheme(this);
    }

    public bool AutoHideEnabled
    {
        get => _autoHideEnabled;
        set
        {
            _autoHideEnabled = value;
            if (!_autoHideEnabled)
            {
                ShowTopPanel();
                _autoHideTimer?.Stop();
            }
            else
            {
                _autoHideTimer?.Stop();
                _autoHideTimer?.Start();
            }
        }
    }

    public int AutoHideDelay
    {
        get => _autoHideDelay;
        set
        {
            if (value <= 0) return;
            _autoHideDelay = value;
            UpdateAutoHideTimerInterval();
        }
    }

    public void HideTopPanel()
    {
        pnlTop.Visible = false;
        UpdatePreviewPanelSize();
    }

    public void ControlsAddMenuStrip(MenuStrip menuStrip)
    {
        Controls.Add(menuStrip);
    }

    public void ShowTopPanel()
    {
        pnlTop.Visible = true;
        UpdatePreviewPanelSize();
    }

    public void UpdateAutoHideTimerInterval()
    {
        _autoHideTimer.Interval = _autoHideDelay;
    }

    public void CloseApplication()
    {
        var result = MessageBox.Show("確定要關閉程式嗎？", "關閉確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
        if (result == DialogResult.Yes)
        {
            foreach (var viewer in _viewers.Values)
            {
                var connection = viewer.GetConnection();
                if (connection != null)
                {
                    connection.Disconnect();
                    connection.Dispose();
                }
            }

            Application.Exit();
        }
    }

    private void AddProxyMenu()
    {
        var settingsMenu = new ToolStripMenuItem("設定(&S)");
        var configMenuItem = new ToolStripMenuItem("RPC 連線設定(&C)", null, OpenProxySettings);
        settingsMenu.DropDownItems.Add(configMenuItem);
        _menuStrip.Items.Insert(0, settingsMenu);
    }

    private void OpenProxySettings(object sender, EventArgs e)
    {
        using (var settingsForm = new FormProxySettings(_proxySettings))
        {
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                // Apply settings
                numConnections.Value = _proxySettings.RpcCount;
                ApplyConnectionSettings();
            }
        }
    }

    private void ApplyConnectionSettings()
    {
        for (int i = 0; i < _proxySettings.Connections.Count && i < dgvConnections.Rows.Count; i++)
        {
            var conn = _proxySettings.Connections[i];
            var row = dgvConnections.Rows[i];
            row.Cells["HostName"].Value = conn.HostName;
            row.Cells["UserName"].Value = conn.UserName;
            row.Cells["Password"].Value = conn.Password;
        }
    }

    /// <summary>
    /// API method to open a specific RPC viewer in fullscreen
    /// </summary>
    /// <param name="rpcIndex">0-based index of the RPC to open</param>
    public void OpenRpcFullscreen(int rpcIndex)
    {
        if (rpcIndex < 0 || rpcIndex >= _viewers.Count)
        {
            _logger.LogWarning($"Invalid RPC index: {rpcIndex}");
            return;
        }

        var viewerKey = $"viewer_{rpcIndex}";
        if (!_viewers.ContainsKey(viewerKey))
        {
            _logger.LogWarning($"Viewer not found: {viewerKey}");
            return;
        }

        var viewer = _viewers[viewerKey];
        if (!viewer.IsFullScreen)
        {
            ToggleViewerSize(rpcIndex);
        }
        
        _logger.LogInfo($"Opened RPC {rpcIndex} in fullscreen");
    }

    private void InitializeDataGridView()
    {
        ThemeManager.ApplyThemeToDataGridView(dgvConnections);

        dgvConnections.Columns.Add("HostIndex", "PC No");
        dgvConnections.Columns.Add("HostName", "主機位址");
        dgvConnections.Columns.Add("UserName", "使用者名稱");

        var passwordColumn = new DataGridViewTextBoxColumn
        {
            Name = "Password",
            HeaderText = "密碼",
            DefaultCellStyle = new DataGridViewCellStyle { NullValue = "••••••" }
        };
        dgvConnections.Columns.Add(passwordColumn);

        var statusColumn = new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "連線狀態",
            Width = 100
        };
        dgvConnections.Columns.Add(statusColumn);

        var connectColumn = new DataGridViewButtonColumn
        {
            Name = "Connect",
            HeaderText = "連線",
            Text = "連線",
            UseColumnTextForButtonValue = true,
            Width = 80
        };

        var disconnectColumn = new DataGridViewButtonColumn
        {
            Name = "Disconnect",
            HeaderText = "斷線",
            Text = "斷線",
            UseColumnTextForButtonValue = true,
            Width = 80
        };

        dgvConnections.Columns.Add(connectColumn);
        dgvConnections.Columns.Add(disconnectColumn);

        dgvConnections.Columns["HostIndex"].Width = 60;
        dgvConnections.Columns["HostName"].Width = 180;
        dgvConnections.Columns["UserName"].Width = 150;
        dgvConnections.Columns["Password"].Width = 120;

        dgvConnections.CellPainting += DgvConnections_CellPainting;
    }

    private void SetupEventHandlers()
    {
        numConnections.ValueChanged += NumConnections_ValueChanged;
        dgvConnections.CellClick += DgvConnections_CellClick;
        dgvConnections.CellDoubleClick += DgvConnections_CellDoubleClick;
    }

    private void NumConnections_ValueChanged(object sender, EventArgs e)
    {
        var count = (int)numConnections.Value;
        UpdateConnectionGrid(count);
        UpdatePreviewPanel(count);
    }

    private void UpdateConnectionGrid(int count)
    {
        var currentRows = dgvConnections.Rows.Count;
        if (count > currentRows)
            for (var i = currentRows; i < count; i++)
            {
                dgvConnections.Rows.Add();
                dgvConnections.Rows[i].Cells["HostIndex"].Value = i + 1;
                dgvConnections.Rows[i].Cells["Status"].Value = "未連線";
                dgvConnections.Rows[i].Cells["Status"].Style.ForeColor = ThemeManager.SecondaryForegroundColor;
            }
        else if (count < currentRows)
            for (var i = currentRows - 1; i >= count; i--)
            {
                DisconnectViewer(i);
                dgvConnections.Rows.RemoveAt(i);
            }
    }

    private void UpdatePreviewPanel(int count)
    {
        var existingConnections = new Dictionary<string, RdpConnection>();
        foreach (var kv in _viewers)
        {
            var conn = kv.Value.GetConnection();
            if (conn != null) existingConnections.Add(kv.Key, conn);
        }

        pnlPreviews.Controls.Clear();
        _viewers.Clear();

        if (count == 0) return;

        var cols = (int)Math.Ceiling(Math.Sqrt(count));
        var rows = (int)Math.Ceiling((double)count / cols);
        var width = Math.Max(1, pnlPreviews.ClientSize.Width / cols);
        var height = Math.Max(1, pnlPreviews.ClientSize.Height / rows);

        for (var i = 0; i < count; i++)
        {
            var ucViewer = new Uc_Viewer();
            ucViewer.Init(width, height, i, cols, existingConnections);
            ucViewer.ActMaxViewer += ActMaxViewer;
            ucViewer.ActSinkViewer += ActSinkViewer;
            pnlPreviews.Controls.Add(ucViewer);

            var key = $"viewer_{i}";
            _viewers.Add(key, ucViewer);

            try
            {
                File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [FormRDP_Proxy] Created \r\n");
            }
            catch
            {
            }
        }

        foreach (var c in existingConnections.Values)
        {
            if (_viewers.Values.Any(v => v.GetConnection() == c)) 
                continue;
            try
            {
                c.Disconnect();
            }
            catch
            {
            }

            try
            {
                c.Dispose();
            }
            catch
            {
            }
        }
        Invalidate();
    }

    private void ActSinkViewer(int index)
    {
        UpdatePreviewPanelSize();
    }

    private void ActMaxViewer(int index)
    {
        ToggleViewerSize(index);
    }


    /// <summary>
    ///     設定指定 viewer 的狀態文字與可見性
    /// </summary>
    public void SetViewerStatus(int index, string text, bool visible = true)
    {
        var key = $"viewer_{index}";
        if (!_viewers.ContainsKey(key)) return;
        var viewer = _viewers[key];
        viewer.SetStatusText(text, visible);
    }


    private void UpdateStatusFromConnection(RdpConnection connection, DataGridViewRow row)
    {
        if (connection == null || row == null) return;

        if (connection.State == RdpState.Connecting || connection.State == RdpState.Retrying)
        {
            row.Cells["Status"].Value = "連線中";
            row.Cells["Status"].Style.ForeColor = ThemeManager.WarningColor;
        }
        else if (connection.State == RdpState.Connected && connection.IsConnected)
        {
            row.Cells["Status"].Value = "已連線";
            row.Cells["Status"].Style.ForeColor = ThemeManager.SuccessColor;
        }
        else
        {
            row.Cells["Status"].Value = "未連線";
            row.Cells["Status"].Style.ForeColor = ThemeManager.SecondaryForegroundColor;
        }
    }

    private void DgvConnections_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        if (e.ColumnIndex == dgvConnections.Columns["Connect"].Index)
            ConnectViewer(e.RowIndex);
        else if (e.ColumnIndex == dgvConnections.Columns["Disconnect"].Index)
            DisconnectViewer(e.RowIndex);
    }

    private async void ConnectViewer(int index)
    {
        var row = dgvConnections.Rows[index];
        var hostName = row.Cells["HostName"].Value?.ToString();
        var userName = row.Cells["UserName"].Value?.ToString();
        var password = row.Cells["Password"].Value?.ToString();

        if (string.IsNullOrEmpty(hostName) || string.IsNullOrEmpty(userName))
        {
            MessageBox.Show("請輸入主機位址和使用者名稱", "連線資訊不完整", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var viewer = _viewers[$"viewer_{index}"];
        var connection = new RdpConnection();
        connection.Configure(new RdpConfig { ScreenWidth = viewer.InnerViewer.Width, ScreenHeight = viewer.InnerViewer.Height, ColorDepth = 64 });

        // 顯示連線中
        row.Cells["Status"].Value = "連線中";
        row.Cells["Status"].Style.ForeColor = ThemeManager.WarningColor;

        viewer.SetRdpConnection(connection);

        var stateHandler = StateHandler(connection, row);
        connection.ConnectionStateChanged += stateHandler;

        // 啟動非同步連線，待最終驗證結果；UI 由事件驅動更新
        var success = await connection.ConnectAsync(hostName, userName, password);
        if (!success) 
            _logger.LogWarning($"連線到 {hostName} 失敗");
    }

    private EventHandler StateHandler(RdpConnection connection, DataGridViewRow row)
    {
        EventHandler stateHandler = null;
        stateHandler = (s, e) =>
        {
            SafeInvoke(() =>
            {
                UpdateStatusFromConnection(connection, row);
                if (connection.State == RdpState.Connected || connection.State == RdpState.Error || connection.State == RdpState.Disconnected) 
                    connection.ConnectionStateChanged -= stateHandler;
            });
        };
        return stateHandler;
    }


    private void DisconnectViewer(int index)
    {
        try
        {
            var viewer = _viewers[$"viewer_{index}"];
            if (viewer != null)
            {
                try
                {
                    if (viewer.InnerViewer.Tag is BottomMenuController ctrl)
                    {
                        try
                        {
                            ctrl.Dispose();
                        }
                        catch
                        {
                        }

                        try
                        {
                            viewer.InnerViewer.Controls.Remove(ctrl.Panel);
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }

                var connection = viewer.GetConnection();
                if (connection != null)
                {
                    connection.Disconnect();
                    connection.Dispose();
                    _logger.LogInfo($"已中斷連線 #{index + 1}");
                }

                viewer.SetRdpConnection(null);
            }

            var row = dgvConnections.Rows[index];
            row.Cells["Status"].Value = "未連線";
            row.Cells["Status"].Style.ForeColor = ThemeManager.SecondaryForegroundColor;
        }
        catch (Exception ex)
        {
            _logger.LogError($"中斷連線 #{index + 1} 時發生異常", ex);
            MessageBox.Show($"中斷連線失敗: {ex.Message}", "斷線錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        _rdpManager.Dispose();
    }

    private void FormRDP_Proxy_Load(object sender, EventArgs e)
    {
        numConnections.Value = _proxySettings.RpcCount;
        ApplyConnectionSettings();
        _autoHideEnabled = true;
        HideTopPanel();
        _logger.LogInfo($"FormRDP_Proxy 已載入，初始連線數: {_proxySettings.RpcCount}");
    }

    private void DgvConnections_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.ColumnIndex == dgvConnections.Columns["Status"].Index && e.RowIndex >= 0)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            var status = e.Value?.ToString() ?? "未連線";
            var statusColor = ThemeManager.GetConnectionStateColor(status);

            var dotSize = 10;
            var dotX = e.CellBounds.X + 10;
            var dotY = e.CellBounds.Y + (e.CellBounds.Height - dotSize) / 2;

            using (var brush = new SolidBrush(statusColor))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, dotX, dotY, dotSize, dotSize);
            }

            var textRect = new Rectangle(dotX + dotSize + 8, e.CellBounds.Y,
                e.CellBounds.Width - (dotX + dotSize + 8 - e.CellBounds.X), e.CellBounds.Height);

            TextRenderer.DrawText(e.Graphics, status, e.CellStyle.Font, textRect, e.CellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            e.Handled = true;
        }
    }

    private void UpdatePreviewPanelSize()
    {
        try
        {
            if (_menuManager == null) return;
            var availableHeight = ClientSize.Height - (pnlTop.Visible ? pnlTop.Height : 0) - _menuManager.MenuStrip.Height;
            pnlPreviews.Height = availableHeight;

            if (_viewers?.Count > 0)
                UpdatePreviewPanel((int)numConnections.Value);
        }
        catch (Exception)
        {
        }
    }

    private void DgvConnections_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (e.ColumnIndex == dgvConnections.Columns["HostIndex"].Index) ToggleViewerSize(e.RowIndex);
    }

    private void ToggleViewerSize(int index)
    {
        var viewerKey = $"viewer_{index}";
        if (!_viewers.ContainsKey(viewerKey)) return;
        var uc = _viewers[viewerKey];
        var connection = uc.GetConnection();
        if (connection != null)
            connection.Configure(new RdpConfig
                { ScreenWidth = uc.InnerViewer.Width, ScreenHeight = uc.InnerViewer.Height, ColorDepth = 64 });

        if (!uc.IsFullScreen)
        {
            foreach (var otherUc in _viewers.Values)
                if (otherUc != uc)
                    otherUc.Visible = false;
            uc.SaveCurrentState();
            uc.SetMax(pnlPreviews.Width, pnlPreviews.Height);
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();
        }
        else
        {
            uc.RestoreOriginalState();
            foreach (var otherUc in _viewers.Values) otherUc.Visible = true;
        }
    }


    private void InitializeAutoHideTimer()
    {
        _autoHideTimer = new Timer { Interval = _autoHideDelay, Enabled = false };
        _autoHideTimer.Tick += AutoHideTimer_Tick;
    }

    private void AutoHideTimer_Tick(object sender, EventArgs e)
    {
        if (_autoHideEnabled)
        {
            HideTopPanel();
            _autoHideTimer.Stop();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_autoHideEnabled)
        {
            var mousePosition = PointToClient(Cursor.Position);
            if (mousePosition.Y <= pnlTop.Height)
            {
                pnlTop.Visible = true;
                _autoHideTimer.Stop();
                _autoHideTimer.Start();
            }
        }
    }

    private void pnlTop_MouseEnter(object sender, EventArgs e)
    {
        if (_autoHideEnabled)
        {
            _autoHideTimer.Stop();
            if (!pnlTop.Visible)
            {
                pnlTop.Visible = true;
                UpdatePreviewPanelSize();
            }
        }
    }

    private void pnlTop_MouseLeave(object sender, EventArgs e)
    {
        if (_autoHideEnabled) _autoHideTimer.Start();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdatePreviewPanelSize();
    }

    private void SafeInvoke(Action action)
    {
        if (InvokeRequired) BeginInvoke(action);
        else action();
    }
}

// Proxy settings data class
public class ProxySettings
{
    public int RpcCount { get; set; } = 12;
    public List<ConnectionInfo> Connections { get; set; } = new List<ConnectionInfo>();

    public ProxySettings()
    {
        // Initialize with default connections
        for (int i = 0; i < 12; i++)
        {
            Connections.Add(new ConnectionInfo());
        }
    }
}

public class ConnectionInfo
{
    public string HostName { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
