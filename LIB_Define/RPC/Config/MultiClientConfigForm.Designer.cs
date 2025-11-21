namespace LIB_Define.RPC.Config
{
    partial class MultiClientConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            numClientCount = new NumericUpDown();
            lblClientCount = new Label();
            dgvClients = new DataGridView();
            colIndex = new DataGridViewTextBoxColumn();
            colEnabled = new DataGridViewCheckBoxColumn();
            colDisplayName = new DataGridViewTextBoxColumn();
            colHost = new DataGridViewTextBoxColumn();
            colPort = new DataGridViewTextBoxColumn();
            colLogPath = new DataGridViewTextBoxColumn();
            colStoragePath = new DataGridViewTextBoxColumn();
            colConfigPath = new DataGridViewTextBoxColumn();
            btnSave = new Button();
            btnCancel = new Button();
            btnLoad = new Button();
            btnApplyTemplate = new Button();
            btnEditSelected = new Button();
            btnEnableAll = new Button();
            btnDisableAll = new Button();
            btnImportRdpIPs = new Button();
            ((System.ComponentModel.ISupportInitialize)numClientCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvClients).BeginInit();
            SuspendLayout();
            // 
            // numClientCount
            // 
            numClientCount.Location = new Point(835, 413);
            numClientCount.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numClientCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numClientCount.Name = "numClientCount";
            numClientCount.Size = new Size(70, 23);
            numClientCount.TabIndex = 1;
            numClientCount.Value = new decimal(new int[] { 12, 0, 0, 0 });
            numClientCount.Visible = false;
            numClientCount.ValueChanged += numClientCount_ValueChanged;
            // 
            // lblClientCount
            // 
            lblClientCount.AutoSize = true;
            lblClientCount.Location = new Point(717, 418);
            lblClientCount.Name = "lblClientCount";
            lblClientCount.Size = new Size(112, 15);
            lblClientCount.TabIndex = 0;
            lblClientCount.Text = "Number of Clients:";
            lblClientCount.Visible = false;
            // 
            // dgvClients
            // 
            dgvClients.AllowUserToAddRows = false;
            dgvClients.AllowUserToDeleteRows = false;
            dgvClients.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvClients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClients.Columns.AddRange(new DataGridViewColumn[] { colIndex, colEnabled, colDisplayName, colHost, colPort, colLogPath, colStoragePath, colConfigPath });
            dgvClients.Location = new Point(12, 12);
            dgvClients.MultiSelect = false;
            dgvClients.Name = "dgvClients";
            dgvClients.RowHeadersWidth = 25;
            dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClients.Size = new Size(1119, 392);
            dgvClients.TabIndex = 2;
            dgvClients.CellDoubleClick += dgvClients_CellDoubleClick;
            dgvClients.CellValueChanged += dgvClients_CellValueChanged;
            dgvClients.CurrentCellDirtyStateChanged += dgvClients_CurrentCellDirtyStateChanged;
            // 
            // colIndex
            // 
            colIndex.HeaderText = "#";
            colIndex.Name = "colIndex";
            colIndex.ReadOnly = true;
            colIndex.Width = 40;
            // 
            // colEnabled
            // 
            colEnabled.HeaderText = "Enabled";
            colEnabled.Name = "colEnabled";
            colEnabled.Width = 60;
            // 
            // colDisplayName
            // 
            colDisplayName.HeaderText = "Display Name";
            colDisplayName.Name = "colDisplayName";
            colDisplayName.Width = 120;
            // 
            // colHost
            // 
            colHost.HeaderText = "Host";
            colHost.Name = "colHost";
            colHost.Width = 150;
            // 
            // colPort
            // 
            colPort.HeaderText = "Port";
            colPort.Name = "colPort";
            colPort.Width = 70;
            // 
            // colLogPath
            // 
            colLogPath.HeaderText = "Log Path";
            colLogPath.Name = "colLogPath";
            colLogPath.Width = 200;
            // 
            // colStoragePath
            // 
            colStoragePath.HeaderText = "Storage Path";
            colStoragePath.Name = "colStoragePath";
            colStoragePath.Width = 200;
            // 
            // colConfigPath
            // 
            colConfigPath.HeaderText = "Config Path";
            colConfigPath.Name = "colConfigPath";
            colConfigPath.Width = 250;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Location = new Point(945, 410);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 30);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(1026, 410);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLoad.Location = new Point(12, 410);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(100, 30);
            btnLoad.TabIndex = 5;
            btnLoad.Text = "Load Config...";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnApplyTemplate
            // 
            btnApplyTemplate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnApplyTemplate.Location = new Point(118, 410);
            btnApplyTemplate.Name = "btnApplyTemplate";
            btnApplyTemplate.Size = new Size(133, 30);
            btnApplyTemplate.TabIndex = 6;
            btnApplyTemplate.Text = "Apply Template...";
            btnApplyTemplate.UseVisualStyleBackColor = true;
            btnApplyTemplate.Click += btnApplyTemplate_Click;
            // 
            // btnEditSelected
            // 
            btnEditSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEditSelected.Location = new Point(285, 410);
            btnEditSelected.Name = "btnEditSelected";
            btnEditSelected.Size = new Size(100, 30);
            btnEditSelected.TabIndex = 7;
            btnEditSelected.Text = "Edit Selected...";
            btnEditSelected.UseVisualStyleBackColor = true;
            btnEditSelected.Click += btnEditSelected_Click;
            // 
            // btnEnableAll
            // 
            btnEnableAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEnableAll.Location = new Point(391, 410);
            btnEnableAll.Name = "btnEnableAll";
            btnEnableAll.Size = new Size(80, 30);
            btnEnableAll.TabIndex = 8;
            btnEnableAll.Text = "Enable All";
            btnEnableAll.UseVisualStyleBackColor = true;
            btnEnableAll.Click += btnEnableAll_Click;
            // 
            // btnDisableAll
            // 
            btnDisableAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDisableAll.Location = new Point(477, 410);
            btnDisableAll.Name = "btnDisableAll";
            btnDisableAll.Size = new Size(80, 30);
            btnDisableAll.TabIndex = 9;
            btnDisableAll.Text = "Disable All";
            btnDisableAll.UseVisualStyleBackColor = true;
            btnDisableAll.Click += btnDisableAll_Click;
            // 
            // btnImportRdpIPs
            // 
            btnImportRdpIPs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnImportRdpIPs.Location = new Point(563, 410);
            btnImportRdpIPs.Name = "btnImportRdpIPs";
            btnImportRdpIPs.Size = new Size(120, 30);
            btnImportRdpIPs.TabIndex = 10;
            btnImportRdpIPs.Text = "Import RDP IPs";
            btnImportRdpIPs.UseVisualStyleBackColor = true;
            btnImportRdpIPs.Click += btnImportRdpIPs_Click;
            // 
            // MultiClientConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1154, 452);
            ControlBox = false;
            Controls.Add(btnImportRdpIPs);
            Controls.Add(numClientCount);
            Controls.Add(btnDisableAll);
            Controls.Add(lblClientCount);
            Controls.Add(btnEnableAll);
            Controls.Add(btnEditSelected);
            Controls.Add(btnApplyTemplate);
            Controls.Add(btnLoad);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(dgvClients);
            Name = "MultiClientConfigForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Configuration Manager";
            ((System.ComponentModel.ISupportInitialize)numClientCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvClients).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NumericUpDown numClientCount;
        private System.Windows.Forms.Label lblClientCount;
        private System.Windows.Forms.DataGridView dgvClients;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnApplyTemplate;
        private System.Windows.Forms.Button btnEditSelected;
        private System.Windows.Forms.Button btnEnableAll;
        private System.Windows.Forms.Button btnDisableAll;
        private System.Windows.Forms.Button btnImportRdpIPs;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisplayName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHost;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPort;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStoragePath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConfigPath;
    }
}
