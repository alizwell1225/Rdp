using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LIB_RPC;
using LIB_RPC.API;
using LIB_RPC.Abstractions;

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

        public ServerForm()
        {
            InitializeComponent();
            _controller = new GrpcServerApi();
            // wire controller events
            _controller.OnLog += line => BeginInvoke(new Action(() => _log.AppendText(line + Environment.NewLine)));
            _controller.OnFileAdded += path => BeginInvoke(new Action(() =>
            {
                RefreshFiles();
                _log.AppendText($"File added: {Path.GetFileName(path)}\r\n");
                _totalRequestsReceived++;
                UpdateServerStats();
            }));

            // Wire additional events for monitoring
            _controller.OnFileUploadCompleted += (clientId, fileName) => BeginInvoke(new Action(() =>
            {
                _totalRequestsReceived++;
                var fileInfo = new FileInfo(Path.Combine(_controller.Config?.StorageRoot ?? "", fileName));
                if (fileInfo.Exists)
                    _totalBytesReceived += (int)fileInfo.Length;
                UpdateServerStats();
            }));

            _controller.OnClientConnected += clientId => BeginInvoke(new Action(() =>
            {
                UpdateServerStats();
            }));

            _controller.OnClientDisconnected += clientId => BeginInvoke(new Action(() =>
            {
                UpdateServerStats();
            }));

            // Setup stats update timer
            _statsUpdateTimer = new System.Windows.Forms.Timer();
            _statsUpdateTimer.Interval = 1000; // Update every second
            _statsUpdateTimer.Tick += (s, e) => UpdateServerStats();
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
                                   $"總流量: {totalMB:F2} MB ({avgMBPerSec:F3} MB/秒) | " +
                                   $"連線數: {GetConnectedClientCount()}";
        }

        private int GetConnectedClientCount()
        {
            // This would require tracking connected clients in the API
            // For now, we'll just return 0 as a placeholder
            return 0;
        }

        private void _btnResetStats_Click(object sender, EventArgs e)
        {
            _totalRequestsReceived = 0;
            _totalBytesReceived = 0;
            _serverRuntime.Restart();
            UpdateServerStats();
            _log.AppendText("統計資料已重置\r\n");
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

        private async Task BroadcastAsync()
        {
            try
            {
                var type = string.IsNullOrWhiteSpace(_txtType.Text) ? "broadcast" : _txtType.Text.Trim();
                var body = string.IsNullOrWhiteSpace(_txtJson.Text) ? "{}" : _txtJson.Text;
                await _controller.BroadcastJsonAsync(type, body);
                _log.AppendText($"Broadcast sent type={type}\r\n");
            }
            catch (Exception ex)
            {
                _log.AppendText($"Broadcast error: {ex.Message}\r\n");
            }
        }

        private async Task PushFileAsync()
        {
            using var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            try
            {
                await _controller.PushFileAsync(ofd.FileName);
                _log.AppendText($"Pushed file: {Path.GetFileName(ofd.FileName)}\r\n");
            }
            catch (Exception ex)
            {
                _log.AppendText($"Push file error: {ex.Message}\r\n");
            }
        }

        private async void _btnPushFile_Click(object sender, EventArgs e)
        {
            await PushFileAsync();
        }

        private async void _btnBroadcast_Click(object sender, EventArgs e)
        {
            await BroadcastAsync();
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
                _log.AppendText($"Config applied: {host}:{port}\r\n");
                // Optionally start immediately
                await StartAsync();
            }
            catch (Exception ex)
            {
                _log.AppendText($"Apply error: {ex.Message}\r\n");
            }
        }
    }
}
