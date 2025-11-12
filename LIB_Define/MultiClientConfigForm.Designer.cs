namespace LIB_Define
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpClientCount = new System.Windows.Forms.GroupBox();
            this.numClientCount = new System.Windows.Forms.NumericUpDown();
            this.lblClientCount = new System.Windows.Forms.Label();
            this.dgvClients = new System.Windows.Forms.DataGridView();
            this.colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStoragePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConfigPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnApplyTemplate = new System.Windows.Forms.Button();
            this.btnEditSelected = new System.Windows.Forms.Button();
            this.btnEnableAll = new System.Windows.Forms.Button();
            this.btnDisableAll = new System.Windows.Forms.Button();
            this.grpClientCount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numClientCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClients)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(281, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Multi-Client Configuration Manager";
            // 
            // grpClientCount
            // 
            this.grpClientCount.Controls.Add(this.numClientCount);
            this.grpClientCount.Controls.Add(this.lblClientCount);
            this.grpClientCount.Location = new System.Drawing.Point(12, 35);
            this.grpClientCount.Name = "grpClientCount";
            this.grpClientCount.Size = new System.Drawing.Size(200, 60);
            this.grpClientCount.TabIndex = 1;
            this.grpClientCount.TabStop = false;
            this.grpClientCount.Text = "Client Count";
            // 
            // numClientCount
            // 
            this.numClientCount.Location = new System.Drawing.Point(115, 25);
            this.numClientCount.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numClientCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numClientCount.Name = "numClientCount";
            this.numClientCount.Size = new System.Drawing.Size(70, 23);
            this.numClientCount.TabIndex = 1;
            this.numClientCount.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numClientCount.ValueChanged += new System.EventHandler(this.numClientCount_ValueChanged);
            // 
            // lblClientCount
            // 
            this.lblClientCount.AutoSize = true;
            this.lblClientCount.Location = new System.Drawing.Point(15, 27);
            this.lblClientCount.Name = "lblClientCount";
            this.lblClientCount.Size = new System.Drawing.Size(94, 15);
            this.lblClientCount.TabIndex = 0;
            this.lblClientCount.Text = "Number of Clients:";
            // 
            // dgvClients
            // 
            this.dgvClients.AllowUserToAddRows = false;
            this.dgvClients.AllowUserToDeleteRows = false;
            this.dgvClients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvClients.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndex,
            this.colEnabled,
            this.colDisplayName,
            this.colHost,
            this.colPort,
            this.colLogPath,
            this.colStoragePath,
            this.colConfigPath});
            this.dgvClients.Location = new System.Drawing.Point(12, 101);
            this.dgvClients.MultiSelect = false;
            this.dgvClients.Name = "dgvClients";
            this.dgvClients.RowHeadersWidth = 25;
            this.dgvClients.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvClients.Size = new System.Drawing.Size(1160, 450);
            this.dgvClients.TabIndex = 2;
            this.dgvClients.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvClients_CellValueChanged);
            this.dgvClients.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvClients_CurrentCellDirtyStateChanged);
            // 
            // colIndex
            // 
            this.colIndex.HeaderText = "#";
            this.colIndex.Name = "colIndex";
            this.colIndex.ReadOnly = true;
            this.colIndex.Width = 40;
            // 
            // colEnabled
            // 
            this.colEnabled.HeaderText = "Enabled";
            this.colEnabled.Name = "colEnabled";
            this.colEnabled.Width = 60;
            // 
            // colDisplayName
            // 
            this.colDisplayName.HeaderText = "Display Name";
            this.colDisplayName.Name = "colDisplayName";
            this.colDisplayName.Width = 120;
            // 
            // colHost
            // 
            this.colHost.HeaderText = "Host";
            this.colHost.Name = "colHost";
            this.colHost.Width = 150;
            // 
            // colPort
            // 
            this.colPort.HeaderText = "Port";
            this.colPort.Name = "colPort";
            this.colPort.Width = 70;
            // 
            // colLogPath
            // 
            this.colLogPath.HeaderText = "Log Path";
            this.colLogPath.Name = "colLogPath";
            this.colLogPath.Width = 200;
            // 
            // colStoragePath
            // 
            this.colStoragePath.HeaderText = "Storage Path";
            this.colStoragePath.Name = "colStoragePath";
            this.colStoragePath.Width = 200;
            // 
            // colConfigPath
            // 
            this.colConfigPath.HeaderText = "Config Path";
            this.colConfigPath.Name = "colConfigPath";
            this.colConfigPath.Width = 250;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(1016, 562);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(1097, 562);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoad.Location = new System.Drawing.Point(12, 562);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(100, 30);
            this.btnLoad.TabIndex = 5;
            this.btnLoad.Text = "Load Config...";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnApplyTemplate
            // 
            this.btnApplyTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnApplyTemplate.Location = new System.Drawing.Point(118, 562);
            this.btnApplyTemplate.Name = "btnApplyTemplate";
            this.btnApplyTemplate.Size = new System.Drawing.Size(110, 30);
            this.btnApplyTemplate.TabIndex = 6;
            this.btnApplyTemplate.Text = "Apply Template...";
            this.btnApplyTemplate.UseVisualStyleBackColor = true;
            this.btnApplyTemplate.Click += new System.EventHandler(this.btnApplyTemplate_Click);
            // 
            // btnEditSelected
            // 
            this.btnEditSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditSelected.Location = new System.Drawing.Point(234, 562);
            this.btnEditSelected.Name = "btnEditSelected";
            this.btnEditSelected.Size = new System.Drawing.Size(100, 30);
            this.btnEditSelected.TabIndex = 7;
            this.btnEditSelected.Text = "Edit Selected...";
            this.btnEditSelected.UseVisualStyleBackColor = true;
            this.btnEditSelected.Click += new System.EventHandler(this.btnEditSelected_Click);
            // 
            // btnEnableAll
            // 
            this.btnEnableAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEnableAll.Location = new System.Drawing.Point(340, 562);
            this.btnEnableAll.Name = "btnEnableAll";
            this.btnEnableAll.Size = new System.Drawing.Size(80, 30);
            this.btnEnableAll.TabIndex = 8;
            this.btnEnableAll.Text = "Enable All";
            this.btnEnableAll.UseVisualStyleBackColor = true;
            this.btnEnableAll.Click += new System.EventHandler(this.btnEnableAll_Click);
            // 
            // btnDisableAll
            // 
            this.btnDisableAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDisableAll.Location = new System.Drawing.Point(426, 562);
            this.btnDisableAll.Name = "btnDisableAll";
            this.btnDisableAll.Size = new System.Drawing.Size(80, 30);
            this.btnDisableAll.TabIndex = 9;
            this.btnDisableAll.Text = "Disable All";
            this.btnDisableAll.UseVisualStyleBackColor = true;
            this.btnDisableAll.Click += new System.EventHandler(this.btnDisableAll_Click);
            // 
            // MultiClientConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 604);
            this.Controls.Add(this.btnDisableAll);
            this.Controls.Add(this.btnEnableAll);
            this.Controls.Add(this.btnEditSelected);
            this.Controls.Add(this.btnApplyTemplate);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.dgvClients);
            this.Controls.Add(this.grpClientCount);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new System.Drawing.Size(1200, 600);
            this.Name = "MultiClientConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Multi-Client Configuration Manager";
            this.grpClientCount.ResumeLayout(false);
            this.grpClientCount.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numClientCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClients)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpClientCount;
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
