using LIB_Define.RPC.Client_org;

namespace LIB_Define.RPC
{
    /// <summary>
    /// Form for configuring multiple RpcClient instances on a single machine
    /// </summary>
    public partial class MultiClientConfigForm : Form
    {
        private MultiClientConfig _config;
        private readonly string _configFilePath;
        public event Action<int,bool>? EvConfigurationSaved;

        public MultiClientConfigForm(string configFilePath)
        {
            InitializeComponent();
            _configFilePath = configFilePath;
            LoadConfiguration();
        }

        /// <summary>
        /// Gets the current multi-client configuration
        /// </summary>
        public MultiClientConfig Config => _config;

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    _config = MultiClientConfig.Load(_configFilePath);
                }
                else
                {
                    _config = new MultiClientConfig();
                }

                // Update UI
                numClientCount.Value = _config.ClientCount;
                RefreshClientGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RefreshClientGrid()
        {
            dgvClients.Rows.Clear();
            
            foreach (var clientRef in _config.Clients)
            {
                // Load the actual config for display
                var config = clientRef.LoadConfig();
                
                int rowIndex = dgvClients.Rows.Add();
                var row = dgvClients.Rows[rowIndex];
                
                row.Cells["colIndex"].Value = clientRef.Index;
                row.Cells["colEnabled"].Value = clientRef.Enabled;
                row.Cells["colDisplayName"].Value = $"Client {clientRef.Index}";
                row.Cells["colHost"].Value = config.Host;
                row.Cells["colPort"].Value = config.Port;
                row.Cells["colLogPath"].Value = config.LogFilePath;
                row.Cells["colStoragePath"].Value = config.GetClientDownloadPath();
                row.Cells["colConfigPath"].Value = clientRef.ConfigPath;
                
                // Color code based on enabled state
                if (!clientRef.Enabled)
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                    row.DefaultCellStyle.ForeColor = Color.DarkGray;
                }
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                // Update config from grid and save individual configs
                for (int i = 0; i < dgvClients.Rows.Count && i < _config.Clients.Count; i++)
                {
                    var row = dgvClients.Rows[i];
                    var clientRef = _config.Clients[i];

                    // Update enabled state and config path in the reference
                    clientRef.Enabled = row.Cells["colEnabled"].Value is bool enabled && enabled;
                    var configPath = row.Cells["colConfigPath"].Value?.ToString() ?? $"./Config/client_{i}_config.json";
                    clientRef.ConfigPath = NormalizePath(configPath);
                    
                    // Load or create the individual config
                    var config = clientRef.LoadConfig();
                    
                    // Update config values from grid
                    config.Host = row.Cells["colHost"].Value?.ToString() ?? "localhost";
                    
                    if (int.TryParse(row.Cells["colPort"].Value?.ToString(), out int port))
                        config.Port = port;
                    
                    config.LogFilePath = NormalizePath(row.Cells["colLogPath"].Value?.ToString() ?? $"./Logs/client_{i}_grpc.log");
                    var storageRoot = NormalizePath(row.Cells["colStoragePath"].Value?.ToString() ?? $"./Storage/client_{i}");
                    config.StorageRoot = storageRoot;
                    config.ClientDownloadPath = storageRoot;
                    
                    // Save individual config file
                    if (clientRef.Enabled)
                    {
                        clientRef.SaveConfig(config);
                        EvConfigurationSaved?.Invoke(i,true);
                    }
                }

                // Save multi-client config (only references)
                _config.Save(_configFilePath);

                MessageBox.Show("Configuration saved successfully.\nAll enabled client configurations have been updated.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void numClientCount_ValueChanged(object sender, EventArgs e)
        {
            int newCount = (int)numClientCount.Value;
            _config.EnsureClientCount(newCount);
            RefreshClientGrid();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfiguration();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                ofd.Title = "Load Multi-Client Configuration";

                if (!string.IsNullOrEmpty(_configFilePath))
                {
                    var dir = Path.GetDirectoryName(_configFilePath);
                    if (Directory.Exists(dir))
                        ofd.InitialDirectory = dir;
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _config = MultiClientConfig.Load(ofd.FileName);
                        numClientCount.Value = _config.ClientCount;
                        RefreshClientGrid();

                        MessageBox.Show("Configuration loaded successfully.",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading configuration: {ex.Message}",
                            "Load Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnApplyTemplate_Click(object sender, EventArgs e)
        {
            using (var templateForm = new Form())
            {
                templateForm.Text = "Apply Template Settings";
                templateForm.Size = new Size(500, 300);
                templateForm.StartPosition = FormStartPosition.CenterParent;
                templateForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                templateForm.MaximizeBox = false;
                templateForm.MinimizeBox = false;

                var lblHost = new Label { Text = "Host:", Location = new Point(20, 20), AutoSize = true };
                var txtHost = new TextBox { Location = new Point(150, 17), Width = 310, Text = "localhost" };

                var lblPort = new Label { Text = "Base Port:", Location = new Point(20, 50), AutoSize = true };
                var numPort = new NumericUpDown 
                { 
                    Location = new Point(150, 47), 
                    Width = 100, 
                    Minimum = 1, 
                    Maximum = 65535, 
                    Value = 50051 
                };

                var chkIncrementPort = new CheckBox 
                { 
                    Text = "Increment port for each client (+1)", 
                    Location = new Point(150, 75), 
                    Width = 310,
                    Checked = true
                };

                var lblLogPath = new Label { Text = "Log Base Path:", Location = new Point(20, 110), AutoSize = true };
                var txtLogPath = new TextBox { Location = new Point(150, 107), Width = 260, Text = "./Logs" };
                var btnBrowseLog = new Button { Text = "...", Location = new Point(420, 106), Width = 40 };

                var lblStoragePath = new Label { Text = "Storage Base Path:", Location = new Point(20, 145), AutoSize = true };
                var txtStoragePath = new TextBox { Location = new Point(150, 142), Width = 260, Text = "./Storage" };
                var btnBrowseStorage = new Button { Text = "...", Location = new Point(420, 141), Width = 40 };

                var lblInfo = new Label 
                { 
                    Text = "Each client will have a subfolder: client_0, client_1, etc.", 
                    Location = new Point(20, 180), 
                    AutoSize = false,
                    Width = 440,
                    Height = 30
                };

                var btnOK = new Button { Text = "Apply", Location = new Point(305, 225), Width = 75 };
                var btnCancelTemplate = new Button { Text = "Cancel", Location = new Point(390, 225), Width = 75 };

                btnBrowseLog.Click += (s, ev) =>
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Select Log Base Directory";
                        fbd.ShowNewFolderButton = true;
                        if (!string.IsNullOrEmpty(txtLogPath.Text))
                            fbd.SelectedPath = Path.GetFullPath(txtLogPath.Text);
                        
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            txtLogPath.Text = fbd.SelectedPath;
                        }
                    }
                };

                btnBrowseStorage.Click += (s, ev) =>
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Select Storage Base Directory";
                        fbd.ShowNewFolderButton = true;
                        if (!string.IsNullOrEmpty(txtStoragePath.Text))
                            fbd.SelectedPath = Path.GetFullPath(txtStoragePath.Text);
                        
                        if (fbd.ShowDialog() == DialogResult.OK)
                        {
                            txtStoragePath.Text = fbd.SelectedPath;
                        }
                    }
                };

                btnOK.Click += (s, ev) =>
                {
                    // Apply template to all enabled clients
                    int basePort = (int)numPort.Value;
                    
                    for (int i = 0; i < dgvClients.Rows.Count && i < _config.Clients.Count; i++)
                    {
                        var clientRef = _config.Clients[i];
                        if (clientRef.Enabled)
                        {
                            var row = dgvClients.Rows[i];
                            row.Cells["colHost"].Value = txtHost.Text;
                            row.Cells["colPort"].Value = chkIncrementPort.Checked ? basePort + clientRef.Index : basePort;
                            row.Cells["colLogPath"].Value = NormalizePath(Path.Combine(txtLogPath.Text, $"client_{clientRef.Index}_grpc.log"));
                            row.Cells["colStoragePath"].Value = NormalizePath(Path.Combine(txtStoragePath.Text, $"client_{clientRef.Index}"));
                        }
                    }
                    
                    templateForm.DialogResult = DialogResult.OK;
                    templateForm.Close();
                };

                btnCancelTemplate.Click += (s, ev) =>
                {
                    templateForm.DialogResult = DialogResult.Cancel;
                    templateForm.Close();
                };

                templateForm.Controls.AddRange(new Control[] 
                { 
                    lblHost, txtHost, 
                    lblPort, numPort, chkIncrementPort,
                    lblLogPath, txtLogPath, btnBrowseLog,
                    lblStoragePath, txtStoragePath, btnBrowseStorage,
                    lblInfo,
                    btnOK, btnCancelTemplate 
                });

                templateForm.ShowDialog(this);
            }
        }

        private void btnEditSelected_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a client to edit.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            int selectedIndex = dgvClients.SelectedRows[0].Index;
            if (selectedIndex >= 0 && selectedIndex < _config.Clients.Count)
            {
                var clientRef = _config.Clients[selectedIndex];
                var configPath = clientRef.ConfigPath;

                // Show individual config form for this client
                using (var configForm = new GrpcConfigForm(configPath, isServerMode: false))
                {
                    if (configForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Reload the grid to show updated values
                        RefreshClientGrid();
                    }
                }
            }
        }

        private void btnEnableAll_Click(object sender, EventArgs e)
        {
            foreach (var clientRef in _config.Clients)
            {
                clientRef.Enabled = true;
            }
            RefreshClientGrid();
        }

        private void btnDisableAll_Click(object sender, EventArgs e)
        {
            foreach (var clientRef in _config.Clients)
            {
                clientRef.Enabled = false;
            }
            RefreshClientGrid();
        }

        private void dgvClients_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _config.Clients.Count)
            {
                var clientRef = _config.Clients[e.RowIndex];
                var row = dgvClients.Rows[e.RowIndex];

                // Update color based on enabled state
                if (e.ColumnIndex == dgvClients.Columns["colEnabled"].Index)
                {
                    bool isEnabled = row.Cells["colEnabled"].Value is bool enabled && enabled;
                    clientRef.Enabled = isEnabled;
                    
                    if (!isEnabled)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        row.DefaultCellStyle.ForeColor = Color.DarkGray;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }

        private void dgvClients_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvClients.IsCurrentCellDirty && 
                dgvClients.CurrentCell.ColumnIndex == dgvClients.Columns["colEnabled"].Index)
            {
                dgvClients.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvClients_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Handle double-click on LogPath column
            if (e.ColumnIndex == dgvClients.Columns["colLogPath"].Index)
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Select Log File Path";
                    sfd.Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*";
                    sfd.FileName = dgvClients.Rows[e.RowIndex].Cells["colLogPath"].Value?.ToString() ?? "grpc.log";
                    
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        dgvClients.Rows[e.RowIndex].Cells["colLogPath"].Value = NormalizePath(sfd.FileName);
                    }
                }
            }
            // Handle double-click on StoragePath column
            else if (e.ColumnIndex == dgvClients.Columns["colStoragePath"].Index)
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Select Storage Directory";
                    fbd.ShowNewFolderButton = true;
                    
                    var currentPath = dgvClients.Rows[e.RowIndex].Cells["colStoragePath"].Value?.ToString();
                    if (!string.IsNullOrEmpty(currentPath) && Directory.Exists(currentPath))
                    {
                        fbd.SelectedPath = Path.GetFullPath(currentPath);
                    }
                    
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        dgvClients.Rows[e.RowIndex].Cells["colStoragePath"].Value = NormalizePath(fbd.SelectedPath);
                    }
                }
            }
        }

        private void btnImportRdpIPs_Click(object sender, EventArgs e)
        {
            try
            {
                // Load RDP connections configuration
                string rdpConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "Rdp_Connections.json");
                
                if (!File.Exists(rdpConfigPath))
                {
                    MessageBox.Show("未找到 RDP 連線配置檔案 (Rdp_Connections.json)\nRDP connection configuration file not found.",
                        "配置檔案不存在 / File Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Read and deserialize RDP configuration
                var json = File.ReadAllText(rdpConfigPath);
                var rdpProfiles = System.Text.Json.JsonSerializer.Deserialize<List<LIB_RDP.Models.RdpConnectionProfile>>(json);

                if (rdpProfiles == null || rdpProfiles.Count == 0)
                {
                    MessageBox.Show("RDP 配置檔案中沒有連線資訊\nNo RDP connection information found.",
                        "無資料 / No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Import IP addresses to grid
                int importedCount = 0;
                for (int i = 0; i < rdpProfiles.Count && i < dgvClients.Rows.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(rdpProfiles[i].HostName))
                    {
                        dgvClients.Rows[i].Cells["colHost"].Value = rdpProfiles[i].HostName;
                        importedCount++;
                    }
                }

                MessageBox.Show($"已從 RDP 配置匯入 {importedCount} 個 IP 位址\nImported {importedCount} IP addresses from RDP configuration.",
                    "匯入成功 / Import Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匯入 RDP IP 位址失敗 / Failed to import RDP IP addresses:\n{ex.Message}",
                    "錯誤 / Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Normalize path to use forward slashes
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            return path.Replace('\\', '/');
        }
    }
}
