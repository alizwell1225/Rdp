namespace LIB_Define.RPC
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
            lblMode = new Label();
            grpConnection = new GroupBox();
            numPort = new NumericUpDown();
            txtHost = new TextBox();
            lblPort = new Label();
            lblHost = new Label();
            grpPaths = new GroupBox();
            btnBrowseStorageRoot = new Button();
            txtStorageRoot = new TextBox();
            lblStorageRoot = new Label();
            btnBrowseLogPath = new Button();
            txtLogFilePath = new TextBox();
            lblLogFilePath = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            btnLoad = new Button();
            btnResetDefaults = new Button();
            grpConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            grpPaths.SuspendLayout();
            SuspendLayout();
            // 
            // lblMode
            // 
            lblMode.AutoSize = true;
            lblMode.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            lblMode.Location = new Point(12, 9);
            lblMode.Name = "lblMode";
            lblMode.Size = new Size(98, 17);
            lblMode.TabIndex = 0;
            lblMode.Text = "Mode: Client";
            // 
            // grpConnection
            // 
            grpConnection.Controls.Add(numPort);
            grpConnection.Controls.Add(txtHost);
            grpConnection.Controls.Add(lblPort);
            grpConnection.Controls.Add(lblHost);
            grpConnection.Location = new Point(12, 35);
            grpConnection.Name = "grpConnection";
            grpConnection.Size = new Size(460, 90);
            grpConnection.TabIndex = 1;
            grpConnection.TabStop = false;
            grpConnection.Text = "Connection Settings";
            // 
            // numPort
            // 
            numPort.Location = new Point(80, 55);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(120, 23);
            numPort.TabIndex = 3;
            numPort.Value = new decimal(new int[] { 50051, 0, 0, 0 });
            // 
            // txtHost
            // 
            txtHost.Location = new Point(80, 25);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(360, 23);
            txtHost.TabIndex = 1;
            txtHost.Text = "localhost";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(15, 57);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(33, 15);
            lblPort.TabIndex = 2;
            lblPort.Text = "Port:";
            // 
            // lblHost
            // 
            lblHost.AutoSize = true;
            lblHost.Location = new Point(15, 28);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(36, 15);
            lblHost.TabIndex = 0;
            lblHost.Text = "Host:";
            // 
            // grpPaths
            // 
            grpPaths.Controls.Add(btnBrowseStorageRoot);
            grpPaths.Controls.Add(txtStorageRoot);
            grpPaths.Controls.Add(lblStorageRoot);
            grpPaths.Controls.Add(btnBrowseLogPath);
            grpPaths.Controls.Add(txtLogFilePath);
            grpPaths.Controls.Add(lblLogFilePath);
            grpPaths.Location = new Point(12, 131);
            grpPaths.Name = "grpPaths";
            grpPaths.Size = new Size(460, 123);
            grpPaths.TabIndex = 2;
            grpPaths.TabStop = false;
            grpPaths.Text = "File Paths";
            // 
            // btnBrowseStorageRoot
            // 
            btnBrowseStorageRoot.Location = new Point(410, 87);
            btnBrowseStorageRoot.Name = "btnBrowseStorageRoot";
            btnBrowseStorageRoot.Size = new Size(30, 23);
            btnBrowseStorageRoot.TabIndex = 5;
            btnBrowseStorageRoot.Text = "...";
            btnBrowseStorageRoot.UseVisualStyleBackColor = true;
            btnBrowseStorageRoot.Click += btnBrowseStorageRoot_Click;
            // 
            // txtStorageRoot
            // 
            txtStorageRoot.Location = new Point(15, 87);
            txtStorageRoot.Name = "txtStorageRoot";
            txtStorageRoot.Size = new Size(389, 23);
            txtStorageRoot.TabIndex = 4;
            // 
            // lblStorageRoot
            // 
            lblStorageRoot.AutoSize = true;
            lblStorageRoot.Location = new Point(15, 69);
            lblStorageRoot.Name = "lblStorageRoot";
            lblStorageRoot.Size = new Size(122, 15);
            lblStorageRoot.TabIndex = 3;
            lblStorageRoot.Text = "Storage Root / Path:";
            // 
            // btnBrowseLogPath
            // 
            btnBrowseLogPath.Location = new Point(410, 42);
            btnBrowseLogPath.Name = "btnBrowseLogPath";
            btnBrowseLogPath.Size = new Size(30, 23);
            btnBrowseLogPath.TabIndex = 2;
            btnBrowseLogPath.Text = "...";
            btnBrowseLogPath.UseVisualStyleBackColor = true;
            btnBrowseLogPath.Click += btnBrowseLogPath_Click;
            // 
            // txtLogFilePath
            // 
            txtLogFilePath.Location = new Point(15, 42);
            txtLogFilePath.Name = "txtLogFilePath";
            txtLogFilePath.Size = new Size(389, 23);
            txtLogFilePath.TabIndex = 1;
            // 
            // lblLogFilePath
            // 
            lblLogFilePath.AutoSize = true;
            lblLogFilePath.Location = new Point(15, 24);
            lblLogFilePath.Name = "lblLogFilePath";
            lblLogFilePath.Size = new Size(82, 15);
            lblLogFilePath.TabIndex = 0;
            lblLogFilePath.Text = "Log File Path:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(316, 260);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 30);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(397, 260);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(12, 260);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(100, 30);
            btnLoad.TabIndex = 5;
            btnLoad.Text = "Load Config...";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnResetDefaults
            // 
            btnResetDefaults.Location = new Point(118, 260);
            btnResetDefaults.Name = "btnResetDefaults";
            btnResetDefaults.Size = new Size(100, 30);
            btnResetDefaults.TabIndex = 6;
            btnResetDefaults.Text = "Reset Defaults";
            btnResetDefaults.UseVisualStyleBackColor = true;
            btnResetDefaults.Click += btnResetDefaults_Click;
            // 
            // GrpcConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 303);
            Controls.Add(btnResetDefaults);
            Controls.Add(btnLoad);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(grpPaths);
            Controls.Add(grpConnection);
            Controls.Add(lblMode);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GrpcConfigForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "gRPC Configuration";
            grpConnection.ResumeLayout(false);
            grpConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            grpPaths.ResumeLayout(false);
            grpPaths.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

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
