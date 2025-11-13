namespace LIB_RDP.UI
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
            dgvSettings = new DataGridView();
            Index = new DataGridViewTextBoxColumn();
            HostName = new DataGridViewTextBoxColumn();
            UserName = new DataGridViewTextBoxColumn();
            Password = new DataGridViewTextBoxColumn();
            btnOK = new Button();
            btnCancel = new Button();
            btnApply = new Button();
            btnApplyTemplate = new Button();
            chkUseJsonFormat = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dgvSettings).BeginInit();
            SuspendLayout();
            // 
            // dgvSettings
            // 
            dgvSettings.AllowUserToAddRows = false;
            dgvSettings.AllowUserToDeleteRows = false;
            dgvSettings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSettings.Columns.AddRange(new DataGridViewColumn[] { Index, HostName, UserName, Password });
            dgvSettings.Location = new Point(20, 24);
            dgvSettings.Name = "dgvSettings";
            dgvSettings.RowHeadersVisible = false;
            dgvSettings.RowTemplate.Height = 30;
            dgvSettings.Size = new Size(760, 460);
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
            Password.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Password.HeaderText = "密碼";
            Password.Name = "Password";
            // 
            // btnOK
            // 
            btnOK.Location = new Point(440, 503);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 3;
            btnOK.Text = "確定";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(550, 503);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(690, 503);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(90, 30);
            btnApply.TabIndex = 3;
            btnApply.Text = "套用";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // btnApplyTemplate
            // 
            btnApplyTemplate.Location = new Point(20, 503);
            btnApplyTemplate.Name = "btnApplyTemplate";
            btnApplyTemplate.Size = new Size(120, 30);
            btnApplyTemplate.TabIndex = 5;
            btnApplyTemplate.Text = "批次設定...";
            btnApplyTemplate.UseVisualStyleBackColor = true;
            btnApplyTemplate.Click += btnApplyTemplate_Click;
            // 
            // chkUseJsonFormat
            // 
            chkUseJsonFormat.AutoSize = true;
            chkUseJsonFormat.Location = new Point(160, 510);
            chkUseJsonFormat.Name = "chkUseJsonFormat";
            chkUseJsonFormat.Size = new Size(130, 19);
            chkUseJsonFormat.TabIndex = 6;
            chkUseJsonFormat.Text = "使用 JSON 格式儲存";
            chkUseJsonFormat.UseVisualStyleBackColor = true;
            chkUseJsonFormat.CheckedChanged += chkUseJsonFormat_CheckedChanged;
            // 
            // FormProxySettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 545);
            Controls.Add(chkUseJsonFormat);
            Controls.Add(btnApplyTemplate);
            Controls.Add(btnCancel);
            Controls.Add(btnApply);
            Controls.Add(btnOK);
            Controls.Add(dgvSettings);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormProxySettings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "RPC 連線設定";
            ((System.ComponentModel.ISupportInitialize)dgvSettings).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.DataGridView dgvSettings;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private DataGridViewTextBoxColumn Index;
        private DataGridViewTextBoxColumn HostName;
        private DataGridViewTextBoxColumn UserName;
        private DataGridViewTextBoxColumn Password;
        private Button btnApply;
        private Button btnApplyTemplate;
        private CheckBox chkUseJsonFormat;
    }
}
