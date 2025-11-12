namespace LIB_Define
{
    partial class GrpcConfigForm
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
            this.lblMode = new System.Windows.Forms.Label();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblHost = new System.Windows.Forms.Label();
            this.grpPaths = new System.Windows.Forms.GroupBox();
            this.btnBrowseStorageRoot = new System.Windows.Forms.Button();
            this.txtStorageRoot = new System.Windows.Forms.TextBox();
            this.lblStorageRoot = new System.Windows.Forms.Label();
            this.btnBrowseLogPath = new System.Windows.Forms.Button();
            this.txtLogFilePath = new System.Windows.Forms.TextBox();
            this.lblLogFilePath = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnResetDefaults = new System.Windows.Forms.Button();
            this.grpConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpPaths.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMode.Location = new System.Drawing.Point(12, 9);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(101, 17);
            this.lblMode.TabIndex = 0;
            this.lblMode.Text = "Mode: Client";
            // 
            // grpConnection
            // 
            this.grpConnection.Controls.Add(this.numPort);
            this.grpConnection.Controls.Add(this.txtHost);
            this.grpConnection.Controls.Add(this.lblPort);
            this.grpConnection.Controls.Add(this.lblHost);
            this.grpConnection.Location = new System.Drawing.Point(12, 35);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Size = new System.Drawing.Size(460, 90);
            this.grpConnection.TabIndex = 1;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Connection Settings";
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(80, 55);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(120, 23);
            this.numPort.TabIndex = 3;
            this.numPort.Value = new decimal(new int[] {
            50051,
            0,
            0,
            0});
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(80, 25);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(360, 23);
            this.txtHost.TabIndex = 1;
            this.txtHost.Text = "localhost";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(15, 57);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(32, 15);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Port:";
            // 
            // lblHost
            // 
            this.lblHost.AutoSize = true;
            this.lblHost.Location = new System.Drawing.Point(15, 28);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(35, 15);
            this.lblHost.TabIndex = 0;
            this.lblHost.Text = "Host:";
            // 
            // grpPaths
            // 
            this.grpPaths.Controls.Add(this.btnBrowseStorageRoot);
            this.grpPaths.Controls.Add(this.txtStorageRoot);
            this.grpPaths.Controls.Add(this.lblStorageRoot);
            this.grpPaths.Controls.Add(this.btnBrowseLogPath);
            this.grpPaths.Controls.Add(this.txtLogFilePath);
            this.grpPaths.Controls.Add(this.lblLogFilePath);
            this.grpPaths.Location = new System.Drawing.Point(12, 131);
            this.grpPaths.Name = "grpPaths";
            this.grpPaths.Size = new System.Drawing.Size(460, 110);
            this.grpPaths.TabIndex = 2;
            this.grpPaths.TabStop = false;
            this.grpPaths.Text = "File Paths";
            // 
            // btnBrowseStorageRoot
            // 
            this.btnBrowseStorageRoot.Location = new System.Drawing.Point(410, 70);
            this.btnBrowseStorageRoot.Name = "btnBrowseStorageRoot";
            this.btnBrowseStorageRoot.Size = new System.Drawing.Size(30, 23);
            this.btnBrowseStorageRoot.TabIndex = 5;
            this.btnBrowseStorageRoot.Text = "...";
            this.btnBrowseStorageRoot.UseVisualStyleBackColor = true;
            this.btnBrowseStorageRoot.Click += new System.EventHandler(this.btnBrowseStorageRoot_Click);
            // 
            // txtStorageRoot
            // 
            this.txtStorageRoot.Location = new System.Drawing.Point(15, 70);
            this.txtStorageRoot.Name = "txtStorageRoot";
            this.txtStorageRoot.Size = new System.Drawing.Size(389, 23);
            this.txtStorageRoot.TabIndex = 4;
            // 
            // lblStorageRoot
            // 
            this.lblStorageRoot.AutoSize = true;
            this.lblStorageRoot.Location = new System.Drawing.Point(15, 52);
            this.lblStorageRoot.Name = "lblStorageRoot";
            this.lblStorageRoot.Size = new System.Drawing.Size(110, 15);
            this.lblStorageRoot.TabIndex = 3;
            this.lblStorageRoot.Text = "Storage Root / Path:";
            // 
            // btnBrowseLogPath
            // 
            this.btnBrowseLogPath.Location = new System.Drawing.Point(410, 25);
            this.btnBrowseLogPath.Name = "btnBrowseLogPath";
            this.btnBrowseLogPath.Size = new System.Drawing.Size(30, 23);
            this.btnBrowseLogPath.TabIndex = 2;
            this.btnBrowseLogPath.Text = "...";
            this.btnBrowseLogPath.UseVisualStyleBackColor = true;
            this.btnBrowseLogPath.Click += new System.EventHandler(this.btnBrowseLogPath_Click);
            // 
            // txtLogFilePath
            // 
            this.txtLogFilePath.Location = new System.Drawing.Point(15, 25);
            this.txtLogFilePath.Name = "txtLogFilePath";
            this.txtLogFilePath.Size = new System.Drawing.Size(389, 23);
            this.txtLogFilePath.TabIndex = 1;
            // 
            // lblLogFilePath
            // 
            this.lblLogFilePath.AutoSize = true;
            this.lblLogFilePath.Location = new System.Drawing.Point(15, 7);
            this.lblLogFilePath.Name = "lblLogFilePath";
            this.lblLogFilePath.Size = new System.Drawing.Size(79, 15);
            this.lblLogFilePath.TabIndex = 0;
            this.lblLogFilePath.Text = "Log File Path:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(316, 256);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(397, 256);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 256);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(100, 30);
            this.btnLoad.TabIndex = 5;
            this.btnLoad.Text = "Load Config...";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnResetDefaults
            // 
            this.btnResetDefaults.Location = new System.Drawing.Point(118, 256);
            this.btnResetDefaults.Name = "btnResetDefaults";
            this.btnResetDefaults.Size = new System.Drawing.Size(100, 30);
            this.btnResetDefaults.TabIndex = 6;
            this.btnResetDefaults.Text = "Reset Defaults";
            this.btnResetDefaults.UseVisualStyleBackColor = true;
            this.btnResetDefaults.Click += new System.EventHandler(this.btnResetDefaults_Click);
            // 
            // GrpcConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 298);
            this.Controls.Add(this.btnResetDefaults);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpPaths);
            this.Controls.Add(this.grpConnection);
            this.Controls.Add(this.lblMode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GrpcConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "gRPC Configuration";
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpPaths.ResumeLayout(false);
            this.grpPaths.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.GroupBox grpPaths;
        private System.Windows.Forms.Button btnBrowseStorageRoot;
        private System.Windows.Forms.TextBox txtStorageRoot;
        private System.Windows.Forms.Label lblStorageRoot;
        private System.Windows.Forms.Button btnBrowseLogPath;
        private System.Windows.Forms.TextBox txtLogFilePath;
        private System.Windows.Forms.Label lblLogFilePath;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnResetDefaults;
    }
}
