using System;
using System.Windows.Forms;

namespace RDP_DEMO
{
    public partial class FormProxySettings : Form
    {
        public int NumberOfConnections { get; private set; }
        public string[] HostNames { get; private set; }
        public string[] UserNames { get; private set; }
        public string[] Passwords { get; private set; }

        public FormProxySettings()
        {
            InitializeComponent();
            NumberOfConnections = 2; // 預設2個連線
        }

        public FormProxySettings(int currentConnections) : this()
        {
            NumberOfConnections = currentConnections;
            numRdpCount.Value = currentConnections;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            NumberOfConnections = (int)numRdpCount.Value;
            
            // 收集所有設定的連線資訊
            HostNames = new string[NumberOfConnections];
            UserNames = new string[NumberOfConnections];
            Passwords = new string[NumberOfConnections];

            for (int i = 0; i < dgvSettings.Rows.Count && i < NumberOfConnections; i++)
            {
                var row = dgvSettings.Rows[i];
                HostNames[i] = row.Cells["colHost"].Value?.ToString() ?? "";
                UserNames[i] = row.Cells["colUser"].Value?.ToString() ?? "";
                Passwords[i] = row.Cells["colPassword"].Value?.ToString() ?? "";
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void numRdpCount_ValueChanged(object sender, EventArgs e)
        {
            UpdateDataGridRows();
        }

        private void UpdateDataGridRows()
        {
            int targetCount = (int)numRdpCount.Value;
            
            while (dgvSettings.Rows.Count < targetCount)
            {
                int index = dgvSettings.Rows.Count;
                dgvSettings.Rows.Add(index + 1, "", "", "");
            }

            while (dgvSettings.Rows.Count > targetCount)
            {
                dgvSettings.Rows.RemoveAt(dgvSettings.Rows.Count - 1);
            }
        }

        private void FormProxySettings_Load(object sender, EventArgs e)
        {
            UpdateDataGridRows();
        }

        public void LoadSettings(System.Windows.Forms.DataGridView sourceGrid, int connectionCount)
        {
            numRdpCount.Value = connectionCount;
            NumberOfConnections = connectionCount;
            
            for (int i = 0; i < sourceGrid.Rows.Count && i < connectionCount; i++)
            {
                if (i < dgvSettings.Rows.Count)
                {
                    var row = dgvSettings.Rows[i];
                    row.Cells["colHost"].Value = sourceGrid.Rows[i].Cells["HostName"].Value;
                    row.Cells["colUser"].Value = sourceGrid.Rows[i].Cells["UserName"].Value;
                    row.Cells["colPassword"].Value = sourceGrid.Rows[i].Cells["Password"].Value;
                }
            }
        }

        public void ApplySettingsToGrid(System.Windows.Forms.DataGridView targetGrid, System.Windows.Forms.NumericUpDown numConnections)
        {
            numConnections.Value = NumberOfConnections;
            
            for (int i = 0; i < NumberOfConnections && i < targetGrid.Rows.Count; i++)
            {
                targetGrid.Rows[i].Cells["HostName"].Value = HostNames[i];
                targetGrid.Rows[i].Cells["UserName"].Value = UserNames[i];
                targetGrid.Rows[i].Cells["Password"].Value = Passwords[i];
            }
        }
    }
}
