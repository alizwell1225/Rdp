using Google.Protobuf.WellKnownTypes;
using LIB_Log;
using LIB_RPC;
using LIB_RPC.Abstractions;
using LIB_RPC.API;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrpcClientApp
{
    public partial class ClientForm : Form
    {
        private IClientApi? _api;
        private GrpcConfig? _config;
        private bool _isConnected = false;
        private bool _isReconnecting = false;
        private readonly object _connectionLock = new object();
        private const int ReconnectDelayMs = 2000;

        // Stress test fields
        private CancellationTokenSource? _stressTestCts;
        private bool _isStressTesting = false;
        private int _stressTestIterations = 0;
        private int _stressTestSuccesses = 0;
        private int _stressTestFailures = 0;
        private Stopwatch _stressTestStopwatch = new Stopwatch();

        public ClientForm()
        {
            InitializeComponent();
            _config = GrpcConfig.Load(configPath);
            _config.Save(configPath);
        }

        private bool autoReStart { get; set; } = true;
        private void ClientForm_Load(object? sender, EventArgs e)
        {
            autoReStart = chkAutoRestart.Checked;
            txtHost.Text = _config?.Host ?? "localhost";    
            txtPort.Text = (_config?.Port ?? 50051).ToString();
        }

        private string configPath = Path.Combine(AppContext.BaseDirectory, "Config", "Config.json");
        private async void _btnApply_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtPort?.Text, out var port)) { MessageBox.Show("Port 必須為數字"); return; }

            if (_config == null)
            {
                _config = GrpcConfig.Load(configPath);
            }
            //else
            {
                _config.SaveHost(txtHost?.Text ?? "localhost");
                _config.SavePort(port);
            }
            _config.Save(configPath);
            //// dispose previous api if any
            //if (_api != null) await _api.DisposeAsync();
            //_api = new GrpcClientApi(_config);
            // enable/disable controls handled by ConnectAsync
            autoReStart = chkAutoRestart.Checked;
            await DisconnectAsyncUI();
            await ConnectAsync(autoReStart);

        }

        private async Task ConnectAsync(bool re=false)
        {
            if (_api == null || _config== null)
            {
                _config ??= new GrpcConfig();
                _api ??= new GrpcClientApi(_config);
                // forward api logs to UI
                _api.OnLog += apiOnLog;
                _api.OnUploadProgress += apiOnUploadProgress;
                _api.OnDownloadProgress += apiOnDownloadProgress;
                _api.OnScreenshotProgress += apiOnScreenshotProgress;
                _api.OnServerJson += apiOnServerJson;
                _api.OnServerFileCompleted += apiOnServerFileCompleted;
                _api.OnConnected += _api_OnConnected;
                _api.OnDisconnected += apiOnDisconnected;
                _api.OnConnectionError += apiOnConnectionError;
            }
            await _api.ConnectAsync(re);

        }

        private void apiOnDisconnected()
        {
            lock (_connectionLock)
            {
                _isConnected = false;
                UpdateUiConnectedState(false);
                AutoReStartWork();
            }
            
        }

        private void apiOnConnectionError(string obj)
        {
            lock (_connectionLock)
            {
                _isConnected = false;
                UpdateUiConnectedState(false);
                AutoReStartWork();
            }
        }
        bool shouldReconnect = false;

        private async void AutoReStartWork()
        {
            lock (_connectionLock)
            {
                if (autoReStart && !_isConnected)
                {
                    shouldReconnect = true;
                }
                else
                {
                    return;
                }
            }

            //Task.Run( () =>
            {
                try
                {
                    await Task.Delay(ReconnectDelayMs);

                    lock (_connectionLock)
                    {
                        shouldReconnect = !_isConnected && autoReStart;
                        if (shouldReconnect)
                        {
                            DisconnectAsyncUI();
                            ConnectAsync(shouldReconnect);
                        }
                    }
                }
                catch (Exception ex)
                {
                    BeginInvoke(new Action(() =>
                    {
                        _log.AppendText($"Auto-reconnect failed: {ex.Message}\r\n");
                    }));
                }
                finally
                {
                }
            };
            //);
        }

        private void _api_OnConnected()
        {
            lock (_connectionLock)
            {
                _isConnected = true;
                shouldReconnect = false;
                UpdateUiConnectedState(true);

            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            LogViewerForm dlg = new LogViewerForm();
            dlg.SetDefinePath(_api.Config.LogFilePath);
            dlg.ShowDialog();
        }

        private void apiOnServerFileCompleted(string path)
        {
            BeginInvoke(new Action(() =>
            {
                _log.AppendText($"[Recive] FIle Path={path}\r\n");
            }));
        }

        private void apiOnServerJson(JsonMessage env)
        {
            BeginInvoke(new Action(() =>
            {
                _log.AppendText($"[Recive] type={env.Type} id={env.Id} bytes={env.Json?.Length}\r\n");
            }));
        }

        private void apiOnScreenshotProgress(double percent)
        {
            BeginInvoke(new Action(() =>
            {
                _pbScreenshot.Value = (int)Math.Min(100, Math.Round(percent));
                _lblScreenshot.Text = $"Screenshot: {percent:F2}%";
                if (percent >= 100) _pbScreenshot.Value = 0; // 完成後歸零方便下次
            }));
        }

        private void apiOnDownloadProgress(string path, double percent)
        {
            BeginInvoke(new Action(() =>
            {
                _pbDownload.Value = (int)Math.Min(100, Math.Round(percent));
                _lblDownload.Text = $"Download: {percent:F2}% ({path})";
            }));
        }


        private void apiOnUploadProgress(string path, double percent)
        {
            BeginInvoke(new Action(() =>
            {
                _pbUpload.Value = (int)Math.Min(100, Math.Round(percent));
                _lblUpload.Text = $"Upload: {percent:F2}% ({path})";
            }));
        }


        private void apiOnLog(string line)
        {
            BeginInvoke(new Action(() => _log.AppendText(line + Environment.NewLine)));
        }



        private void UpdateUiConnectedState(bool connected)
        {
            BeginInvoke(new Action(() =>
            {
                // toggle buttons and text
                _btnSendJson.Enabled = connected;
                _btnUpload.Enabled = connected;
                _btnDownload.Enabled = connected;
                _btnScreenshot.Enabled = connected;
                _btnConnect.Text = connected ? "Disconnect" : "Connect to Server";
            }
            ));
        }

        private async Task DisconnectAsyncUI()
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
                // log and show minimal feedback
                _log.AppendText($"Disconnect error: {ex.Message}{Environment.NewLine}");
            }
            finally
            {
                lock (_connectionLock)
                {
                    _isConnected = false;
                }
                UpdateUiConnectedState(false);
            }
        }

        private async Task SendJsonAsync()
        {
            if (_api == null) return;
            var ack = await _api.SendJsonAsync("test", "{\"msg\":\"hello\"}");
            _log.AppendText($"Ack: {ack.Success} {ack.Error}\r\n");
        }

        private async Task UploadAsync()
        {
            if (_api == null) return;
            using var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var status = await _api.UploadFileAsync(ofd.FileName);
            _log.AppendText($"Upload: {status.Success} {status.Error}\r\n");
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
                input = Microsoft.VisualBasic.Interaction.InputBox("請輸入伺服端檔名：", "下載檔案");
                if (string.IsNullOrWhiteSpace(input)) return;
            }
            else
            {
                // show a small modal list selection dialog
                using var dlg = new Form();
                dlg.Text = "Select remote file";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.Width = 400; dlg.Height = 300;
                var lb = new ListBox { Dock = DockStyle.Fill }; lb.Items.AddRange(files);
                var btnOk = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 30 };
                btnOk.Click += (sender, args) => OnFileSelectionOkClick(sender, args, lb, dlg);
                dlg.Controls.Add(lb); dlg.Controls.Add(btnOk);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                input = lb.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(input)) return;
            }

            using var sfd = new SaveFileDialog();
            sfd.FileName = input;
            if (sfd.ShowDialog() != DialogResult.OK) return;
            await _api.DownloadFileAsync(input, sfd.FileName);
            _log.AppendText($"Download 完成: {sfd.FileName}\r\n");
        }

        private async Task ScreenshotAsync()
        {
            if (_api == null) return;
            var bytes = await _api.GetScreenshotAsync();
            if (bytes == null || bytes.Length == 0)
            {
                _log.AppendText("Screenshot 失敗\r\n");
                return;
            }
            else
            {
                using var ms = new MemoryStream(bytes);
                _pic.Image = Image.FromStream(ms);
                _log.AppendText($"Screenshot 顯示\r\n");
            }
        }

        private async void _btnConnect_Click(object sender, EventArgs e)
        {
            // Toggle connect / disconnect
            //_btnConnect.Enabled = false;
            //try
            //{
            //    if (!_isConnected)
            //    {
            //        await ConnectAsync();
            //    }
            //    else
            //    {
            //        await DisconnectAsyncUI();
            //    }
            //}
            //finally
            //{
            //    _btnConnect.Enabled = true;
            //}
            Connect();
        }

        async void Connect()
        {
            BeginInvoke(() => _btnConnect.Enabled = false);
            try
            {
                autoReStart = chkAutoRestart.Checked;
                bool isConnected;
                lock (_connectionLock)
                {
                    isConnected = _isConnected;
                }

                if (!isConnected)
                {
                    await ConnectAsync(autoReStart);
                }
                else
                {
                    await DisconnectAsyncUI();
                }
            }
            finally
            {
                BeginInvoke(() => _btnConnect.Enabled = true);
            }
        }

        private async void btnSendJson_Click(object sender, EventArgs e)
        {
           await SendJsonAsync();
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            await UploadAsync();
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            await DownloadAsync();
        }

        private async void btnScreenshot_Click(object sender, EventArgs e)
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

        #region Stress Test Methods

        private async void btnStartStressTest_Click(object sender, EventArgs e)
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
                isConnected = _isConnected;
            }

            if (!isConnected || _api == null)
            {
                MessageBox.Show("請先連線到 Server!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse parameters
            if (!int.TryParse(_txtStressInterval.Text, out var intervalMs) || intervalMs < 10)
            {
                MessageBox.Show("間隔時間必須 >= 10ms", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(_txtStressSize.Text, out var sizeKB) || sizeKB < 1)
            {
                MessageBox.Show("資料大小必須 >= 1KB", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maxIterations = 0;
            if (_chkStressUnlimited.Checked)
            {
                maxIterations = 0; // 0 means unlimited
            }
            else
            {
                if (!int.TryParse(_txtStressIterations.Text, out maxIterations) || maxIterations < 1)
                {
                    MessageBox.Show("執行次數必須 >= 1", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Start stress test
            _isStressTesting = true;
            _stressTestIterations = 0;
            _stressTestSuccesses = 0;
            _stressTestFailures = 0;
            _stressTestStopwatch.Restart();
            _stressTestCts = new CancellationTokenSource();

            _btnStartStressTest.Text = "停止壓測";
            _btnStartStressTest.BackColor = Color.Red;
            _txtStressInterval.Enabled = false;
            _txtStressSize.Enabled = false;
            _txtStressIterations.Enabled = false;
            _chkStressUnlimited.Enabled = false;
            _cmbStressType.Enabled = false;

            _log.AppendText($"=== 壓力測試開始 ===\r\n");
            _log.AppendText($"測試類型: {_cmbStressType.Text}\r\n");
            _log.AppendText($"間隔: {intervalMs}ms, 大小: {sizeKB}KB, 次數: {(maxIterations == 0 ? "無限制" : maxIterations.ToString())}\r\n");

            try
            {
                await RunStressTestAsync(intervalMs, sizeKB, maxIterations, _stressTestCts.Token);
            }
            catch (OperationCanceledException)
            {
                _log.AppendText("壓力測試已取消\r\n");
            }
            catch (Exception ex)
            {
                _log.AppendText($"壓力測試錯誤: {ex.Message}\r\n");
            }
            finally
            {
                StopStressTest();
            }
        }

        private async Task RunStressTestAsync(int intervalMs, int sizeKB, int maxIterations, CancellationToken ct)
        {
            var testType = _cmbStressType.SelectedIndex;
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
                    _log.AppendText($"[第 {iteration} 次失敗] {ex.Message}\r\n");
                    UpdateStressTestStats();
                    try
                    {
                        await DisconnectAsyncUI();
                        await ConnectAsync();
                    }
                    finally { }
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
                ? (_stressTestSuccesses * 100.0 / _stressTestIterations)
                : 0;

            var avgTime = _stressTestIterations > 0
                ? elapsed.TotalMilliseconds / _stressTestIterations
                : 0;

            BeginInvoke(new Action(() =>
            {
                _lblStressStats.Text = $"執行: {_stressTestIterations} | 成功: {_stressTestSuccesses} | 失敗: {_stressTestFailures} | " +
                                       $"成功率: {successRate:F2}% | 平均: {avgTime:F0}ms | 總時間: {elapsed:hh\\:mm\\:ss}";
            }));
        }

        private void StopStressTest()
        {
            _isStressTesting = false;
            _stressTestStopwatch.Stop();
            _stressTestCts?.Dispose();
            _stressTestCts = null;

            BeginInvoke(new Action(() =>
            {
                _btnStartStressTest.Text = "開始壓測";
                _btnStartStressTest.BackColor = SystemColors.Control;
                _txtStressInterval.Enabled = true;
                _txtStressSize.Enabled = true;
                _txtStressIterations.Enabled = true;
                _chkStressUnlimited.Enabled = true;
                _cmbStressType.Enabled = true;

                _log.AppendText($"=== 壓力測試結束 ===\r\n");
                _log.AppendText($"總執行: {_stressTestIterations} 次\r\n");
                _log.AppendText($"成功: {_stressTestSuccesses} 次\r\n");
                _log.AppendText($"失敗: {_stressTestFailures} 次\r\n");
                _log.AppendText($"總時間: {_stressTestStopwatch.Elapsed:hh\\:mm\\:ss}\r\n\r\n");
            }));
        }

        private void _chkStressUnlimited_CheckedChanged(object sender, EventArgs e)
        {
            _txtStressIterations.Enabled = !_chkStressUnlimited.Checked;
        }

        #endregion
    }
}
