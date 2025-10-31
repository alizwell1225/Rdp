using LIB_RDP.Core;
using LIB_RDP.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace LIB_RDP.UI;

public partial class FormRDP : Form, IFormRDP
{
    private const int DEFAULT_PREVIEW_SIZE = 320;
    private static readonly string _debugLogPath = Path.Combine(Path.GetTempPath(), "RdpViewer-debug.log");
    private readonly RdpLogger _logger;
    private readonly MenuManager _menuManager;
    private readonly RdpManager _rdpManager;
    private readonly Dictionary<string, Uc_Viewer> _viewers;
    private readonly IPreviewManager _previewManager;
    private int _autoHideDelay = 5000;
    private bool _autoHideEnabled;
    private Timer _autoHideTimer;
    private readonly MenuStrip _menuStrip = new();

    public FormRDP()
    {
        InitializeComponent();
        _logger = RdpLogger.Instance;
        _menuManager = new MenuManager(this, _menuStrip);
        InitializeAutoHideTimer();
        _rdpManager = new RdpManager();
        _viewers = new Dictionary<string, Uc_Viewer>();
        _previewManager = new PreviewManager();
        _previewManager.ViewerStatusChanged += PreviewManager_ViewerStatusChanged;
        InitializeDataGridView();
        SetupEventHandlers();
        SizeChanged += FormRDP_SizeChanged;
        ThemeManager.ApplyTheme(this);
    }

    // Public properties used by MenuManager
    public bool AutoHideEnabled
    {
        get => _autoHideEnabled;
        set
        {
            _autoHideEnabled = value;
            if (!_autoHideEnabled)
            {
                // ensure panel is visible when auto-hide is disabled
                ShowTopPanel();
                _autoHideTimer?.Stop();
            }
            else
            {
                // start timer when enabled
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
            Uc_Viewer ucViewer = new Uc_Viewer();
            ucViewer.Init(width, height,i,cols, existingConnections);
            ucViewer.ActMaxViewer+= ActMaxViewer;
            ucViewer.ActSinkViewer+= ActSinkViewer;
            // register with the preview manager (decouples FormRDP from direct management)
            try { _previewManager.RegisterViewer(ucViewer); } catch { }
            pnlPreviews.Controls.Add(ucViewer);
            var key = $"viewer_{i}";
            _viewers.Add(key, ucViewer);

            try
            {
                File.AppendAllText(_debugLogPath, $"{DateTime.Now:O} [FormRDP] Created \r\n");
            }
            catch
            {
            }
        }

        foreach (var c in existingConnections.Values)
            if (!_viewers.Values.Any(v => v.GetConnection() == c))
            {
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

    private void PreviewManager_ViewerStatusChanged(int index, string action, bool visible)
    {
        // map PreviewManager notifications to existing behaviours
        if (action == "maximize") ToggleViewerSize(index);
        else if (action == "restore") ToggleViewerSize(index);
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

    public void ToggleViewerFullScreen(int index)
    {
        ToggleViewerSize(index);
    }

    // Called by Uc_Viewer's Full button to maximize a viewer
    public void MaximizeViewer(int index)
    {
        // reuse ToggleViewerSize behaviour to enter fullscreen for the specified index
        if (index < 0) return;
        ToggleViewerSize(index);
    }

    // Called by Uc_Viewer's Sink button to restore a viewer
    public void RestoreViewer(int index)
    {
        if (index < 0) return;
        // If the viewer is currently fullscreen, ToggleViewerSize will restore it
        ToggleViewerSize(index);
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
        connection.Configure(
            new RdpConfig { ScreenWidth = viewer.InnerViewer.Width, ScreenHeight = viewer.InnerViewer.Height, ColorDepth = 32 });

        // 顯示連線中
        row.Cells["Status"].Value = "連線中";
        row.Cells["Status"].Style.ForeColor = ThemeManager.WarningColor;

        viewer.SetRdpConnection(connection);

        EventHandler stateHandler = null;
        stateHandler = (s, e) =>
        {
            SafeInvoke(() =>
            {
                UpdateStatusFromConnection(connection, row);
                if (connection.State == RdpState.Connected || connection.State == RdpState.Error ||
                    connection.State == RdpState.Disconnected) connection.ConnectionStateChanged -= stateHandler;
            });
        };

        connection.ConnectionStateChanged += stateHandler;

        // 啟動非同步連線，等待最終驗證結果；UI 由事件驅動更新
        var success = await connection.ConnectAsync(hostName, userName, password);
        if (!success) _logger.LogWarning($"連線到 {hostName} 失敗");
    }

    private void DisconnectViewer(int index)
    {
        try
        {
            var viewer = _viewers[$"viewer_{index}"];
            if (viewer != null)
            {
                // cleanup any BottomMenuController stored in Tag (if legacy code added one)
                try
                {
                    if (viewer.InnerViewer.Tag is LIB_RDP.UI.BottomMenuController ctrl)
                    {
                        try { ctrl.Dispose(); } catch { }
                        try { viewer.InnerViewer.Controls.Remove(ctrl.Panel); } catch { }
                    }
                }
                catch { }

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

    private void FormRDP_Load(object sender, EventArgs e)
    {
        numConnections.Value = 12;
        _autoHideEnabled = true;
        HideTopPanel();
        _logger.LogInfo("FormRDP 已載入，初始連線數: 12");
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

    public void HideTopPanel()
    {
        pnlTop.Visible = false;
        UpdatePreviewPanelSize();
    }

    public void ControlsAddMenuStrip(MenuStrip menuStrip)
    {
        this.Controls.Add(menuStrip);
    }

    public void ShowTopPanel()
    {
        pnlTop.Visible = true;
        UpdatePreviewPanelSize();
    }

    private void UpdatePreviewPanelSize()
    {
        try
        {
            if (_menuManager == null) return;
            var availableHeight = ClientSize.Height - (pnlTop.Visible ? pnlTop.Height : 0) -
                                  _menuManager.MenuStrip.Height;
            pnlPreviews.Height = availableHeight;

            if (_viewers?.Count > 0) UpdatePreviewPanel((int)numConnections.Value);
        }
        catch (Exception)
        {
        }
    }

    private void btnSetDemoIP_Click(object sender, EventArgs e)
    {
        try
        {
            if (dgvConnections.Rows.Count == 0) numConnections.Value = 2;
            var firstRow = dgvConnections.Rows[0];
            firstRow.Cells["HostName"].Value = "169.254.28.154";
            firstRow.Cells["UserName"].Value = "myuser";
            firstRow.Cells["Password"].Value = "1234";
            var Row2 = dgvConnections.Rows[1];
            Row2.Cells["HostName"].Value = "169.254.63.243";
            Row2.Cells["UserName"].Value = "gavin";
            Row2.Cells["Password"].Value = "Lau72573000";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"設定示範 IP 時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                { ScreenWidth = uc.InnerViewer.Width, ScreenHeight = uc.InnerViewer.Height, ColorDepth = 32 });

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

    private void FormRDP_SizeChanged(object sender, EventArgs e)
    {
        if (_viewers.Count > 0) UpdatePreviewPanel((int)numConnections.Value);
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

    // helper for invoking on UI thread
    private void SafeInvoke(Action action)
    {
        if (InvokeRequired) BeginInvoke(action);
        else action();
    }
}