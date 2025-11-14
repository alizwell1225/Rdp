using LIB_Define.RPC;
using System;
using System.Windows.Forms;

namespace TestGrpcServerApp
{
    public partial class ServerConfigForm : Form
    {
        private readonly GrpcServerHelper _serverHelper;
        
        public ServerConfigForm(GrpcServerHelper serverHelper)
        {
            _serverHelper = serverHelper;
            InitializeComponent();
            LoadCurrentConfig();
        }

        private void LoadCurrentConfig()
        {
            var config = _serverHelper.Config;
            txtHost.Text = config.Host;
            txtPort.Text = config.Port.ToString();
            txtStoragePath.Text = config.StorageRoot;
            txtLogPath.Text = config.LogFilePath;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(txtHost.Text))
                {
                    MessageBox.Show("Host cannot be empty.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
                {
                    MessageBox.Show("Port must be between 1 and 65535.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update configuration
                _serverHelper.UpdateConfig(txtHost.Text.Trim(), port);
                _serverHelper.Config.StorageRoot = txtStoragePath.Text.Trim();
                _serverHelper.Config.LogFilePath = txtLogPath.Text.Trim();
                
                // Save configuration
                if (_serverHelper.SaveConfig())
                {
                    MessageBox.Show("Configuration saved successfully.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Failed to save configuration.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnBrowseStorage_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select storage directory";
            folderDialog.SelectedPath = txtStoragePath.Text;
            
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtStoragePath.Text = folderDialog.SelectedPath;
            }
        }

        private void btnBrowseLog_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog();
            saveDialog.Title = "Select log file location";
            saveDialog.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";
            saveDialog.FileName = "grpc.log";
            
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                txtLogPath.Text = saveDialog.FileName;
            }
        }
    }
}
