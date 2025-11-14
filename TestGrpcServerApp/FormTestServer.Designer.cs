namespace TestGrpcServerApp
{
    partial class FormTestServer
    {
        private System.ComponentModel.IContainer components = null;
        
        private Button btnOpenConfig;
        private Button btnStartServer;
        private Button btnStopServer;
        private Button btnSendJson;
        private Button btnSendFile;
        private Button btnSendImage;
        private Button btnViewLogs;
        private Button btnClearLog;
        private TextBox txtLog;
        private GroupBox grpServerControl;
        private GroupBox grpOperations;
        private GroupBox grpLog;
        private CheckBox chkAutoStart;

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
            this.btnOpenConfig = new Button();
            this.btnStartServer = new Button();
            this.btnStopServer = new Button();
            this.btnSendJson = new Button();
            this.btnSendFile = new Button();
            this.btnSendImage = new Button();
            this.btnViewLogs = new Button();
            this.btnClearLog = new Button();
            this.txtLog = new TextBox();
            this.grpServerControl = new GroupBox();
            this.grpOperations = new GroupBox();
            this.grpLog = new GroupBox();
            this.chkAutoStart = new CheckBox();
            
            this.grpServerControl.SuspendLayout();
            this.grpOperations.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            
            // grpServerControl
            this.grpServerControl.Controls.Add(this.btnOpenConfig);
            this.grpServerControl.Controls.Add(this.btnStartServer);
            this.grpServerControl.Controls.Add(this.btnStopServer);
            this.grpServerControl.Controls.Add(this.chkAutoStart);
            this.grpServerControl.Location = new Point(12, 12);
            this.grpServerControl.Name = "grpServerControl";
            this.grpServerControl.Size = new Size(760, 80);
            this.grpServerControl.TabIndex = 0;
            this.grpServerControl.TabStop = false;
            this.grpServerControl.Text = "Server Control";
            
            // btnOpenConfig
            this.btnOpenConfig.Location = new Point(20, 30);
            this.btnOpenConfig.Name = "btnOpenConfig";
            this.btnOpenConfig.Size = new Size(150, 35);
            this.btnOpenConfig.TabIndex = 0;
            this.btnOpenConfig.Text = "Open Server Config";
            this.btnOpenConfig.UseVisualStyleBackColor = true;
            this.btnOpenConfig.Click += new EventHandler(this.btnOpenConfig_Click);
            
            // btnStartServer
            this.btnStartServer.Location = new Point(190, 30);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new Size(120, 35);
            this.btnStartServer.TabIndex = 1;
            this.btnStartServer.Text = "Start Server";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new EventHandler(this.btnStartServer_Click);
            
            // btnStopServer
            this.btnStopServer.Location = new Point(330, 30);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new Size(120, 35);
            this.btnStopServer.TabIndex = 2;
            this.btnStopServer.Text = "Stop Server";
            this.btnStopServer.Enabled = false;
            this.btnStopServer.UseVisualStyleBackColor = true;
            this.btnStopServer.Click += new EventHandler(this.btnStopServer_Click);
            
            // chkAutoStart
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new Point(470, 38);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new Size(150, 19);
            this.chkAutoStart.TabIndex = 3;
            this.chkAutoStart.Text = "Auto-start on Launch";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            this.chkAutoStart.CheckedChanged += new EventHandler(this.chkAutoStart_CheckedChanged);
            
            // grpOperations
            this.grpOperations.Controls.Add(this.btnSendJson);
            this.grpOperations.Controls.Add(this.btnSendFile);
            this.grpOperations.Controls.Add(this.btnSendImage);
            this.grpOperations.Location = new Point(12, 98);
            this.grpOperations.Name = "grpOperations";
            this.grpOperations.Size = new Size(760, 80);
            this.grpOperations.TabIndex = 1;
            this.grpOperations.TabStop = false;
            this.grpOperations.Text = "Test Operations";
            
            // btnSendJson
            this.btnSendJson.Location = new Point(20, 30);
            this.btnSendJson.Name = "btnSendJson";
            this.btnSendJson.Size = new Size(150, 35);
            this.btnSendJson.TabIndex = 0;
            this.btnSendJson.Text = "Send JSON";
            this.btnSendJson.Enabled = false;
            this.btnSendJson.UseVisualStyleBackColor = true;
            this.btnSendJson.Click += new EventHandler(this.btnSendJson_Click);
            
            // btnSendFile
            this.btnSendFile.Location = new Point(190, 30);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new Size(150, 35);
            this.btnSendFile.TabIndex = 1;
            this.btnSendFile.Text = "Send File";
            this.btnSendFile.Enabled = false;
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new EventHandler(this.btnSendFile_Click);
            
            // btnSendImage
            this.btnSendImage.Location = new Point(360, 30);
            this.btnSendImage.Name = "btnSendImage";
            this.btnSendImage.Size = new Size(150, 35);
            this.btnSendImage.TabIndex = 2;
            this.btnSendImage.Text = "Send Image (Path)";
            this.btnSendImage.Enabled = false;
            this.btnSendImage.UseVisualStyleBackColor = true;
            this.btnSendImage.Click += new EventHandler(this.btnSendImage_Click);
            
            // grpLog
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Controls.Add(this.btnViewLogs);
            this.grpLog.Controls.Add(this.btnClearLog);
            this.grpLog.Location = new Point(12, 184);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new Size(760, 354);
            this.grpLog.TabIndex = 2;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Log";
            
            // txtLog
            this.txtLog.Location = new Point(20, 25);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = ScrollBars.Vertical;
            this.txtLog.Size = new Size(720, 280);
            this.txtLog.TabIndex = 0;
            
            // btnViewLogs
            this.btnViewLogs.Location = new Point(20, 315);
            this.btnViewLogs.Name = "btnViewLogs";
            this.btnViewLogs.Size = new Size(120, 30);
            this.btnViewLogs.TabIndex = 1;
            this.btnViewLogs.Text = "View Log Files";
            this.btnViewLogs.UseVisualStyleBackColor = true;
            this.btnViewLogs.Click += new EventHandler(this.btnViewLogs_Click);
            
            // btnClearLog
            this.btnClearLog.Location = new Point(160, 315);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new Size(120, 30);
            this.btnClearLog.TabIndex = 2;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new EventHandler(this.btnClearLog_Click);
            
            // TestServerForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 561);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpOperations);
            this.Controls.Add(this.grpServerControl);
            this.Name = "TestServerForm";
            this.Text = "Test gRPC Server Application";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += new EventHandler(this.TestServerForm_Load);
            
            this.grpServerControl.ResumeLayout(false);
            this.grpServerControl.PerformLayout();
            this.grpOperations.ResumeLayout(false);
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
