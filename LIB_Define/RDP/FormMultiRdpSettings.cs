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
