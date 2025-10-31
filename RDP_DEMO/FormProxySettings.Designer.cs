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
            this.lblRdpCount = new System.Windows.Forms.Label();
            this.numRdpCount = new System.Windows.Forms.NumericUpDown();
            this.dgvSettings = new System.Windows.Forms.DataGridView();
            this.colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPassword = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numRdpCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSettings)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRdpCount
            // 
            this.lblRdpCount.AutoSize = true;
            this.lblRdpCount.Location = new System.Drawing.Point(12, 15);
            this.lblRdpCount.Name = "lblRdpCount";
            this.lblRdpCount.Size = new System.Drawing.Size(103, 15);
            this.lblRdpCount.TabIndex = 0;
            this.lblRdpCount.Text = "RDP 控制項數量：";
            // 
            // numRdpCount
            // 
            this.numRdpCount.Location = new System.Drawing.Point(121, 13);
            this.numRdpCount.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numRdpCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRdpCount.Name = "numRdpCount";
            this.numRdpCount.Size = new System.Drawing.Size(80, 23);
            this.numRdpCount.TabIndex = 1;
            this.numRdpCount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numRdpCount.ValueChanged += new System.EventHandler(this.numRdpCount_ValueChanged);
            // 
            // dgvSettings
            // 
            this.dgvSettings.AllowUserToAddRows = false;
            this.dgvSettings.AllowUserToDeleteRows = false;
            this.dgvSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSettings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSettings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndex,
            this.colHost,
            this.colUser,
            this.colPassword});
            this.dgvSettings.Location = new System.Drawing.Point(12, 50);
            this.dgvSettings.Name = "dgvSettings";
            this.dgvSettings.RowHeadersVisible = false;
            this.dgvSettings.RowTemplate.Height = 30;
            this.dgvSettings.Size = new System.Drawing.Size(760, 350);
            this.dgvSettings.TabIndex = 2;
            // 
            // colIndex
            // 
            this.colIndex.FillWeight = 50F;
            this.colIndex.HeaderText = "編號";
            this.colIndex.Name = "colIndex";
            this.colIndex.ReadOnly = true;
            // 
            // colHost
            // 
            this.colHost.HeaderText = "主機名稱/IP";
            this.colHost.Name = "colHost";
            // 
            // colUser
            // 
            this.colUser.HeaderText = "使用者名稱";
            this.colUser.Name = "colUser";
            // 
            // colPassword
            // 
            this.colPassword.HeaderText = "密碼";
            this.colPassword.Name = "colPassword";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(576, 415);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 35);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "確定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(682, 415);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormProxySettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.dgvSettings);
            this.Controls.Add(this.numRdpCount);
            this.Controls.Add(this.lblRdpCount);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProxySettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Proxy 設定";
            this.Load += new System.EventHandler(this.FormProxySettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numRdpCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSettings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRdpCount;
        private System.Windows.Forms.NumericUpDown numRdpCount;
        private System.Windows.Forms.DataGridView dgvSettings;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHost;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPassword;
    }
}
