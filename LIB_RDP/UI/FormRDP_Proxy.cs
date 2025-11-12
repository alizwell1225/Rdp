using LIB_RDP.Core;
using LIB_RDP.Models;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace LIB_RDP.UI;

public partial class FormRDP_Proxy : Form
{
    private static readonly string excuPath = Application.StartupPath;
    private static readonly string _debugLogPath = Path.Combine(excuPath, "RdpViewer_debug.log");
    private readonly RdpLogger _logger;

    private readonly RdpManager _rdpManager;
    private readonly Dictionary<string, Uc_Viewer> _viewers;


    public FormRDP_Proxy(RdpManager rdpManager, Dictionary<string, Uc_Viewer> viewers)
    {
        InitializeComponent();
        _logger = RdpLogger.Instance;
        _rdpManager = rdpManager;
        _viewers = viewers;

        ThemeManager.ApplyTheme(this);
    }

    public void ControlsAddMenuStrip(MenuStrip menuStrip)
    {
        Controls.Add(menuStrip);
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
        this.Hide();
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



    private async void ConnectViewer(int index)
    {

        //var hostName = _rdpManager.RdpProxySettings.ConnectionsInfo[index].HostName;
        //var userName = _rdpManager.RdpProxySettings.ConnectionsInfo[index].UserName;
        //var password = _rdpManager.RdpProxySettings.ConnectionsInfo[index].Password;

        //if (string.IsNullOrEmpty(hostName) || string.IsNullOrEmpty(userName))
        //{
        //    MessageBox.Show("請輸入主機位址和使用者名稱", "連線資訊不完整", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    return;
        //}

        var viewer = _viewers[$"viewer_{index}"];
        var connection = new RdpConnection();
        connection.Configure(new RdpConfig { ScreenWidth = viewer.InnerViewer.Width, ScreenHeight = viewer.InnerViewer.Height, ColorDepth = 64 });

        // 顯示連線中
        //_rdpManager.RdpProxySettings.ConnectionsInfo[index].Status = "連線中";
        viewer.SetRdpConnection(connection);


        // 啟動非同步連線，待最終驗證結果；UI 由事件驅動更新
        //var success = await connection.ConnectAsync(hostName, userName, password);
        //if (!success) 
        //    _logger.LogWarning($"連線到 {hostName} 失敗");
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
           // _rdpManager.RdpProxySettings.ConnectionsInfo[index].Status = "未連線";
        }
        catch (Exception ex)
        {
            _logger.LogError($"中斷連線 #{index + 1} 時發生異常", ex);
            //_rdpManager.RdpProxySettings.ConnectionsInfo[index].Status = $"中斷連線 #{index + 1} 時發生異常";
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        _rdpManager.Dispose();
    }

    private void FormRDP_Proxy_Load(object sender, EventArgs e)
    {
       // _logger.LogInfo($"FormRDP_Proxy 已載入，初始連線數: {_rdpManager.RdpProxySettings.RpcCount}");
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


    private void SafeInvoke(Action action)
    {
        if (InvokeRequired) BeginInvoke(action);
        else action();
    }
}


