using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GrpcClientApp
{
    partial class ClientForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        // UI controls (created here so designer can preview)
        private TextBox _log;
    private TextBox txtHost;
    private TextBox txtPort;
    private Button _btnApply;
        private ProgressBar _pbUpload;
        private Label _lblUpload;
        private ProgressBar _pbDownload;
        private Label _lblDownload;
        private ProgressBar _pbScreenshot;
        private Label _lblScreenshot;
        private PictureBox _pic;
        private Button _btnSendJson;
        private Button _btnUpload;
        private Button _btnDownload;
        private Button _btnScreenshot;
        private Button _btnConnect;
        private Panel _panelLeft;

        /// <summary>
        /// Initialize UI components. Only creates controls and sets their properties line-by-line.
        /// No conditional logic here.
        /// </summary>
        private void InitializeComponent()
        {
            _log = new TextBox();
            _pbUpload = new ProgressBar();
            _lblUpload = new Label();
            _pbDownload = new ProgressBar();
            _lblDownload = new Label();
            _pbScreenshot = new ProgressBar();
            _lblScreenshot = new Label();
            _pic = new PictureBox();
            _btnSendJson = new Button();
            _btnUpload = new Button();
            _btnDownload = new Button();
            _btnScreenshot = new Button();
            _btnConnect = new Button();
            _panelLeft = new Panel();
            lblHost = new Label();
            txtHost = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            _btnApply = new Button();
            splitContainer1 = new SplitContainer();
            ((ISupportInitialize)_pic).BeginInit();
            _panelLeft.SuspendLayout();
            ((ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // _log
            // 
            _log.Dock = DockStyle.Fill;
            _log.Location = new Point(0, 308);
            _log.Multiline = true;
            _log.Name = "_log";
            _log.ScrollBars = ScrollBars.Vertical;
            _log.Size = new Size(623, 336);
            _log.TabIndex = 3;
            // 
            // _pbUpload
            // 
            _pbUpload.Dock = DockStyle.Top;
            _pbUpload.Location = new Point(0, 200);
            _pbUpload.Name = "_pbUpload";
            _pbUpload.Size = new Size(623, 18);
            _pbUpload.TabIndex = 6;
            // 
            // _lblUpload
            // 
            _lblUpload.Dock = DockStyle.Top;
            _lblUpload.Location = new Point(0, 218);
            _lblUpload.Name = "_lblUpload";
            _lblUpload.Size = new Size(623, 18);
            _lblUpload.TabIndex = 5;
            _lblUpload.Text = "Upload: -";
            // 
            // _pbDownload
            // 
            _pbDownload.Dock = DockStyle.Top;
            _pbDownload.Location = new Point(0, 236);
            _pbDownload.Name = "_pbDownload";
            _pbDownload.Size = new Size(623, 18);
            _pbDownload.TabIndex = 5;
            // 
            // _lblDownload
            // 
            _lblDownload.Dock = DockStyle.Top;
            _lblDownload.Location = new Point(0, 254);
            _lblDownload.Name = "_lblDownload";
            _lblDownload.Size = new Size(623, 18);
            _lblDownload.TabIndex = 3;
            _lblDownload.Text = "Download: -";
            // 
            // _pbScreenshot
            // 
            _pbScreenshot.Dock = DockStyle.Top;
            _pbScreenshot.Location = new Point(0, 272);
            _pbScreenshot.Name = "_pbScreenshot";
            _pbScreenshot.Size = new Size(623, 18);
            _pbScreenshot.TabIndex = 4;
            // 
            // _lblScreenshot
            // 
            _lblScreenshot.Dock = DockStyle.Top;
            _lblScreenshot.Location = new Point(0, 290);
            _lblScreenshot.Name = "_lblScreenshot";
            _lblScreenshot.Size = new Size(623, 18);
            _lblScreenshot.TabIndex = 1;
            _lblScreenshot.Text = "Screenshot: -";
            // 
            // _pic
            // 
            _pic.BorderStyle = BorderStyle.FixedSingle;
            _pic.Dock = DockStyle.Fill;
            _pic.Location = new Point(0, 0);
            _pic.Name = "_pic";
            _pic.Size = new Size(294, 544);
            _pic.TabIndex = 1;
            _pic.TabStop = false;
            // 
            // _btnSendJson
            // 
            _btnSendJson.Dock = DockStyle.Top;
            _btnSendJson.Enabled = false;
            _btnSendJson.Location = new Point(0, 40);
            _btnSendJson.Name = "_btnSendJson";
            _btnSendJson.Size = new Size(623, 40);
            _btnSendJson.TabIndex = 10;
            _btnSendJson.Text = "送 JSON";
            _btnSendJson.UseVisualStyleBackColor = true;
            _btnSendJson.Click += _btnSendJson_Click;
            // 
            // _btnUpload
            // 
            _btnUpload.Dock = DockStyle.Top;
            _btnUpload.Enabled = false;
            _btnUpload.Location = new Point(0, 80);
            _btnUpload.Name = "_btnUpload";
            _btnUpload.Size = new Size(623, 40);
            _btnUpload.TabIndex = 9;
            _btnUpload.Text = "上傳檔案";
            _btnUpload.UseVisualStyleBackColor = true;
            _btnUpload.Click += _btnUpload_Click;
            // 
            // _btnDownload
            // 
            _btnDownload.Dock = DockStyle.Top;
            _btnDownload.Enabled = false;
            _btnDownload.Location = new Point(0, 120);
            _btnDownload.Name = "_btnDownload";
            _btnDownload.Size = new Size(623, 40);
            _btnDownload.TabIndex = 8;
            _btnDownload.Text = "下載檔案";
            _btnDownload.UseVisualStyleBackColor = true;
            _btnDownload.Click += _btnDownload_Click;
            // 
            // _btnScreenshot
            // 
            _btnScreenshot.Dock = DockStyle.Top;
            _btnScreenshot.Enabled = false;
            _btnScreenshot.Location = new Point(0, 160);
            _btnScreenshot.Name = "_btnScreenshot";
            _btnScreenshot.Size = new Size(623, 40);
            _btnScreenshot.TabIndex = 7;
            _btnScreenshot.Text = "取得截圖";
            _btnScreenshot.UseVisualStyleBackColor = true;
            _btnScreenshot.Click += _btnScreenshot_Click;
            // 
            // _btnConnect
            // 
            _btnConnect.Dock = DockStyle.Top;
            _btnConnect.Location = new Point(0, 0);
            _btnConnect.Name = "_btnConnect";
            _btnConnect.Size = new Size(623, 40);
            _btnConnect.TabIndex = 11;
            _btnConnect.Text = "連線 Server";
            _btnConnect.UseVisualStyleBackColor = true;
            _btnConnect.Click += _btnConnect_Click;
            // 
            // _panelLeft
            // 
            _panelLeft.Controls.Add(_log);
            _panelLeft.Controls.Add(_lblScreenshot);
            _panelLeft.Controls.Add(_pbScreenshot);
            _panelLeft.Controls.Add(_lblDownload);
            _panelLeft.Controls.Add(_pbDownload);
            _panelLeft.Controls.Add(_lblUpload);
            _panelLeft.Controls.Add(_pbUpload);
            _panelLeft.Controls.Add(_btnScreenshot);
            _panelLeft.Controls.Add(_btnDownload);
            _panelLeft.Controls.Add(_btnUpload);
            _panelLeft.Controls.Add(_btnSendJson);
            _panelLeft.Controls.Add(_btnConnect);
            _panelLeft.Dock = DockStyle.Left;
            _panelLeft.Location = new Point(0, 0);
            _panelLeft.Name = "_panelLeft";
            _panelLeft.Size = new Size(623, 644);
            _panelLeft.TabIndex = 0;
            // 
            // lblHost
            // 
            lblHost.AutoSize = true;
            lblHost.Location = new Point(12, 24);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(36, 15);
            lblHost.TabIndex = 0;
            lblHost.Text = "Host:";
            // 
            // txtHost
            // 
            txtHost.Location = new Point(54, 21);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(120, 23);
            txtHost.TabIndex = 0;
            txtHost.Text = "localhost";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(180, 22);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(33, 15);
            lblPort.TabIndex = 1;
            lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(219, 19);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(60, 23);
            txtPort.TabIndex = 1;
            txtPort.Text = "50051";
            // 
            // _btnApply
            // 
            _btnApply.Location = new Point(160, 62);
            _btnApply.Name = "_btnApply";
            _btnApply.Size = new Size(120, 26);
            _btnApply.TabIndex = 2;
            _btnApply.Text = "Apply && Connect";
            _btnApply.UseVisualStyleBackColor = true;
            _btnApply.Click += _btnApply_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Right;
            splitContainer1.Location = new Point(629, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lblHost);
            splitContainer1.Panel1.Controls.Add(_btnApply);
            splitContainer1.Panel1.Controls.Add(txtPort);
            splitContainer1.Panel1.Controls.Add(lblPort);
            splitContainer1.Panel1.Controls.Add(txtHost);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(_pic);
            splitContainer1.Size = new Size(294, 644);
            splitContainer1.SplitterDistance = 96;
            splitContainer1.TabIndex = 3;
            // 
            // ClientForm
            // 
            ClientSize = new Size(923, 644);
            Controls.Add(splitContainer1);
            Controls.Add(_panelLeft);
            Name = "ClientForm";
            Text = "gRPC Client";
            Load += ClientForm_Load;
            ((ISupportInitialize)_pic).EndInit();
            _panelLeft.ResumeLayout(false);
            _panelLeft.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

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
        private Label lblHost;
        private Label lblPort;
        private SplitContainer splitContainer1;
    }
}
