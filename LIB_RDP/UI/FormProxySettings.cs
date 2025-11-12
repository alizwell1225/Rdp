using LIB_RDP.Core;
using LIB_RDP.Interfaces;

namespace LIB_RDP.UI
{
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

            //int count = 0;//_settings.RpcCount;

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

                //var conn = new ConnectionInfo
                //{
                //    HostName = dgvSettings.Rows[i].Cells["HostName"].Value?.ToString() ?? "",
                //    UserName = dgvSettings.Rows[i].Cells["UserName"].Value?.ToString() ?? "",
                //    Password = dgvSettings.Rows[i].Cells["Password"].Value?.ToString() ?? ""
                //};
                //_settings.ConnectionsInfo.Add(conn);
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
