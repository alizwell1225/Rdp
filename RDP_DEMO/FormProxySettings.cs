using System;
using System.Windows.Forms;

namespace RDP_DEMO
{
    public partial class FormProxySettings : Form
    {
        private readonly ProxySettings _settings;

        public FormProxySettings(ProxySettings settings)
        {
            InitializeComponent();
            _settings = settings;
            LoadSettings();
        }

        private void LoadSettings()
        {
            numRpcCount.Value = _settings.RpcCount;
            UpdateConnectionGrid();
            
            for (int i = 0; i < _settings.Connections.Count && i < dgvSettings.Rows.Count; i++)
            {
                var conn = _settings.Connections[i];
                dgvSettings.Rows[i].Cells["HostName"].Value = conn.HostName;
                dgvSettings.Rows[i].Cells["UserName"].Value = conn.UserName;
                dgvSettings.Rows[i].Cells["Password"].Value = conn.Password;
            }
        }

        private void UpdateConnectionGrid()
        {
            int count = (int)numRpcCount.Value;
            
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
            _settings.RpcCount = (int)numRpcCount.Value;
            _settings.Connections.Clear();
            
            for (int i = 0; i < dgvSettings.Rows.Count; i++)
            {
                var conn = new ConnectionInfo
                {
                    HostName = dgvSettings.Rows[i].Cells["HostName"].Value?.ToString() ?? "",
                    UserName = dgvSettings.Rows[i].Cells["UserName"].Value?.ToString() ?? "",
                    Password = dgvSettings.Rows[i].Cells["Password"].Value?.ToString() ?? ""
                };
                _settings.Connections.Add(conn);
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

        private void numRpcCount_ValueChanged(object sender, EventArgs e)
        {
            UpdateConnectionGrid();
        }
    }
}
