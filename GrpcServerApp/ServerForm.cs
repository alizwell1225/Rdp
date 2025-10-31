using System;
using System.IO;
using System.Linq;
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
        }

        private async Task StopAsync()
        {
            await _controller.StopAsync();
            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
            _btnBroadcast.Enabled = false;
            _btnPushFile.Enabled = false;
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
