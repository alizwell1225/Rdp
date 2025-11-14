namespace TestGrpcServerApp
{
    partial class ServerConfigForm
    {
        private System.ComponentModel.IContainer components = null;
        
        private Label lblHost;
        private Label lblPort;
        private Label lblStoragePath;
        private Label lblLogPath;
        private TextBox txtHost;
        private TextBox txtPort;
        private TextBox txtStoragePath;
        private TextBox txtLogPath;
        private Button btnBrowseStorage;
        private Button btnBrowseLog;
        private Button btnOK;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblHost = new Label();
            this.lblPort = new Label();
            this.lblStoragePath = new Label();
            this.lblLogPath = new Label();
            this.txtHost = new TextBox();
            this.txtPort = new TextBox();
            this.txtStoragePath = new TextBox();
            this.txtLogPath = new TextBox();
            this.btnBrowseStorage = new Button();
            this.btnBrowseLog = new Button();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            
            // lblHost
            this.lblHost.AutoSize = true;
            this.lblHost.Location = new Point(20, 20);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new Size(35, 15);
            this.lblHost.TabIndex = 0;
            this.lblHost.Text = "Host:";
            
            // txtHost
            this.txtHost.Location = new Point(120, 17);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new Size(300, 23);
            this.txtHost.TabIndex = 1;
            
            // lblPort
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new Point(20, 55);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new Size(32, 15);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Port:";
            
            // txtPort
            this.txtPort.Location = new Point(120, 52);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new Size(100, 23);
            this.txtPort.TabIndex = 3;
            
            // lblStoragePath
            this.lblStoragePath.AutoSize = true;
            this.lblStoragePath.Location = new Point(20, 90);
            this.lblStoragePath.Name = "lblStoragePath";
            this.lblStoragePath.Size = new Size(80, 15);
            this.lblStoragePath.TabIndex = 4;
            this.lblStoragePath.Text = "Storage Path:";
            
            // txtStoragePath
            this.txtStoragePath.Location = new Point(120, 87);
            this.txtStoragePath.Name = "txtStoragePath";
            this.txtStoragePath.Size = new Size(250, 23);
            this.txtStoragePath.TabIndex = 5;
            
            // btnBrowseStorage
            this.btnBrowseStorage.Location = new Point(380, 86);
            this.btnBrowseStorage.Name = "btnBrowseStorage";
            this.btnBrowseStorage.Size = new Size(40, 25);
            this.btnBrowseStorage.TabIndex = 6;
            this.btnBrowseStorage.Text = "...";
            this.btnBrowseStorage.UseVisualStyleBackColor = true;
            this.btnBrowseStorage.Click += new EventHandler(this.btnBrowseStorage_Click);
            
            // lblLogPath
            this.lblLogPath.AutoSize = true;
            this.lblLogPath.Location = new Point(20, 125);
            this.lblLogPath.Name = "lblLogPath";
            this.lblLogPath.Size = new Size(59, 15);
            this.lblLogPath.TabIndex = 7;
            this.lblLogPath.Text = "Log Path:";
            
            // txtLogPath
            this.txtLogPath.Location = new Point(120, 122);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new Size(250, 23);
            this.txtLogPath.TabIndex = 8;
            
            // btnBrowseLog
            this.btnBrowseLog.Location = new Point(380, 121);
            this.btnBrowseLog.Name = "btnBrowseLog";
            this.btnBrowseLog.Size = new Size(40, 25);
            this.btnBrowseLog.TabIndex = 9;
            this.btnBrowseLog.Text = "...";
            this.btnBrowseLog.UseVisualStyleBackColor = true;
            this.btnBrowseLog.Click += new EventHandler(this.btnBrowseLog_Click);
            
            // btnOK
            this.btnOK.Location = new Point(220, 170);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(90, 30);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            
            // btnCancel
            this.btnCancel.Location = new Point(330, 170);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(90, 30);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            
            // ServerConfigForm
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(444, 221);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnBrowseLog);
            this.Controls.Add(this.txtLogPath);
            this.Controls.Add(this.lblLogPath);
            this.Controls.Add(this.btnBrowseStorage);
            this.Controls.Add(this.txtStoragePath);
            this.Controls.Add(this.lblStoragePath);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtHost);
            this.Controls.Add(this.lblHost);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ServerConfigForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Server Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
