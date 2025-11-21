using LIB_Define.RPC.Config;
using LIB_RDP.Core;

namespace LIB_Define.RDP
{
    public partial class FormMultiRdpSettings : Form
    {
        private RdpManager _rdpManager;


        public FormMultiRdpSettings(RdpManager rdpManager)
        {
            InitializeComponent();
            _rdpManager=rdpManager;
            _rdpManager.ConfigurationManager.SetUseJsonFormat(true);
            LoadSettings();
        }

        private void LoadSettings()
        {
            var data=_rdpManager.ConfigurationManager.LoadAllConnections();
            UpdateConnectionGrid(data.Count);

            for (int i = 0; i < _rdpManager.MaxConnections; i++)
            {
                var conn = _rdpManager.ConfigurationManager.FindConnIndex(i);
                dgvSettings.Rows[i].Cells["HostName"].Value = conn.HostName;
                dgvSettings.Rows[i].Cells["UserName"].Value = conn.UserName;
                dgvSettings.Rows[i].Cells["Password"].Value = conn.GetSecureCredentials()?.GetPassword()==null?"": conn.GetSecureCredentials().GetPassword();
            }
        }

        private void UpdateConnectionGrid(int count)
        {
            while (dgvSettings.Rows.Count < count)
            {
                int index = dgvSettings.Rows.Add();
                dgvSettings.Rows[index].Cells["Index"].Value = index + 1;
            }

            while (dgvSettings.Rows.Count > count)
            {
                dgvSettings.Rows.RemoveAt(dgvSettings.Rows.Count - 1);
            }
        }

        private void SaveSettings()
        {
           // _settings.ConnectionsInfo.Clear();

            for (int i = 0; i < dgvSettings.Rows.Count; i++)
            {
                var conn = _rdpManager.ConfigurationManager.FindConnIndex(i);
                conn.HostName = dgvSettings.Rows[i].Cells["HostName"].Value?.ToString() ?? "";
                var _UserName = dgvSettings.Rows[i].Cells["UserName"].Value?.ToString() ?? "";
                var _Password = dgvSettings.Rows[i].Cells["Password"].Value?.ToString() ?? "";
                conn.SetCredentials(_UserName,_Password);
                _rdpManager.ConfigurationManager.SaveConnection(conn);
            }
            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }


        private void btnApply_Click(object sender, EventArgs e)
        {
            Apply();
        }

        void Apply()
        {
            SaveSettings();
        }

        private void btnApplyTemplate_Click(object sender, EventArgs e)
        {
            using (var templateForm = new Form())
            {
                templateForm.Text = "批次設定模板";
                templateForm.Size = new Size(450, 280);
                templateForm.StartPosition = FormStartPosition.CenterParent;
                templateForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                templateForm.MaximizeBox = false;
                templateForm.MinimizeBox = false;

                var lblHostBase = new Label { Text = "主機位址基礎:", Location = new Point(20, 20), AutoSize = true };
                var txtHostBase = new TextBox { Location = new Point(150, 17), Width = 250, Text = "192.168.1." };

                var lblHostStart = new Label { Text = "起始編號:", Location = new Point(20, 50), AutoSize = true };
                var numHostStart = new NumericUpDown 
                { 
                    Location = new Point(150, 47), 
                    Width = 100, 
                    Minimum = 1, 
                    Maximum = 255, 
                    Value = 1 
                };

                var chkAutoIncrement = new CheckBox 
                { 
                    Text = "自動遞增主機編號", 
                    Location = new Point(150, 75), 
                    Width = 250,
                    Checked = true
                };

                var lblUserName = new Label { Text = "使用者名稱:", Location = new Point(20, 110), AutoSize = true };
                var txtUserName = new TextBox { Location = new Point(150, 107), Width = 250, Text = "Administrator" };

                var lblPassword = new Label { Text = "密碼:", Location = new Point(20, 140), AutoSize = true };
                var txtPassword = new TextBox { Location = new Point(150, 137), Width = 250, Text = "", PasswordChar = '*' };

                var chkApplyToAll = new CheckBox 
                { 
                    Text = "套用到所有連線", 
                    Location = new Point(150, 170), 
                    Width = 250,
                    Checked = true
                };

                var btnOK = new Button { Text = "套用", Location = new Point(250, 210), Width = 75 };
                var btnCancelTemplate = new Button { Text = "取消", Location = new Point(335, 210), Width = 75 };

                btnOK.Click += (s, ev) =>
                {
                    try
                    {
                        string hostBase = txtHostBase.Text.Trim();
                        int hostStart = (int)numHostStart.Value;
                        string userName = txtUserName.Text.Trim();
                        string password = txtPassword.Text;
                        bool applyToAll = chkApplyToAll.Checked;
                        bool autoIncrement = chkAutoIncrement.Checked;

                        int connectionCount = applyToAll ? dgvSettings.Rows.Count : Math.Min(dgvSettings.Rows.Count, 1);

                        for (int i = 0; i < connectionCount; i++)
                        {
                            var row = dgvSettings.Rows[i];
                            
                            // 設定主機位址
                            if (autoIncrement)
                            {
                                row.Cells["HostName"].Value = hostBase + (hostStart + i).ToString();
                            }
                            else
                            {
                                row.Cells["HostName"].Value = hostBase + hostStart.ToString();
                            }
                            
                            // 設定使用者名稱和密碼
                            if (!string.IsNullOrEmpty(userName))
                            {
                                row.Cells["UserName"].Value = userName;
                            }
                            
                            if (!string.IsNullOrEmpty(password))
                            {
                                row.Cells["Password"].Value = password;
                            }
                        }

                        MessageBox.Show($"已套用設定到 {connectionCount} 個連線", "完成", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        templateForm.DialogResult = DialogResult.OK;
                        templateForm.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"套用設定時發生錯誤: {ex.Message}", "錯誤", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnCancelTemplate.Click += (s, ev) =>
                {
                    templateForm.DialogResult = DialogResult.Cancel;
                    templateForm.Close();
                };

                templateForm.Controls.AddRange(new Control[] 
                { 
                    lblHostBase, txtHostBase,
                    lblHostStart, numHostStart, chkAutoIncrement,
                    lblUserName, txtUserName, 
                    lblPassword, txtPassword,
                    chkApplyToAll,
                    btnOK, btnCancelTemplate 
                });

                templateForm.ShowDialog(this);
            }
        }

        private void chkUseJsonFormat_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void btnImportClientConfigIPs_Click(object sender, EventArgs e)
        {
            try
            {
                // Load Multi-Client configuration
                string multiClientConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "multi_client_config.json");
                
                if (!File.Exists(multiClientConfigPath))
                {
                    MessageBox.Show("未找到多客戶端配置檔案 (multi_client_config.json)\nMulti-client configuration file not found.",
                        "配置檔案不存在 / File Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Read and deserialize multi-client configuration
                var json = File.ReadAllText(multiClientConfigPath);
                var multiClientConfig = System.Text.Json.JsonSerializer.Deserialize<MultiClientConfig>(json);

                if (multiClientConfig == null || multiClientConfig.Clients == null || multiClientConfig.Clients.Count == 0)
                {
                    MessageBox.Show("多客戶端配置檔案中沒有客戶端資訊\nNo client information found in multi-client configuration.",
                        "無資料 / No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Import IP addresses from individual client configs
                int importedCount = 0;
                for (int i = 0; i < multiClientConfig.Clients.Count && i < dgvSettings.Rows.Count; i++)
                {
                    var clientRef = multiClientConfig.Clients[i];
                    if (clientRef.Enabled)
                    {
                        try
                        {
                            var clientConfig = clientRef.LoadConfig();
                            if (!string.IsNullOrWhiteSpace(clientConfig.Host))
                            {
                                dgvSettings.Rows[i].Cells["HostName"].Value = clientConfig.Host;
                                importedCount++;
                            }
                        }
                        catch
                        {
                            // Skip if individual config cannot be loaded
                            continue;
                        }
                    }
                }

                MessageBox.Show($"已從客戶端配置匯入 {importedCount} 個 IP 位址\nImported {importedCount} IP addresses from client configuration.",
                    "匯入成功 / Import Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匯入客戶端 IP 位址失敗 / Failed to import client IP addresses:\n{ex.Message}",
                    "錯誤 / Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    /*
      public partial class FormProxySettings : Form
       {
           private RdpManager _rdpManager;
           //private readonly ProxySettings _settings;

           public FormProxySettings(RdpManager rdpManager)
           {
               InitializeComponent();
               _rdpManager=rdpManager;
               //_settings = _rdpManager.RdpProxySettings;
               LoadSettings();
           }

           private void LoadSettings()
           {
               var data=_rdpManager.ConfigurationManager.LoadAllConnections();
               UpdateConnectionGrid();

               for (int i = 0; i < _settings.ConnectionsInfo.Count && i < dgvSettings.Rows.Count; i++)
               {
                   var conn = _settings.ConnectionsInfo[i];
                   dgvSettings.Rows[i].Cells["HostName"].Value = conn.HostName;
                   dgvSettings.Rows[i].Cells["UserName"].Value = conn.UserName;
                   dgvSettings.Rows[i].Cells["Password"].Value = conn.Password;
               }
           }

           private void UpdateConnectionGrid()
           {
               
               int count = _settings.RpcCount;

               while (dgvSettings.Rows.Count < count)
               {
                   int index = dgvSettings.Rows.Add();
                   dgvSettings.Rows[index].Cells["Index"].Value = index + 1;
               }

               while (dgvSettings.Rows.Count > count)
               {
                   dgvSettings.Rows.RemoveAt(dgvSettings.Rows.Count - 1);
               }
           }

           private void SaveSettings()
           {
               _settings.ConnectionsInfo.Clear();

               for (int i = 0; i < dgvSettings.Rows.Count; i++)
               {
                   var conn = new ConnectionInfo
                   {
                       HostName = dgvSettings.Rows[i].Cells["HostName"].Value?.ToString() ?? "",
                       UserName = dgvSettings.Rows[i].Cells["UserName"].Value?.ToString() ?? "",
                       Password = dgvSettings.Rows[i].Cells["Password"].Value?.ToString() ?? ""
                   };
                   _settings.ConnectionsInfo.Add(conn);
               }
           }

           private void btnOK_Click(object sender, EventArgs e)
           {
               SaveSettings();
               DialogResult = DialogResult.OK;
               Close();
           }

           private void btnCancel_Click(object sender, EventArgs e)
           {
               DialogResult = DialogResult.Cancel;
               Close();
           }


           private void btnApply_Click(object sender, EventArgs e)
           {
               Apply();
           }

           void Apply()
           {
               SaveSettings();
           }
       }
     */
}
