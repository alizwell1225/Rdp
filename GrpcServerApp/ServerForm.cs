using LIB_RPC;
using LIB_RPC.Abstractions;
using LIB_RPC.API;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LIB_Log;

namespace GrpcServerApp
{
    public partial class ServerForm : Form
    {
        private readonly IServerApi _controller;

        // Stress test monitoring fields
        private int _totalRequestsReceived = 0;
        private int _totalBytesReceived = 0;
        private Stopwatch _serverRuntime = new Stopwatch();
        private System.Windows.Forms.Timer? _statsUpdateTimer;

        // Server-side stress test fields
        private CancellationTokenSource? _stressTestCts;
        private bool _isStressTesting = false;
        private int _stressTestIterations = 0;
        private int _stressTestSuccesses = 0;
        private int _stressTestFailures = 0;
        private Stopwatch _stressTestStopwatch = new Stopwatch();
        LoggerServer _logger;

        public ServerForm()
        {
            InitializeComponent();
            _controller = new GrpcServerApi();

            // Wire controller events
            _controller.OnLog += ControllerOnLog;
            _controller.OnFileAdded += ControllerOnFileAdded;
            _controller.OnFileUploadCompleted += ControllerOnFileUploadCompleted;
            _controller.OnClientConnected += ControllerOnClientConnected;
            _controller.OnClientDisconnected += ControllerOnClientDisconnected;

            // Setup stats update timer
            _statsUpdateTimer = new System.Windows.Forms.Timer();
            _statsUpdateTimer.Interval = 1000; // Update every second
            _statsUpdateTimer.Tick += StatsUpdateTimerOnTick;


            _logger = new LoggerServer(Path.Combine(AppContext.BaseDirectory, "Log"), "controller");
        }

        private void ControllerOnLog(string line)
        {
            BeginInvoke(new Action(() =>
            {
                _log?.AppendText(line + Environment.NewLine);
                _logger.Info(line);
            }));
        }

        private void ControllerOnFileAdded(string path)
        {
            BeginInvoke(new Action(() =>
            {
                RefreshFiles();
                _log?.AppendText($"File added: {Path.GetFileName(path)}\r\n");
                _logger.Info($"File added: {Path.GetFileName(path)}");
                _totalRequestsReceived++;
                UpdateServerStats();
            }));
        }

        private void ControllerOnFileUploadCompleted(string fileName)
        {
            BeginInvoke(new Action(() =>
            {
                _totalRequestsReceived++;
                var fileInfo = new FileInfo(Path.Combine(_controller.Config?.StorageRoot ?? "", fileName));
                if (fileInfo.Exists)
                    _totalBytesReceived += (int)fileInfo.Length;
                UpdateServerStats();
            }));
        }

        private void StatsUpdateTimerOnTick(object? sender, EventArgs e)
        {
            UpdateServerStats();
        }

        private void ControllerOnClientConnected(string obj)
        {
            BeginInvoke(new Action(() =>
            {
                UpdateServerStats();
            }));
        }

        private void ControllerOnClientDisconnected(string obj)
        {
            BeginInvoke(new Action(() =>
            {
                UpdateServerStats();
            }));
        }


        private async Task StartAsync()
        {
            await _controller.StartAsync();
            _btnStart.Enabled = false;
            _btnStop.Enabled = true;
            _btnBroadcast.Enabled = true;
            _btnPushFile.Enabled = true;
            RefreshFiles();

            // Start monitoring
            _serverRuntime.Restart();
            _statsUpdateTimer?.Start();
            _totalRequestsReceived = 0;
            _totalBytesReceived = 0;
            UpdateServerStats();
        }

        private async Task StopAsync()
        {
            await _controller.StopAsync();
            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
            _btnBroadcast.Enabled = false;
            _btnPushFile.Enabled = false;

            // Stop monitoring
            _serverRuntime.Stop();
            _statsUpdateTimer?.Stop();
            UpdateServerStats();
        }

        private void UpdateServerStats()
        {
            var runtime = _serverRuntime.Elapsed;
            var avgRequestsPerSec = runtime.TotalSeconds > 0 ? _totalRequestsReceived / runtime.TotalSeconds : 0;
            var totalMB = _totalBytesReceived / (1024.0 * 1024.0);
            var avgMBPerSec = runtime.TotalSeconds > 0 ? totalMB / runtime.TotalSeconds : 0;

            _lblServerStats.Text = $"運行時間: {runtime:hh\\:mm\\:ss} | " +
                                   $"總請求: {_totalRequestsReceived} ({avgRequestsPerSec:F2}/秒) | " +
                                   $"總流量: {totalMB:F2} MB ({avgMBPerSec:F3} MB/秒) | ";
            _logger.Info(_lblServerStats.Text);
        }

        private void _btnResetStats_Click(object sender, EventArgs e)
        {
            _totalRequestsReceived = 0;
            _totalBytesReceived = 0;
            _serverRuntime.Restart();
            UpdateServerStats();
            _log?.AppendText("統計資料已重置\r\n");
        }

        private void RefreshFiles()
        {
            var cfg = _controller.Config;
            if (cfg == null) return;
            Directory.CreateDirectory(cfg.StorageRoot);
            _files.Items.Clear();
            foreach (var f in Directory.EnumerateFiles(cfg.StorageRoot).Select(Path.GetFileName))
            {
                if (f != null)
                    _files.Items.Add(f);
            }
        }

        private async Task BroadcastAsync(string type, string body, bool ackFlag = true)
        {
            try
            {
                if (ackFlag)
                {
                    var result = await _controller.BroadcastWithAckAsync("stress_test", body);//接收ack
                    if (!result.Success)
                    {
                        throw new Exception($"ACK mode failed: {result.Error}");
                    }
                }
                else
                {
                    await _controller.BroadcastJsonAsync("stress_test", body);
                    _log?.AppendText($"Broadcast sent type={type}\r\n");
                }
            }
            catch (Exception ex)
            {
                _log?.AppendText($"Broadcast error: {ex.Message}\r\n");
            }
        }

        private async Task PushFileAsync(string storagePath, bool useAckMode = true, bool UseStrogePath = false)
        {
            try
            {
                _controller.Config.CheckStorageRootHaveFile = UseStrogePath;
                if (useAckMode)
                {
                    // Use ACK mode with retry - pass full path
                    var result = await _controller.PushFileWithAckAsync(storagePath);
                    if (!result.Success)
                    {
                        throw new Exception($"ACK mode failed: {result.Error}");
                    }
                }
                else
                {
                    // Use streaming mode (existing) - pass full path
                    await _controller.PushFileAsync(storagePath);
                }
                _log?.AppendText($"Pushed file: {Path.GetFileName(storagePath)}\r\n");
            }
            catch (Exception ex)
            {
                _log?.AppendText($"Push file error: {ex.Message}\r\n");
            }
        }

        private async void _btnPushFile_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            await PushFileAsync(ofd.FileName);
        }

        private async void _btnBroadcast_Click(object sender, EventArgs e)
        {
            var type = string.IsNullOrWhiteSpace(_txtType.Text) ? "broadcast" : _txtType.Text.Trim();
            var body = string.IsNullOrWhiteSpace(_txtJson.Text) ? "{}" : _txtJson.Text;
            await BroadcastAsync(type, body);
        }

        private async void _btnStart_Click(object sender, EventArgs e)
        {
            await StartAsync();
        }

        private async void _btnStop_Click(object sender, EventArgs e)
        {
            await StopAsync();
        }

        // Apply host/port from designer and start the server
        private async void _btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                // find controls by name on the panel (they were added in Designer)
                var tbHost = txtHost.Text;
                var tbPort = txtPort.Text;
                string host = tbHost ?? "localhost";
                int port = 50051;
                if (int.TryParse(tbPort, out var p)) port = p;
                _controller.UpdateConfig(host, port);
                _log?.AppendText($"Config applied: {host}:{port}\r\n");
                // Optionally start immediately
                await StartAsync();
            }
            catch (Exception ex)
            {
                _log?.AppendText($"Apply error: {ex.Message}\r\n");
            }
        }

        #region Server Stress Test Methods

        private async void _btnStartStressTest_Click(object sender, EventArgs e)
        {
            if (_isStressTesting)
            {
                // Stop test
                _stressTestCts?.Cancel();
                return;
            }

            if (_controller.Config == null || !_btnStop.Enabled)
            {
                MessageBox.Show("請先啟動 Server!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            _log?.AppendText($"=== Server端壓力測試開始 ===\r\n");
            _log?.AppendText($"測試類型: {_cmbStressType.Text}\r\n");
            _log?.AppendText($"間隔: {intervalMs}ms, 大小: {sizeKB}KB, 次數: {(maxIterations == 0 ? "無限制" : maxIterations.ToString())}\r\n");

            try
            {
                await RunStressTestAsync(intervalMs, sizeKB, maxIterations, _stressTestCts.Token);
            }
            catch (OperationCanceledException)
            {
                _log?.AppendText("壓力測試已取消\r\n");
            }
            catch (Exception ex)
            {
                _log?.AppendText($"壓力測試錯誤: {ex.Message}\r\n");
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
                        case 0: // 廣播 JSON
                            await StressTestBroadcastAsync(sizeKB, ct);
                            break;
                        case 1: // 推送檔案
                            await StressTestPushFileAsync(sizeKB, ct);
                            break;
                        case 2: // 混合測試
                            await StressTestMixedAsync(sizeKB, ct);
                            break;
                    }

                    _stressTestSuccesses++;
                    UpdateStressTestStats();
                }
                catch (Exception ex)
                {
                    _stressTestFailures++;
                    _log?.AppendText($"[第 {iteration} 次失敗] {ex.Message}\r\n");
                    UpdateStressTestStats();
                }

                // Wait before next iteration
                await Task.Delay(intervalMs, ct);
            }
        }

        private async Task StressTestBroadcastAsync(int sizeKB, CancellationToken ct)
        {
            // Generate test JSON data
            var data = new string('X', sizeKB * 1024);
            var json = $"{{\"data\":\"{data}\",\"timestamp\":\"{DateTime.Now:O}\"}}";

            // Check if ACK mode is enabled
            bool useAckMode = false;
            int retryCount = 0;
            this.Invoke(() =>
            {
                useAckMode = _chkUseAckMode.Checked;
                retryCount = (int)_numRetryCount.Value;
            });

            if (useAckMode)
            {
                // Use ACK mode with retry
                var result = await _controller.BroadcastWithAckAsync("stress_test", json, retryCount, ct);
                if (!result.Success)
                {
                    throw new Exception($"ACK mode failed: {result.Error}");
                }
            }
            else
            {
                // Use streaming mode (existing)
                await _controller.BroadcastJsonAsync("stress_test", json);
            }
        }

        private async Task StressTestPushFileAsync(int sizeKB, CancellationToken ct)
        {
            // Create temp file in server storage
            var storageRoot = Path.Combine(Environment.CurrentDirectory, "storage");
            if (!Directory.Exists(storageRoot))
                Directory.CreateDirectory(storageRoot);

            var fileName = $"server_stress_{Guid.NewGuid()}.dat";
            var storagePath = Path.Combine(storageRoot, fileName);

            try
            {
                // Generate test data
                var data = new byte[sizeKB * 1024];
                new Random().NextBytes(data);
                await File.WriteAllBytesAsync(storagePath, data, ct);

                // Check if ACK mode is enabled
                bool useAckMode = false;
                int retryCount = 0;
                this.Invoke(() =>
                {
                    useAckMode = _chkUseAckMode.Checked;
                    retryCount = (int)_numRetryCount.Value;
                });
                _controller.Config.CheckStorageRootHaveFile = true;
                if (useAckMode)
                {
                    // Use ACK mode with retry - pass full path
                    var result = await _controller.PushFileWithAckAsync(storagePath, retryCount, ct);
                    if (!result.Success)
                    {
                        throw new Exception($"ACK mode failed: {result.Error}");
                    }
                }
                else
                {
                    // Use streaming mode (existing) - pass full path
                    await _controller.PushFileAsync(storagePath);
                }
            }
            finally
            {
                // Auto-delete temporary stress test file after sending
                if (File.Exists(storagePath))
                {
                    try
                    {
                        File.Delete(storagePath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private async Task StressTestMixedAsync(int sizeKB, CancellationToken ct)
        {
            // Randomly choose one of the operations
            var rnd = new Random();
            var operation = rnd.Next(2);

            switch (operation)
            {
                case 0:
                    await StressTestBroadcastAsync(sizeKB, ct);
                    break;
                case 1:
                    await StressTestPushFileAsync(sizeKB, ct);
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
                _log?.AppendText("=== Server端壓力測試結束 ===\r\n");
            }));
        }

        #endregion

        private void btnLog_Click(object sender, EventArgs e)
        {
            LogViewerForm dlg = new LogViewerForm();
            dlg.SetDefinePath(_controller.Config.LogFilePath);
            dlg.ShowDialog();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            txtHost.Text = _controller.Config?.Host ?? "localhost";
            txtPort.Text = (_controller.Config?.Port ?? 50051).ToString();
        }
    }
}
