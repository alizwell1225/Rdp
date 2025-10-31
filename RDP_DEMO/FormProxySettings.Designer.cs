namespace RDP_DEMO
{
    partial class FormProxySettings
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
            lblRpcCount = new System.Windows.Forms.Label();
            numRpcCount = new System.Windows.Forms.NumericUpDown();
            dgvSettings = new System.Windows.Forms.DataGridView();
            Index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            HostName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numRpcCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvSettings).BeginInit();
            SuspendLayout();
            // 
            // lblRpcCount
            // 
            lblRpcCount.AutoSize = true;
            lblRpcCount.Location = new System.Drawing.Point(20, 20);
            lblRpcCount.Name = "lblRpcCount";
            lblRpcCount.Size = new System.Drawing.Size(89, 15);
            lblRpcCount.TabIndex = 0;
            lblRpcCount.Text = "RPC 控制數量：";
            // 
            // numRpcCount
            // 
            numRpcCount.Location = new System.Drawing.Point(120, 18);
            numRpcCount.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numRpcCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRpcCount.Name = "numRpcCount";
            numRpcCount.Size = new System.Drawing.Size(100, 23);
            numRpcCount.TabIndex = 1;
            numRpcCount.Value = new decimal(new int[] { 12, 0, 0, 0 });
            numRpcCount.ValueChanged += numRpcCount_ValueChanged;
            // 
            // dgvSettings
            // 
            dgvSettings.AllowUserToAddRows = false;
            dgvSettings.AllowUserToDeleteRows = false;
            dgvSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSettings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { Index, HostName, UserName, Password });
            dgvSettings.Location = new System.Drawing.Point(20, 60);
            dgvSettings.Name = "dgvSettings";
            dgvSettings.RowHeadersVisible = false;
            dgvSettings.RowTemplate.Height = 30;
            dgvSettings.Size = new System.Drawing.Size(760, 460);
            dgvSettings.TabIndex = 2;
            // 
            // Index
            // 
            Index.HeaderText = "No";
            Index.Name = "Index";
            Index.ReadOnly = true;
            Index.Width = 60;
            // 
            // HostName
            // 
            HostName.HeaderText = "主機位址";
            HostName.Name = "HostName";
            HostName.Width = 250;
            // 
            // UserName
            // 
            UserName.HeaderText = "使用者名稱";
            UserName.Name = "UserName";
            UserName.Width = 200;
            // 
            // Password
            // 
            Password.HeaderText = "密碼";
            Password.Name = "Password";
            Password.Width = 200;
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(580, 540);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(90, 30);
            btnOK.TabIndex = 3;
            btnOK.Text = "確定";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(690, 540);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(90, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // FormProxySettings
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 590);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(dgvSettings);
            Controls.Add(numRpcCount);
            Controls.Add(lblRpcCount);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormProxySettings";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "RPC 連線設定";
            ((System.ComponentModel.ISupportInitialize)numRpcCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvSettings).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblRpcCount;
        private System.Windows.Forms.NumericUpDown numRpcCount;
        private System.Windows.Forms.DataGridView dgvSettings;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Index;
        private System.Windows.Forms.DataGridViewTextBoxColumn HostName;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Password;
    }
}
