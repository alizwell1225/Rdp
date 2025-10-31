using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using LIB_RPC;
using LIB_RPC.API;
using LIB_RPC.Abstractions;

namespace GrpcClientApp
{
    public partial class ClientForm : Form
    {
        private IClientApi? _api;
        private GrpcConfig? _config;
        private bool _isConnected = false;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object? sender, EventArgs e)
        {

        }


        private async void _btnApply_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtPort?.Text, out var port)) { MessageBox.Show("Port 必須為數字"); return; }
            _config = new GrpcConfig { Host = txtHost?.Text ?? "localhost", Port = port };
            // dispose previous api if any
            if (_api != null) await _api.DisposeAsync();
            _api = new GrpcClientApi(_config);
            // enable/disable controls handled by ConnectAsync
            await ConnectAsync();

        }

        private async Task ConnectAsync()
        {
            _config ??= new GrpcConfig();
            _api ??= new GrpcClientApi(_config);
            // forward api logs to UI
            _api.OnLog += line => BeginInvoke(new Action(() => _log.AppendText(line + Environment.NewLine)));
            _api.OnUploadProgress += (path, percent) => BeginInvoke(new Action(() =>
            {
                _pbUpload.Value = (int)Math.Min(100, Math.Round(percent));
                _lblUpload.Text = $"Upload: {percent:F2}% ({path})";
            }));
            _api.OnDownloadProgress += (path, percent) => BeginInvoke(new Action(() =>
            {
                _pbDownload.Value = (int)Math.Min(100, Math.Round(percent));
                _lblDownload.Text = $"Download: {percent:F2}% ({path})";
            }));
            _api.OnScreenshotProgress += percent => BeginInvoke(new Action(() =>
            {
                _pbScreenshot.Value = (int)Math.Min(100, Math.Round(percent));
                _lblScreenshot.Text = $"Screenshot: {percent:F2}%";
                if (percent >= 100) _pbScreenshot.Value = 0; // 完成後歸零方便下次
            }));
            _api.OnServerJson += env => BeginInvoke(new Action(() =>
            {
                _log.AppendText($"[ServerPush] type={env.Type} id={env.Id} bytes={env.Json?.Length}\r\n");
            }));
            await _api.ConnectAsync();
            _isConnected = true;
            UpdateUiConnectedState(true);
        }

        private void UpdateUiConnectedState(bool connected)
        {
            // toggle buttons and text
            _btnSendJson.Enabled = connected;
            _btnUpload.Enabled = connected;
            _btnDownload.Enabled = connected;
            _btnScreenshot.Enabled = connected;
            _btnConnect.Text = connected ? "Disconnect" : "Connect to Server";
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
                _isConnected = false;
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
                btnOk.Click += (_, _) => { if (lb.SelectedItem != null) dlg.DialogResult = DialogResult.OK; else MessageBox.Show("請選擇一個檔案"); };
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
            using var ms = new MemoryStream(bytes);
            _pic.Image = Image.FromStream(ms);
            _log.AppendText($"Screenshot 顯示\r\n");
        }

        private async void _btnConnect_Click(object sender, EventArgs e)
        {
            // Toggle connect / disconnect
            _btnConnect.Enabled = false;
            try
            {
                if (!_isConnected)
                {
                    await ConnectAsync();
                }
                else
                {
                    await DisconnectAsyncUI();
                }
            }
            finally
            {
                _btnConnect.Enabled = true;
            }
        }

        private async void _btnSendJson_Click(object sender, EventArgs e)
        {
           await SendJsonAsync();
        }

        private async void _btnUpload_Click(object sender, EventArgs e)
        {
            await UploadAsync();
        }

        private async void _btnDownload_Click(object sender, EventArgs e)
        {
            await DownloadAsync();
        }

        private async void _btnScreenshot_Click(object sender, EventArgs e)
        {
             await ScreenshotAsync();
        }
    }
}
