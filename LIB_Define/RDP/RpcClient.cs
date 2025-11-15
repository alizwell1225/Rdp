using LIB_Log;
using LIB_RPC;
using LIB_RPC.Abstractions;
using LIB_RPC.API;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace LIB_Define.RDP;

public class RpcClient
{
    private const int ReconnectDelayMs = 2000;
    private readonly object _connectionLock = new();
    private IClientApi? _api;

    private bool _isReconnecting = false;
    private bool _isStressTesting;
    private CancellationTokenSource? _sendCts;

    // Stress test fields
    private CancellationTokenSource? _stressTestCts;
    private int _stressTestFailures;
    private int _stressTestIterations;
    private readonly Stopwatch _stressTestStopwatch = new();
    private int _stressTestSuccesses;
    
    public Action<int,Image> ActionImage;
    public Action<int, string> ActionOnLog;
    public Action<int, string> ActionOnLogStressStats;
    public Action<int, string> ActionOnConnectionError;

    public Action<int, string> ActionOnServerFileCompleted;
    public Action<int, JsonMessage> ActionOnServerJson;
    public Action<int, string, double> ActionOnUploadProgress;

    public Action<int, string, double> ActionOnDownloadProgress;
    public Action<int, Image> ActionScreenshot;
    public Action<int, double> ActionScreenshotProgress;
    
    // New actions for enhanced functionality
    public Action<int, ShowPictureType, Image> ActionOnServerImage;
    public Action<int, ShowPictureType, string> ActionOnServerImagePath;
    public Action<int, bool> ActionConnectedState;

    private string configPath ;
    public GrpcConfig? _config;

    private bool shouldReconnect;
    public int Index { get; private set; }
    public RpcClient(string path,int index=0)
    {
        Index=index;
        LoadConfig(path);
        _sendCts = new CancellationTokenSource();
    }

    public bool IsConnected { get; private set; }
    private bool autoReStart { get; } = true;
    public bool IsEnableConnect { get; private set; }

    public async void LoadConfig(string path)
    {
        bool runflag = IsConnected && IsEnableConnect;
        if (IsConnected)
        {
            await DisconnectAsync();
        }
        configPath = path;
        if (File.Exists(configPath) == false)
        {
            new Exception($"config Error ,Path={configPath}");
        }
        _config = GrpcConfig.Load(configPath);
        _config.Save(configPath);
        if (runflag)
        {
            await StartConnect();
        }
    }
    public async void SaveConfig(string _hostAddr, string _hostPort)
    {
        await DisconnectAsync();
        if (!int.TryParse(_hostPort, out var port))
        {
            MessageBox.Show("Port 必須為數字");
            return;
        }

        if (_config == null)
            _config = GrpcConfig.Load(configPath);
        _config.SaveHost(_hostAddr ?? "localhost");
        _config.SavePort(port);
        _config.Save(configPath);
        await ConnectAsync(autoReStart);
    }

    private async Task ConnectAsync(bool retry = false)
    {
        lock (_connectionLock)
        {
            if (_api == null || _config == null)
            {
                if (_config == null)
                    _config ??= new GrpcConfig();
                _api ??= new GrpcClientApi(_config);
                // forward api logs to UI
                _api.OnLog += apiOnLog;
                _api.OnUploadProgress += apiOnUploadProgress;
                _api.OnDownloadProgress += apiOnDownloadProgress;
                _api.OnScreenshotProgress += apiOnScreenshotProgress;
                _api.OnServerJson += apiOnServerJson;
                _api.OnServerFileCompleted += apiOnServerFileCompleted;
                _api.OnConnected += api_OnConnected;
                _api.OnDisconnected += apiOnDisconnected;
                _api.OnConnectionError += apiOnConnectionError;
                _api.OnServerByteData += apiOnServerByteData;
            }
            //else
            //{
            //    // Update existing API with new config (important when server port changes)
            //    _api.UpdateConfig(_config);
            //}
        }
        await Task.Delay(10);
        await _api.ConnectAsync(retry);
    }

    public Action<int, string, byte[], string> ActionOnServerByteData;
    private void apiOnServerByteData(string type, byte[] data, string? metadata)
    {
        ActionOnServerByteData?.Invoke(Index, type, data, metadata);
    }

    private async void AutoReStartWork()
    {
        bool shouldAttemptReconnect;
        lock (_connectionLock)
        {
            // Prevent multiple reconnection attempts if one is already in progress
            if (_isReconnecting)
                return;
                
            if (autoReStart && !IsConnected)
            {
                shouldReconnect = true;
                _isReconnecting = true;
            }
            else
                return;
        }

        try
        {
            await Task.Delay(ReconnectDelayMs);

            lock (_connectionLock)
            {
                shouldAttemptReconnect = !IsConnected && autoReStart && shouldReconnect;
            }
            
            if (shouldAttemptReconnect)
            {
                // Await these calls to ensure proper cleanup before reconnecting
                await DisconnectAsync();
                await ConnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            AppendTextSafe($"Auto-reconnect failed: {ex.Message}\r\n");
        }
        finally
        {
            lock (_connectionLock)
            {
                _isReconnecting = false;
            }
        }
    }

    public void OpenLogViewerForm()
    {
        var dlg = new LogViewerForm();
        dlg.SetDefinePath(_api.Config.LogFilePath);
        dlg.ShowDialog();
    }

    private void AppendTextSafe(string text)
    {
        if (ActionOnLog == null)
            return;
        ActionOnLog?.Invoke(Index,text);
    }

    private void UpdateUiConnectedState(bool connected)
    {
        lock (_connectionLock)
        {
            IsConnected = connected;
            if (IsConnected==false)
            {
                ActionConnectedState?.Invoke(Index, IsConnected);
            }
        }
    }

    private async Task DisconnectAsync()
    {
        try
        {
            if (_api != null)
            {
                await _api.DisconnectAsync();
                await _api.DisposeAsync();
                _api = null;
            }
        }
        catch (Exception ex)
        {
            AppendTextSafe($"Disconnect error: {ex.Message}{Environment.NewLine}");
        }
        finally
        {
            UpdateUiConnectedState(false);
        }
    }

    private async Task SendJsonAsync()
    {
        if (_api == null) return;
        var ack = await _api.SendJsonAsync("test", "{\"msg\":\"hello\"}", _sendCts.Token);
        AppendTextSafe($"Ack: {ack.Success} {ack.Error}\r\n");
    }

    private async Task UploadAsync()
    {
        if (_api == null) return;
        using var ofd = new OpenFileDialog();
        if (ofd.ShowDialog() != DialogResult.OK) return;
        var status = await _api.UploadFileAsync(ofd.FileName, _sendCts.Token);
        AppendTextSafe($"Upload: {status.Success} {status.Error}\r\n");
    }

    private async Task DownloadAsync()
    {
        if (_api == null) return;
        // Ask server for available files first
        string[] files;
        try
        {
            files = await _api.ListFilesAsync();
        }
        catch
        {
            files = Array.Empty<string>();
        }

        string? input = null;
        if (files.Length == 0)
        {
            // fallback to input box if server returned nothing
            input = Interaction.InputBox("請輸入伺服端檔名：", "下載檔案");
            if (string.IsNullOrWhiteSpace(input)) return;
        }
        else
        {
            // show a small modal list selection dialog
            using var dlg = new Form();
            dlg.Text = "Select remote file";
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.Width = 400;
            dlg.Height = 300;
            var lb = new ListBox { Dock = DockStyle.Fill };
            lb.Items.AddRange(files);
            var btnOk = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 30 };
            btnOk.Click += (sender, args) => OnFileSelectionOkClick(sender, args, lb, dlg);
            dlg.Controls.Add(lb);
            dlg.Controls.Add(btnOk);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            input = lb.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(input)) return;
        }

        using var sfd = new SaveFileDialog();
        sfd.FileName = input;
        if (sfd.ShowDialog() != DialogResult.OK) return;
        await _api.DownloadFileAsync(input, sfd.FileName, _sendCts.Token);
        AppendTextSafe($"Download 完成: {sfd.FileName}\r\n");
    }

    private async Task ScreenshotAsync()
    {
        if (_api == null) return;
        var bytes = await _api.GetScreenshotAsync(_sendCts.Token);
        if (bytes == null || bytes.Length == 0)
        {
            AppendTextSafe("Screenshot 失敗\r\n");
            return;
        }
        using var ms = new MemoryStream(bytes);
        var img = Image.FromStream(ms);
        ActionScreenshot?.Invoke(Index,img);
        AppendTextSafe("Screenshot 顯示\r\n");
    }

    public async Task StartConnect()
    {
        await Connect();
    }

    public async Task Connect(bool force=false)
    {
        IsEnableConnect = false;
        try
        {
            bool isConnected;
            lock (_connectionLock)
            {
                isConnected = IsConnected;
            }

            if (!isConnected)
                await ConnectAsync(autoReStart);
            else
            {
                if (force)
                {
                    await DisconnectAsync();
                    await ConnectAsync(autoReStart);
                }
                else
                    await DisconnectAsync();
            }
        }
        finally
        {
            IsEnableConnect = true;
        }
    }

    public async void SendDataJsonAsync()
    {
        await SendJsonAsync();
    }

    public async void SendDataUpload()
    {
        await UploadAsync();
    }

    public async void SendDataDownload()
    {
        await DownloadAsync();
    }

    public async void SendDataScreenshot()
    {
        await ScreenshotAsync();
    }

    private void OnFileSelectionOkClick(object? sender, EventArgs e, ListBox listBox, Form dialog)
    {
        if (listBox.SelectedItem != null)
            dialog.DialogResult = DialogResult.OK;
        else
            MessageBox.Show("請選擇一個檔案");
    }

    #region Event

    private void apiOnDisconnected()
    {
        lock (_connectionLock)
        {
            UpdateUiConnectedState(false);
            AutoReStartWork();
        }
    }

    private void apiOnConnectionError(string obj)
    {
        lock (_connectionLock)
        {
            UpdateUiConnectedState(false);
            AutoReStartWork();
            ActionOnConnectionError?.Invoke(Index, obj);
        }
    }

    private void api_OnConnected()
    {
        lock (_connectionLock)
        {
            shouldReconnect = false;
            UpdateUiConnectedState(true);
            ActionConnectedState?.Invoke(Index, IsConnected);
        }
    }

    private void apiOnServerFileCompleted(string path)
    {
        AppendTextSafe($"[Recive] FIle Path={path}\r\n");
        ActionOnServerFileCompleted?.Invoke(Index,path);
    }

    private void apiOnServerJson(JsonMessage env)
    {
        AppendTextSafe($"[Recive] type={env.Type} id={env.Id} bytes={env.Json?.Length}\r\n");
        
        // Handle image messages specially
        if (env.Type == "image")
        {
            HandleServerImageMessage(env);
        }
        
        // Always invoke the generic handler for application-level processing
        ActionOnServerJson?.Invoke(Index,env);
    }

    private void apiOnScreenshotProgress(double percent)
    {
        //BeginInvoke(new Action(() =>
        //{
        //    _pbScreenshot.Value = (int)Math.Min(100, Math.Round(percent));
        //    _lblScreenshot.Text = $"Screenshot: {percent:F2}%";
        //    if (percent >= 100) _pbScreenshot.Value = 0; // 完成後歸零方便下次
        //}));
        ActionScreenshotProgress?.Invoke(Index, percent);
    }

    private void apiOnDownloadProgress(string path, double percent)
    {
        //BeginInvoke(new Action(() =>
        //{
        //    _pbDownload.Value = (int)Math.Min(100, Math.Round(percent));
        //    _lblDownload.Text = $"Download: {percent:F2}% ({path})";
        //}));
        ActionOnDownloadProgress?.Invoke(Index,path,percent);
    }

    private void apiOnUploadProgress(string path, double percent)
    {
        //BeginInvoke(new Action(() =>
        //{
        //    _pbUpload.Value = (int)Math.Min(100, Math.Round(percent));
        //    _lblUpload.Text = $"Upload: {percent:F2}% ({path})";
        //}));
        ActionOnUploadProgress?.Invoke(Index, path, percent);
    }

    private void apiOnLog(string line)
    {
        AppendTextSafe(line + Environment.NewLine);
    }

    #endregion

    #region Generic JSON Methods

    /// <summary>
    /// Send a generic object as JSON to the server
    /// </summary>
    public async Task<JsonAcknowledgment> SendObjectAsJsonAsync<T>(string type, T obj, CancellationToken ct = default)
    {
        if (_api == null)
        {
            return new JsonAcknowledgment { Success = false, Error = "Not connected" };
        }

        try
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions 
            { 
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            });
            
            var ack = await _api.SendJsonAsync(type, json, ct);
            AppendTextSafe($"Sent {typeof(T).Name}: {ack.Success} {ack.Error}\r\n");
            return ack;
        }
        catch (Exception ex)
        {
            AppendTextSafe($"Failed to serialize/send {typeof(T).Name}: {ex.Message}\r\n");
            return new JsonAcknowledgment { Success = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// Send FlowChartOBJ as JSON to the server
    /// </summary>
    public async Task<JsonAcknowledgment> SendFlowChartAsync(FlowChartOBJ flowChart, CancellationToken ct = default)
    {
        // Sync colors before sending
        flowChart.SyncColorsToArgb();
        return await SendObjectAsJsonAsync("flowchart", flowChart, ct);
    }

    /// <summary>
    /// Deserialize JSON message to a specific type
    /// </summary>
    public T? DeserializeJsonMessage<T>(JsonMessage message) where T : class
    {
        try
        {
            if (string.IsNullOrEmpty(message.Json))
            {
                AppendTextSafe($"Empty JSON in message type={message.Type}\r\n");
                return null;
            }

            var obj = JsonSerializer.Deserialize<T>(message.Json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (obj == null)
            {
                AppendTextSafe($"Failed to deserialize {typeof(T).Name} from type={message.Type}\r\n");
                return null;
            }

            // If it's a FlowChartOBJ, sync colors from ARGB
            if (obj is FlowChartOBJ flowChart)
            {
                flowChart.SyncColorsFromArgb();
            }

            return obj;
        }
        catch (Exception ex)
        {
            AppendTextSafe($"Exception deserializing {typeof(T).Name}: {ex.Message}\r\n");
            return null;
        }
    }

    /// <summary>
    /// Handle server image message - expects JSON with ShowPictureType and either ImagePath or ImageData (base64)
    /// </summary>
    private void HandleServerImageMessage(JsonMessage message)
    {
        try
        {
            var imageMsg = JsonSerializer.Deserialize<ImageTransferMessage>(message.Json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (imageMsg == null)
            {
                AppendTextSafe("Failed to deserialize image message\r\n");
                return;
            }

            if (!string.IsNullOrEmpty(imageMsg.ImagePath))
            {
                // Path-based image
                ActionOnServerImagePath?.Invoke(Index, imageMsg.PictureType, imageMsg.ImagePath);
                AppendTextSafe($"Received image path: {imageMsg.ImagePath} (Type: {imageMsg.PictureType})\r\n");
            }
            else if (!string.IsNullOrEmpty(imageMsg.ImageDataBase64))
            {
                // Base64 image data - wrap in try-catch to prevent crashes on corrupt/large data
                try
                {
                    var imageBytes = Convert.FromBase64String(imageMsg.ImageDataBase64);
                    
                    // Validate size before attempting to load
                    if (imageBytes.Length > 50 * 1024 * 1024) // 50MB limit
                    {
                        AppendTextSafe($"Image too large: {imageBytes.Length / (1024 * 1024)}MB (max 50MB)\r\n");
                        return;
                    }
                    
                    using var ms = new MemoryStream(imageBytes);
                    var image = Image.FromStream(ms);
                    ActionOnServerImage?.Invoke(Index, imageMsg.PictureType, image);
                    AppendTextSafe($"Received image: {image.Width}x{image.Height}, {imageBytes.Length / 1024}KB (Type: {imageMsg.PictureType})\r\n");
                }
                catch (OutOfMemoryException)
                {
                    AppendTextSafe($"Out of memory loading image - image too large\r\n");
                }
                catch (ArgumentException ex)
                {
                    AppendTextSafe($"Invalid image data: {ex.Message}\r\n");
                }
            }
        }
        catch (Exception ex)
        {
            AppendTextSafe($"Error handling server image: {ex.Message}\r\n");
        }
    }

    #endregion

    #region Stress Test Methods

    private async Task RunStressTestAsync(int intervalMs, int sizeKB, int maxIterations, CancellationToken ct,
        int testType = 0)
    {
        var iteration = 0;

        while (!ct.IsCancellationRequested)
        {
            if (maxIterations > 0 && iteration >= maxIterations)
                break;

            iteration++;
            _stressTestIterations = iteration;

            try
            {
                switch (testType)
                {
                    case 0: // JSON 傳送
                        await StressTestJsonAsync(sizeKB, ct);
                        break;
                    case 1: // 檔案上傳
                        await StressTestUploadAsync(sizeKB, ct);
                        break;
                    case 2: // 檔案下載
                        await StressTestDownloadAsync(ct);
                        break;
                    case 3: // 混合測試
                        await StressTestMixedAsync(sizeKB, ct);
                        break;
                }

                _stressTestSuccesses++;
                UpdateStressTestStats();
            }
            catch (Exception ex)
            {
                _stressTestFailures++;
                AppendTextSafe($"[第 {iteration} 次失敗] {ex.Message}\r\n");
                UpdateStressTestStats();
                await DisconnectAsync();
                await ConnectAsync();
            }

            // Wait before next iteration
            await Task.Delay(intervalMs, ct);
        }
    }

    private async Task StressTestJsonAsync(int sizeKB, CancellationToken ct)
    {
        if (_api == null) return;

        // Generate test JSON data
        var data = new string('X', sizeKB * 1024);
        var json = $"{{\"data\":\"{data}\",\"timestamp\":\"{DateTime.Now:O}\"}}";

        var ack = await _api.SendJsonAsync("stress_test", json);
        if (!ack.Success)
            throw new Exception($"JSON 傳送失敗: {ack.Error}");
    }

    private async Task StressTestUploadAsync(int sizeKB, CancellationToken ct)
    {
        if (_api == null) return;

        // Create temp file
        var tempPath = Path.Combine(Path.GetTempPath(), $"stress_test_{Guid.NewGuid()}.dat");
        try
        {
            // Generate test data
            var data = new byte[sizeKB * 1024];
            new Random().NextBytes(data);
            await File.WriteAllBytesAsync(tempPath, data, ct);

            // Upload
            var result = await _api.UploadFileAsync(tempPath);
            if (!result.Success)
                throw new Exception($"檔案上傳失敗: {result.Error}");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private async Task StressTestDownloadAsync(CancellationToken ct)
    {
        if (_api == null) return;

        // List files and download the first one
        var files = await _api.ListFilesAsync();
        if (files.Length == 0)
            throw new Exception("伺服器沒有可下載的檔案");

        var tempPath = Path.Combine(Path.GetTempPath(), $"download_{Guid.NewGuid()}.dat");
        try
        {
            await _api.DownloadFileAsync(files[0], tempPath);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private async Task StressTestMixedAsync(int sizeKB, CancellationToken ct)
    {
        // Randomly choose one of the operations
        var rnd = new Random();
        var operation = rnd.Next(3);

        switch (operation)
        {
            case 0:
                await StressTestJsonAsync(sizeKB, ct);
                break;
            case 1:
                await StressTestUploadAsync(sizeKB, ct);
                break;
            case 2:
                await StressTestDownloadAsync(ct);
                break;
        }
    }

    private void UpdateStressTestStats()
    {
        var elapsed = _stressTestStopwatch.Elapsed;
        var successRate = _stressTestIterations > 0
            ? _stressTestSuccesses * 100.0 / _stressTestIterations
            : 0;

        var avgTime = _stressTestIterations > 0
            ? elapsed.TotalMilliseconds / _stressTestIterations
            : 0;

        ActionOnLogStressStats?.Invoke(Index,
            $"執行: {_stressTestIterations} | 成功: {_stressTestSuccesses} | 失敗: {_stressTestFailures} | " +
            $"成功率: {successRate:F2}% | 平均: {avgTime:F0}ms | 總時間: {elapsed:hh\\:mm\\:ss}");
    }

    public async void StartStressTest()
    {
        if (_isStressTesting)
        {
            // Stop test
            _stressTestCts?.Cancel();
            return;
        }

        bool isConnected;
        lock (_connectionLock)
        {
            isConnected = IsConnected;
        }

        if (!isConnected || _api == null)
        {
            MessageBox.Show("請先連線到 Server!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Parse parameters
        if (!int.TryParse("10", out var intervalMs) || intervalMs < 10)
        {
            MessageBox.Show("間隔時間必須 >= 10ms", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse("50", out var sizeKB) || sizeKB < 1)
        {
            MessageBox.Show("資料大小必須 >= 1KB", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var maxIterations = 0; // 0 means unlimited


        // Start stress test
        _isStressTesting = true;
        _stressTestIterations = 0;
        _stressTestSuccesses = 0;
        _stressTestFailures = 0;
        _stressTestStopwatch.Restart();
        _stressTestCts = new CancellationTokenSource();

        AppendTextSafe("=== 壓力測試開始 ===\r\n");
        AppendTextSafe(
            $"間隔: {intervalMs}ms, 大小: {sizeKB}KB, 次數: {(maxIterations == 0 ? "無限制" : maxIterations.ToString())}\r\n");

        try
        {
            await RunStressTestAsync(intervalMs, sizeKB, maxIterations, _stressTestCts.Token);
        }
        catch (OperationCanceledException)
        {
            AppendTextSafe("壓力測試已取消\r\n");
        }
        catch (Exception ex)
        {
            AppendTextSafe($"壓力測試錯誤: {ex.Message}\r\n");
        }
        finally
        {
            StopStressTest();
        }
    }

    public void ResetStressTestStats()
    {
        _stressTestIterations = 0;
        _stressTestSuccesses = 0;
        _stressTestStopwatch.Reset();
    }

    public void StopStressTest()
    {
        _isStressTesting = false;
        _stressTestStopwatch.Stop();
        _stressTestCts?.Dispose();
        _stressTestCts = null;

        AppendTextSafe("=== 壓力測試結束 ===\r\n");
        AppendTextSafe($"總執行: {_stressTestIterations} 次\r\n");
        AppendTextSafe($"成功: {_stressTestSuccesses} 次\r\n");
        AppendTextSafe($"失敗: {_stressTestFailures} 次\r\n");
        AppendTextSafe($"總時間: {_stressTestStopwatch.Elapsed:hh\\:mm\\:ss}\r\n\r\n");
    }

    #endregion
}