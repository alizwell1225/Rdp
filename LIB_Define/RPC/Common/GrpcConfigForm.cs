using LIB_RPC;

namespace LIB_Define.RPC.Common
{
    /// <summary>
    /// Configuration form for GrpcConfig - supports both Server and Client modes
    /// </summary>
    public partial class GrpcConfigForm : Form
    {
        private GrpcConfig _config;
        private readonly string _configFilePath;
        private readonly bool _isServerMode;

        /// <summary>
        /// Creates a new GrpcConfigForm
        /// </summary>
        /// <param name="configFilePath">Path to save/load configuration</param>
        /// <param name="isServerMode">True for server mode, False for client mode</param>
        public GrpcConfigForm(string configFilePath, bool isServerMode = false)
        {
            InitializeComponent();
            _configFilePath = configFilePath;
            _isServerMode = isServerMode;
            
            // Set form title based on mode
            this.Text = isServerMode ? "Server Configuration" : "Client Configuration";
            
            // Load existing config or create new
            LoadConfiguration();
        }

        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public GrpcConfig Config => _config;

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    _config = GrpcConfig.Load(_configFilePath);
                }
                else
                {
                    _config = new GrpcConfig();
                }

                // Populate form fields
                txtHost.Text = _config.Host;
                numPort.Value = _config.Port;
                txtLogFilePath.Text = _config.LogFilePath;
                txtStorageRoot.Text = _config.StorageRoot;

                // Show/hide server-specific or client-specific options
                if (_isServerMode)
                {
                    lblMode.Text = "Mode: Server";
                    lblMode.ForeColor = System.Drawing.Color.DarkBlue;
                    
                    // Server typically uses StorageRoot for uploads
                    txtStorageRoot.Text = _config.GetServerUploadPath();
                }
                else
                {
                    lblMode.Text = "Mode: Client";
                    lblMode.ForeColor = System.Drawing.Color.DarkGreen;
                    
                    // Client typically uses StorageRoot for downloads
                    txtStorageRoot.Text = _config.GetClientDownloadPath();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", 
                    "Load Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(txtHost.Text))
                {
                    MessageBox.Show("Host cannot be empty.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtHost.Focus();
                    return;
                }

                if (numPort.Value < 1 || numPort.Value > 65535)
                {
                    MessageBox.Show("Port must be between 1 and 65535.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numPort.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtLogFilePath.Text))
                {
                    MessageBox.Show("Log file path cannot be empty.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtLogFilePath.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtStorageRoot.Text))
                {
                    MessageBox.Show("Storage root cannot be empty.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStorageRoot.Focus();
                    return;
                }

                // Update config object
                _config.Host = txtHost.Text.Trim();
                _config.Port = (int)numPort.Value;
                _config.LogFilePath = txtLogFilePath.Text.Trim();
                
                if (_isServerMode)
                {
                    _config.SetServerUploadPath(txtStorageRoot.Text.Trim());
                    // Keep StorageRoot as fallback
                    _config.StorageRoot = txtStorageRoot.Text.Trim();
                }
                else
                {
                    _config.SetServerUploadPath(txtStorageRoot.Text.Trim());
                    // Keep StorageRoot as fallback
                    _config.StorageRoot = txtStorageRoot.Text.Trim();
                }

                // Ensure directories exist
                _config.EnsureFolders();

                // Save to file
                _config.Save(_configFilePath);

                MessageBox.Show("Configuration saved successfully.", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        private void btnBrowseLogPath_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*";
                sfd.Title = "Select Log File Path";
                sfd.FileName = Path.GetFileName(txtLogFilePath.Text);
                
                if (!string.IsNullOrEmpty(txtLogFilePath.Text))
                {
                    var dir = Path.GetDirectoryName(txtLogFilePath.Text);
                    if (Directory.Exists(dir))
                        sfd.InitialDirectory = dir;
                }

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    txtLogFilePath.Text = sfd.FileName;
                }
            }
        }

        private void btnBrowseStorageRoot_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = _isServerMode 
                    ? "Select Server Storage Root (Upload Directory)" 
                    : "Select Client Storage Root (Download Directory)";
                
                if (Directory.Exists(txtStorageRoot.Text))
                    fbd.SelectedPath = txtStorageRoot.Text;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtStorageRoot.Text = fbd.SelectedPath;
                }
            }
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
                ofd.Title = "Load Configuration File";
                
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
                        _config = GrpcConfig.Load(ofd.FileName);
                        
                        // Refresh form fields
                        txtHost.Text = _config.Host;
                        numPort.Value = _config.Port;
                        txtLogFilePath.Text = _config.LogFilePath;
                        
                        if (_isServerMode)
                            txtStorageRoot.Text = _config.GetServerUploadPath();
                        else
                            txtStorageRoot.Text = _config.GetClientDownloadPath();

                        MessageBox.Show("Configuration loaded successfully.", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void btnResetDefaults_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Reset all settings to default values?", "Confirm Reset", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _config = new GrpcConfig();
                
                txtHost.Text = _config.Host;
                numPort.Value = _config.Port;
                txtLogFilePath.Text = _config.LogFilePath;
                txtStorageRoot.Text = _config.StorageRoot;
            }
        }
    }
}
