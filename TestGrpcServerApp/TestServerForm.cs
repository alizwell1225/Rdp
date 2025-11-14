using LIB_Define.RPC;
using LIB_Log;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TestGrpcServerApp
{
    public partial class TestServerForm : Form
    {
        private GrpcServerHelper? _serverHelper;
        private LoggerServer? _logger;
        
        public TestServerForm()
        {
            InitializeComponent();
            InitializeServerHelper();
        }

        private void InitializeServerHelper()
        {
            try
            {
                _serverHelper = new GrpcServerHelper();
                
                // Wire up event handlers
                _serverHelper.OnLog += ServerHelper_OnLog;
                _serverHelper.OnServerStarted += ServerHelper_OnServerStarted;
                _serverHelper.OnServerStopped += ServerHelper_OnServerStopped;
                _serverHelper.OnServerStartFailed += ServerHelper_OnServerStartFailed;
                _serverHelper.OnClientConnected += ServerHelper_OnClientConnected;
                _serverHelper.OnClientDisconnected += ServerHelper_OnClientDisconnected;
                _serverHelper.OnFileAdded += ServerHelper_OnFileAdded;
                _serverHelper.OnFileUploadCompleted += ServerHelper_OnFileUploadCompleted;
                
                // Try to load existing config
                if (_serverHelper.LoadConfig())
                {
                    AppendLog("Configuration loaded successfully.");
                }
                else
                {
                    AppendLog("Using default configuration.");
                }
                
                // Initialize logger
                var logPath = Path.Combine(AppContext.BaseDirectory, "Log");
                _logger = new LoggerServer(logPath, "TestServer");
                
                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize server: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ServerHelper_OnLog(string message)
        {
            BeginInvoke(() => 
            {
                AppendLog(message);
                _logger?.Info(message);
            });
        }

        private void ServerHelper_OnServerStarted()
        {
            BeginInvoke(() =>
            {
                AppendLog("Server started successfully!");
                UpdateUIState();
            });
        }

        private void ServerHelper_OnServerStopped()
        {
            BeginInvoke(() =>
            {
                AppendLog("Server stopped.");
                UpdateUIState();
            });
        }

        private void ServerHelper_OnServerStartFailed(string error)
        {
            BeginInvoke(() =>
            {
                AppendLog($"Server start failed: {error}");
                MessageBox.Show($"Failed to start server: {error}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void ServerHelper_OnClientConnected(string clientId)
        {
            BeginInvoke(() => AppendLog($"Client connected: {clientId}"));
        }

        private void ServerHelper_OnClientDisconnected(string clientId)
        {
            BeginInvoke(() => AppendLog($"Client disconnected: {clientId}"));
        }

        private void ServerHelper_OnFileAdded(string filePath)
        {
            BeginInvoke(() => AppendLog($"File added: {Path.GetFileName(filePath)}"));
        }

        private void ServerHelper_OnFileUploadCompleted(string fileName)
        {
            BeginInvoke(() => AppendLog($"File upload completed: {fileName}"));
        }

        private void AppendLog(string message)
        {
            if (txtLog == null) return;
            
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] {message}\r\n";
            
            // Limit log size
            if (txtLog.TextLength > 50000)
            {
                txtLog.Text = txtLog.Text.Substring(txtLog.TextLength - 40000);
            }
            
            txtLog.AppendText(logMessage);
        }

        private void UpdateUIState()
        {
            if (_serverHelper == null) return;
            
            bool isRunning = _serverHelper.IsRunning;
            
            btnStartServer.Enabled = !isRunning;
            btnStopServer.Enabled = isRunning;
            btnSendJson.Enabled = isRunning;
            btnSendFile.Enabled = isRunning;
            btnSendImage.Enabled = isRunning;
        }

        private async void btnOpenConfig_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serverHelper == null) return;
                
                var configForm = new ServerConfigForm(_serverHelper);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    AppendLog("Configuration updated.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serverHelper == null) return;
                
                btnStartServer.Enabled = false;
                AppendLog("Starting server...");
                
                var success = await _serverHelper.StartServerAsync();
                if (!success)
                {
                    btnStartServer.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Error starting server: {ex.Message}");
                btnStartServer.Enabled = true;
            }
        }

        private async void btnStopServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serverHelper == null) return;
                
                btnStopServer.Enabled = false;
                AppendLog("Stopping server...");
                
                await _serverHelper.StopServerAsync();
            }
            catch (Exception ex)
            {
                AppendLog($"Error stopping server: {ex.Message}");
            }
        }

        private async void btnSendJson_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serverHelper == null || !_serverHelper.IsRunning)
                {
                    MessageBox.Show("Server is not running!", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Show dialog to input JSON
                var jsonForm = new JsonInputForm();
                if (jsonForm.ShowDialog() == DialogResult.OK)
                {
                    AppendLog($"Broadcasting JSON message (Type: {jsonForm.MessageType})...");
                    
                    var result = await _serverHelper.BroadcastJsonAsync(
                        jsonForm.MessageType, 
                        jsonForm.JsonContent,
                        useAckMode: true);
                    
                    if (result.Success)
                    {
                        AppendLog($"JSON broadcast succeeded. Clients reached: {result.ClientsReached}");
                    }
                    else
                    {
                        AppendLog($"JSON broadcast failed: {result.Error}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Error sending JSON: {ex.Message}");
                MessageBox.Show($"Failed to send JSON: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serverHelper == null || !_serverHelper.IsRunning)
                {
                    MessageBox.Show("Server is not running!", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                using var ofd = new OpenFileDialog();
                ofd.Title = "Select file to send";
                ofd.Filter = "All files (*.*)|*.*";
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    AppendLog($"Pushing file: {Path.GetFileName(ofd.FileName)}...");
                    
                    var result = await _serverHelper.PushFileAsync(ofd.FileName, useAckMode: true);
                    
                    if (result.Success)
                    {
                        AppendLog($"File push succeeded. Clients reached: {result.ClientsReached}");
                    }
                    else
                    {
                        AppendLog($"File push failed: {result.Error}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Error sending file: {ex.Message}");
                MessageBox.Show($"Failed to send file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSendImage_Click(object sender, EventArgs e)
        {
            try
            {
                if (_serverHelper == null || !_serverHelper.IsRunning)
                {
                    MessageBox.Show("Server is not running!", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                using var ofd = new OpenFileDialog();
                ofd.Title = "Select image to send";
                ofd.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*";
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Validate it's an image
                    if (!IsImageFile(ofd.FileName))
                    {
                        MessageBox.Show("Selected file is not a valid image.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    AppendLog($"Pushing image: {Path.GetFileName(ofd.FileName)}...");
                    
                    var result = await _serverHelper.PushFileAsync(ofd.FileName, useAckMode: true);
                    
                    if (result.Success)
                    {
                        AppendLog($"Image push succeeded. Clients reached: {result.ClientsReached}");
                    }
                    else
                    {
                        AppendLog($"Image push failed: {result.Error}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Error sending image: {ex.Message}");
                MessageBox.Show($"Failed to send image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsImageFile(string filePath)
        {
            try
            {
                var ext = Path.GetExtension(filePath).ToLowerInvariant();
                return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || 
                       ext == ".bmp" || ext == ".gif";
            }
            catch
            {
                return false;
            }
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            try
            {
                var logPath = Path.Combine(AppContext.BaseDirectory, "Log");
                
                if (!Directory.Exists(logPath))
                {
                    MessageBox.Show("Log directory does not exist.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // Open log directory in file explorer
                System.Diagnostics.Process.Start("explorer.exe", logPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open log directory: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            AppendLog("Log cleared.");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            if (_serverHelper != null && _serverHelper.IsRunning)
            {
                var result = MessageBox.Show(
                    "Server is still running. Stop server and exit?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    _serverHelper.StopServerAsync().GetAwaiter().GetResult();
                    _serverHelper.Dispose();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
