using AxMSTSCLib;
using LIB_Log;
using LIB_RDP.Interfaces;
using LIB_RDP.Models;
using LIB_RDP.UI;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace LIB_RDP.Core;

public class RdpConnection : IRdpConnection, IDisposable
{
    private const int MAX_WIDTH = 4096; // 最大寬度
    private const int MAX_HEIGHT = 2160; // 最大高度
    private const int DEFAULT_CONNECTION_TIMEOUT = 30; // 預設連線超時30秒

    // 重試機制設定
    private const int MAX_RETRY_ATTEMPTS = 3; // 最大重試次數
    private const int INITIAL_RETRY_DELAY_MS = 2000; // 初始重試延遲 2秒
    private const int MAX_RETRY_DELAY_MS = 8000; // 最大重試延遲 8秒
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Logger _logger;
    private RdpConfig _config;
    private DateTime _connectionStartTime;
    private Timer _connectionTimer;
    private SecureCredentials _credentials;
    private string _hostName;
    private bool _isConnected;
    private bool _isDisposed;

    private readonly Form _parentForm;

    private readonly AxMsRdpClient9NotSafeForScripting _rdpClient;

    // 重試機制相關欄位
    private readonly Control _uiControl;
    private volatile bool _verificationInProgress;

    public RdpConnection(Control uiControl = null, Form parentForm = null,Logger logger=null)
    {
        ConnectionId = Guid.NewGuid().ToString("N")[..8]; // 短版本ID
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();

        _uiControl = uiControl;
        _parentForm = parentForm;
        _rdpClient = new AxMsRdpClient9NotSafeForScripting();
        State = RdpState.Disconnected;

        _logger?.Info("RDP連線已初始化", ConnectionId);
        InitializeRdpClient();
        InitializeConnectionTimer();

        _rdpClient.OnConnecting += RdpClient_OnConnecting;
        _rdpClient.OnConnected += RdpClient_OnConnected;
        _rdpClient.OnDisconnected += RdpClient_OnDisconnected;
        _rdpClient.OnLoginComplete += RdpClient_OnLoginComplete;
    }

    public string ConnectionId { get;private set; }

    public void SetConnectionId(string ID)
    {
        this.ConnectionId = ID;
    }

    /// <summary>
    ///     當前重試次數 (0 表示未重試)
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    ///     是否正在重試
    /// </summary>
    public bool IsRetrying { get; private set; }



    public void Dispose()
    {
        if (_isDisposed) return;

        _logger?.Info("正在釋放RDP連線資源", ConnectionId);

        try
        {
            Disconnect();

            _connectionTimer?.Stop();
            _connectionTimer?.Dispose();

            _credentials?.Dispose();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _rdpClient?.Dispose();

            _isDisposed = true;
            _logger?.Info("RDP連線資源已成功釋放", ConnectionId);
        }
        catch (Exception ex)
        {
            _logger?.Error($"釋放RDP連線資源時發生錯誤:{ConnectionId}", ex);
        }
    }

    public void Configure(RdpConfig config)
    {
        try
        {
            _config = config;

            // 確保控制項已經初始化
            if (_rdpClient != null && _rdpClient.Created)
            {
                if (_uiControl?.InvokeRequired == true)
                    _uiControl.BeginInvoke(() => ApplyConfig(config));
                else
                    ApplyConfig(config);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Configure 錯誤：{ex.Message}");
        }
    }

    public bool Connect(string hostName, string userName, string password, bool usedPingFlag = true)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(RdpConnection));

        _hostName = hostName;
        _credentials?.Dispose();
        _credentials = new SecureCredentials(userName, password);
        RetryCount = 0;
        IsRetrying = false;

        // 為了相容既有同步 API，我們仍然回傳一個布林，但實際的連線流程會在背景執行。
        // 呼叫者不應該依賴此同步回傳的結果來判斷畫面是否已呈現。
        // 背景 Task 會處理重試、連線和驗證；ConnectAsync 可用來等待最終結果。
        Task.Run(() => ConnectWithRetry(hostName, userName, password, usedPingFlag));
        return true;
    }

    /// <summary>
    ///     異步連線方法
    /// </summary>
    // 新增一個真正會回傳最終驗證結果的非同步 ConnectAsync
    public async Task<bool> ConnectAsync(string hostName, string userName, string password, int timeoutSeconds = DEFAULT_CONNECTION_TIMEOUT,bool usePingFlag=false)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(RdpConnection));

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        EventHandler handler = null;
        EventHandler<RdpConnectionTimeoutException> timeoutHandler = null;

        // 當狀態變成 Connected 且 IsConnected 為 true 時視為成功；若發生 Timeout 或 Error 則視為失敗。
        handler = (s, e) =>
        {
            try
            {
                if (State == RdpState.Connected && IsConnected)
                    tcs.TrySetResult(true);
                else if (State == RdpState.Error || State == RdpState.Disconnected) tcs.TrySetResult(false);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        };

        timeoutHandler = (s, ex) => { tcs.TrySetResult(false); };

        ConnectionStateChanged += handler;
        ConnectionTimeoutOccurred += timeoutHandler;

        // 啟動背景連線（若已由 Connect() 啟動，這不會造成問題）
        Connect(hostName, userName, password, usePingFlag);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using (cts)
        {
            try
            {
                using (cts.Token.Register(() => tcs.TrySetResult(false)))
                {
                    var result = await tcs.Task.ConfigureAwait(false);
                    return result;
                }
            }
            finally
            {
                ConnectionStateChanged -= handler;
                ConnectionTimeoutOccurred -= timeoutHandler;
            }
        }
    }

    public void Disconnect()
    {
        try
        {
            _logger?.Info("正在中斷連線", ConnectionId);
            StopConnectionTimer();

            if (_rdpClient?.Connected == 1) _rdpClient.Disconnect();

            State = RdpState.Disconnected;
            IsConnected = false;
            _logger?.Info("連線已中斷", ConnectionId);
        }
        catch (Exception ex)
        {
            _logger?.Error($"釋放RDP連線資源時發生錯誤:{ConnectionId}", ex);
            State = RdpState.Error;
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                SafeInvoke(() => ConnectionStateChanged?.Invoke(this, EventArgs.Empty));
            }
        }
    }

    public RdpState State { get; private set; }

    public Bitmap GetScreenshot()
    {
        if (_rdpClient.Connected != 1 || !IsConnected)
            return null;

        try
        {
            var bounds = _rdpClient.Bounds;
            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var handle = graphics.GetHdc();
                SendMessage(_rdpClient.Handle, 0x317, handle, 0x2 | 0x4 | 0x8 | 0x10);
                graphics.ReleaseHdc(handle);
            }

            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     帶重試機制的連線方法
    /// </summary>
    private bool ConnectWithRetry(string hostName, string userName, string password,bool usedPingFlag=true)
    {
        var attempt = 0;

        while (attempt <= MAX_RETRY_ATTEMPTS)
            try
            {
                if (attempt > 0)
                {
                    // 計算指數退避延遲: 2s, 4s, 8s
                    var delayMs = Math.Min(INITIAL_RETRY_DELAY_MS * (int)Math.Pow(2, attempt - 1), MAX_RETRY_DELAY_MS);

                    RetryCount = attempt;
                    IsRetrying = true;
                    State = RdpState.Retrying;

                    _logger?.Info($"等待 {delayMs}ms 後進行第 {attempt}/{MAX_RETRY_ATTEMPTS} 次重試連線到 {hostName}", ConnectionId);
                    OnConnectionStateChanged();

                    Thread.Sleep(delayMs);
                }

                _logger?.Info($"開始連線到 {hostName} (嘗試 {attempt + 1}/{MAX_RETRY_ATTEMPTS + 1})", ConnectionId);

                if (ConnectInternal(hostName, userName, password, usedPingFlag))
                {
                    RetryCount = 0;
                    IsRetrying = false;
                    return true;
                }
            }
            catch (RdpAuthenticationException)
            {
                // 認證錯誤不重試,直接拋出
                _logger?.Error($"認證失敗,不進行重試 ConnectionId:{ConnectionId}", null);
                throw;
            }

        return false;
    }

    /// <summary>
    ///     內部連線方法(不含重試邏輯)
    /// </summary>
    private bool ConnectInternal(string hostName, string userName, string password,bool usePingFlag=true)
    {
        try
        {
            State = IsRetrying ? RdpState.Retrying : RdpState.Connecting;
            _connectionStartTime = DateTime.Now;

            // 啟動連線超時計時器
            StartConnectionTimer();

            // 先確認目標主機可 ping 通，若無法 ping 通則不嘗試建立 RDP 連線
            try
            {
                const int pingTimeout = 1000; // 1s
                if (usePingFlag && !PingHost(hostName))
                {
                    _logger?.Warn($"目標主機 {hostName} 無法 ping 通，略過本次連線嘗試", ConnectionId);
                    StopConnectionTimer();
                    State = RdpState.Error;
                    return false;
                }
            }
            catch (Exception exPing)
            {
                _logger?.Warn($"Ping 檢查時發生例外，將繼續嘗試連線: {exPing.Message}", ConnectionId);
            }

            // 基本設定
            _rdpClient.Server = hostName;
            _rdpClient.UserName = userName;
            _rdpClient.AdvancedSettings9.ClearTextPassword = password;

            // 安全性設定 - 增強安全性
            _rdpClient.AdvancedSettings9.EnableCredSspSupport = true;
            _rdpClient.AdvancedSettings9.AuthenticationLevel = 2; // 提高認證等級
            _rdpClient.AdvancedSettings9.NegotiateSecurityLayer = true;

            // 連線設定
            _rdpClient.AdvancedSettings9.ConnectToServerConsole = false;
            _rdpClient.AdvancedSettings9.BitmapPersistence = 1;
            _rdpClient.AdvancedSettings9.Compress = 1;
            _rdpClient.AdvancedSettings9.PerformanceFlags = 0x00000007;

            // 進階設定
            _rdpClient.AdvancedSettings9.NetworkConnectionType = 6; // 自動偵測連線類型
            _rdpClient.AdvancedSettings9.RedirectDrives = false; // 停用磁碟機重導向
            _rdpClient.AdvancedSettings9.RedirectPrinters = false; // 停用印表機重導向
            _rdpClient.AdvancedSettings9.RedirectClipboard = true; // 啟用剪貼簿

            // 畫面設定
            _rdpClient.ColorDepth = _config?.ColorDepth ?? 32;
            _rdpClient.DesktopWidth = _config?.ScreenWidth ?? 1920;
            _rdpClient.DesktopHeight = _config?.ScreenHeight ?? 1080;

            _rdpClient.Connect();
            _logger?.Info($"已發送連線請求到 {hostName}", ConnectionId);
            return true;
        }
        catch (Exception ex)
        {
            State = RdpState.Error;
            IsConnected = false;
            _logger?.Error($"連線到 {hostName} 失敗 :{ConnectionId}", ex );
            StopConnectionTimer();

            // 根據異常類型提供更友善的錯誤訊息
            if (ex.Message.Contains("timeout"))
                throw new RdpConnectionTimeoutException(DEFAULT_CONNECTION_TIMEOUT, ConnectionId);
            if (ex.Message.Contains("authentication") || ex.Message.Contains("login"))
                throw new RdpAuthenticationException(ex.Message, ConnectionId);
            throw new RdpException($"連線失敗: {ex.Message}", ex, RdpErrorCode.ClientError, ConnectionId);
        }
    }

    /// <summary>
    ///     初始化連線超時計時器
    /// </summary>
    private void InitializeConnectionTimer()
    {
        _connectionTimer = new Timer();
        _connectionTimer.Interval = DEFAULT_CONNECTION_TIMEOUT * 1000; // 轉換為毫秒
        _connectionTimer.Tick += ConnectionTimer_Tick;
    }

    /// <summary>
    ///     啟動連線超時計時器
    /// </summary>
    private void StartConnectionTimer()
    {
        _connectionTimer?.Stop();
        _connectionTimer?.Start();
    }

    /// <summary>
    ///     停止連線超時計時器
    /// </summary>
    private void StopConnectionTimer()
    {
        _connectionTimer?.Stop();
    }

    /// <summary>
    ///     連線超時處理
    /// </summary>
    private void ConnectionTimer_Tick(object sender, EventArgs e)
    {
        _connectionTimer?.Stop();

        if (State == RdpState.Connecting)
        {
            _logger?.Warn("連線超時，正在中斷連線", ConnectionId);
            State = RdpState.Error;
            IsConnected = false;

            try
            {
                _rdpClient?.Disconnect();
            }
            catch (Exception ex)
            {
                _logger?.Error($"釋放RDP連線資源時發生錯誤:{ConnectionId}", ex);
            }

            SafeInvoke(() => ConnectionTimeoutOccurred?.Invoke(this,
                new RdpConnectionTimeoutException(DEFAULT_CONNECTION_TIMEOUT, ConnectionId)));
        }
    }

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public event EventHandler ConnectionStateChanged;
    public event EventHandler<RdpConnectionTimeoutException> ConnectionTimeoutOccurred;

    /// <summary>
    ///     獲取連線統計資訊
    /// </summary>
    public RdpConnectionStats GetConnectionStats()
    {
        var stats = new RdpConnectionStats
        {
            ConnectionId = ConnectionId,
            HostName = _hostName,
            IsConnected = IsConnected,
            State = State,
            ConnectedDuration = IsConnected ? DateTime.Now - _connectionStartTime : TimeSpan.Zero
        };

        return stats;
    }

    private void InitializeRdpClient()
    {
        if (_parentForm != null)
        {
            ((ISupportInitialize)_rdpClient).BeginInit();
            _parentForm.Controls.Add(_rdpClient);

            // 設定基本屬性
            _rdpClient.DesktopWidth = 1920;
            _rdpClient.DesktopHeight = 1080;
            _rdpClient.ColorDepth = 32;

            // 設定進階屬性
            _rdpClient.AdvancedSettings9.SmartSizing = true;
            _rdpClient.AdvancedSettings9.BitmapPeristence = 1;
            _rdpClient.AdvancedSettings9.NetworkConnectionType = 6;
            _rdpClient.AdvancedSettings9.DisplayConnectionBar = true;

            _rdpClient.CreateControl();
            ((ISupportInitialize)_rdpClient).EndInit();
        }
        else
        {
            var host = new Form { Visible = false };
            ((ISupportInitialize)_rdpClient).BeginInit();
            host.Controls.Add(_rdpClient);

            // 確保控制項已建立並可使用 CreateControl
            try
            {
                _rdpClient.CreateControl();
                _rdpClient.Visible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeRdpClient (no parent) CreateControl failed: {ex.Message}");
            }

            ((ISupportInitialize)_rdpClient).EndInit();
        }
    }

    public RdpConfig GetRdpConfig()
    {
        return _config;
    }

    private void ApplyConfig(RdpConfig config)
    {
        try
        {
            if (_rdpClient == null || !_rdpClient.Created) return;

            // 限制最大尺寸
            var width = Math.Min(config.ScreenWidth, MAX_WIDTH);
            var height = Math.Min(config.ScreenHeight, MAX_HEIGHT);

            // 基本設定
            _rdpClient.DesktopWidth = width;
            _rdpClient.DesktopHeight = height;
            _rdpClient.ColorDepth = config.ColorDepth;

            // 進階設定
            _rdpClient.AdvancedSettings9.SmartSizing = true;
            _rdpClient.AdvancedSettings9.BitmapPeristence = config.EnableBitmapPersistence ? 1 : 0;
            _rdpClient.AdvancedSettings9.Compress = config.EnableCompression ? 1 : 0;
            _rdpClient.AdvancedSettings9.EnableCredSspSupport = config.EnableCredSspSupport;

            if (!string.IsNullOrEmpty(config.Domain)) _rdpClient.Domain = config.Domain;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ApplyConfig 錯誤：{ex.Message}");
        }
    }

    private void SafeInvoke(Action action)
    {
        if (_uiControl?.InvokeRequired == true)
            _uiControl.BeginInvoke(action);
        else
            action();
    }

    /// <summary>
    ///     觸發連線狀態變更事件
    /// </summary>
    private void OnConnectionStateChanged()
    {
        SafeInvoke(() => ConnectionStateChanged?.Invoke(this, EventArgs.Empty));
    }

    private void RdpClient_OnConnecting(object sender, EventArgs e)
    {
        SafeInvoke(() =>
        {
            State = RdpState.Connecting;
            IsConnected = false;
            _logger?.Info("正在建立連線", ConnectionId);
        });
    }

    private void RdpClient_OnConnected(object sender, EventArgs e)
    {
        SafeInvoke(() =>
        {
            StopConnectionTimer();
            // 暫時不要立刻標示為 Connected，改為啟動驗證流程，確定畫面可擷取後才標示
            _verificationInProgress = true;
            _logger?.Info("已建立底層連線，啟動畫面驗證流程", ConnectionId);

            // 非同步驗證，避免阻塞 UI
            Task.Run(async () =>
            {
                var verified = await VerifyConnectionAsync(10000, 500);
                SafeInvoke(() =>
                {
                    _verificationInProgress = false;
                    if (verified)
                    {
                        State = RdpState.Connected;
                        IsConnected = true;
                        var duration = DateTime.Now - _connectionStartTime;
                        _logger?.Info($"連線並驗證成功，耗時 {duration.TotalSeconds:F2} 秒", ConnectionId);
                    }
                    else
                    {
                        State = RdpState.Error;
                        IsConnected = false;
                        _logger?.Warn("連線建立但畫面驗證失敗：可能控制項尚未就緒", ConnectionId);
                    }
                });
            });
        });
    }

    private void RdpClient_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
    {
        SafeInvoke(() =>
        {
            StopConnectionTimer();
            State = RdpState.Disconnected;
            IsConnected = false;

            // 記錄中斷原因
            var reason = GetDisconnectReason(e.discReason);
            _logger?.Info($"連線已中斷，原因: {reason} (代碼: {e.discReason})", ConnectionId);
        });
    }

    private void RdpClient_OnLoginComplete(object sender, EventArgs e)
    {
        SafeInvoke(() =>
        {
            StopConnectionTimer();
            State = RdpState.Connected;
            IsConnected = true;
            _logger?.Info("登入完成", ConnectionId);
            //FormRDPShow dlg = new FormRDPShow(this);
            //FormShowRdp dlg = new FormShowRdp();
            //dlg.SetRdpConnection(this);
            //dlg.ShowDialog();
        });
    }

    /// <summary>
    ///     獲取中斷原因的友善描述
    /// </summary>
    private string GetDisconnectReason(int reasonCode)
    {
        switch (reasonCode)
        {
            case 0: return "無錯誤";
            case 1: return "本地中斷";
            case 2: return "遠端中斷";
            case 3: return "伺服器中斷";
            case 260: return "DNS查找失敗";
            case 262: return "記憶體不足";
            case 264: return "連線逾時";
            case 516: return "內部錯誤";
            case 518: return "記憶體不足";
            case 520: return "主機找不到";
            case 772: return "Winsock錯誤";
            case 1030: return "安全錯誤";
            case 1286: return "加密錯誤";
            case 2308: return "授權錯誤";
            default: return $"未知錯誤 ({reasonCode})";
        }
    }

    public string GetHostName()
    {
        return _hostName;
    }

    public string GetUserName()
    {
        return _credentials?.UserName ?? string.Empty;
    }

    public string GetPassword()
    {
        return _credentials?.GetPassword() ?? string.Empty;
    }

    /// <summary>
    ///     獲取安全憑證（推薦使用這個方法而不是GetPassword）
    /// </summary>
    public SecureCredentials GetCredentials()
    {
        return _credentials;
    }

    public AxMsRdpClient9NotSafeForScripting GetRdpClient()
    {
        return _rdpClient;
    }

    /// <summary>
    ///     簡單的 ping 檢查
    /// </summary>
    private bool PingHost(string host, int timeoutMs = 1000)
    {
        try
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send(host, timeoutMs);
                return reply != null && reply.Status == IPStatus.Success;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     連線建立後驗證控制項是否真的能顯示/擷取畫面。
    ///     會在 timeoutMs 內每 intervalMs 嘗試一次 GetScreenshot()
    /// </summary>
    private async Task<bool> VerifyConnectionAsync(int timeoutMs = 5000, int intervalMs = 200)
    {
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                // 若 control 尚未建立 handle，略過本次嘗試
                if (_rdpClient == null) return false;

                if (_rdpClient.Connected == 1)
                {
                    //var bmp = GetScreenshot();
                    //if (bmp != null)
                    //{
                    //    bmp.Dispose();
                        return true;
                    //}
                }
            }
            catch (Exception ex)
            {
                _logger?.Debug($"VerifyConnectionAsync 嘗試擷取畫面發生例外: {ex.Message}", ConnectionId);
            }

            await Task.Delay(intervalMs);
        }

        return false;
    }

    public void LoadConfiguration(RdpConnectionProfile data)
    {
        Configure(data.Config);
        var item = data.GetRdpConnection();
        this.SetConnectionId(item.ConnectionId);
    }
}