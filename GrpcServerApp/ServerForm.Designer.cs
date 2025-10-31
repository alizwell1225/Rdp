using System.Windows.Forms;

namespace GrpcServerApp;

public partial class ServerForm
{
    private TextBox _log;
    private Button _btnStart;
    private Button _btnStop;
    private ListBox _files;
    private Panel _panelLeft;
    private TextBox _txtType;
    private TextBox _txtJson;
    private Button _btnBroadcast;
    private Button _btnPushFile;

    private void InitializeComponent()
    {
        lblHost = new Label();
        lblPort = new Label();
        txtHost = new TextBox();
        txtPort = new TextBox();
        _btnApply = new Button();
        _log = new TextBox();
        _btnStart = new Button();
        _btnStop = new Button();
        _files = new ListBox();
        _panelLeft = new Panel();
        _btnPushFile = new Button();
        _btnBroadcast = new Button();
        _txtJson = new TextBox();
        _txtType = new TextBox();
        splitContainer1 = new SplitContainer();
        _panelLeft.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
        splitContainer1.Panel1.SuspendLayout();
        splitContainer1.Panel2.SuspendLayout();
        splitContainer1.SuspendLayout();
        SuspendLayout();
        // 
        // lblHost
        // 
        lblHost.Location = new Point(30, 10);
        lblHost.Name = "lblHost";
        lblHost.Size = new Size(50, 23);
        lblHost.TabIndex = 1;
        lblHost.Text = "Host";
        // 
        // lblPort
        // 
        lblPort.Location = new Point(30, 33);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(35, 23);
        lblPort.TabIndex = 3;
        lblPort.Text = "Port";
        // 
        // txtHost
        // 
        txtHost.Location = new Point(86, 10);
        txtHost.Name = "txtHost";
        txtHost.Size = new Size(140, 23);
        txtHost.TabIndex = 2;
        txtHost.Text = "localhost";
        // 
        // txtPort
        // 
        txtPort.Location = new Point(86, 33);
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(70, 23);
        txtPort.TabIndex = 4;
        txtPort.Text = "50051";
        // 
        // _btnApply
        // 
        _btnApply.Location = new Point(36, 62);
        _btnApply.Name = "_btnApply";
        _btnApply.Size = new Size(120, 26);
        _btnApply.TabIndex = 5;
        _btnApply.Text = "Apply && Start";
        _btnApply.Click += _btnApply_Click;
        // 
        // _log
        // 
        _log.Dock = DockStyle.Fill;
        _log.Location = new Point(0, 263);
        _log.Multiline = true;
        _log.Name = "_log";
        _log.ScrollBars = ScrollBars.Vertical;
        _log.Size = new Size(656, 317);
        _log.TabIndex = 0;
        // 
        // _btnStart
        // 
        _btnStart.Dock = DockStyle.Top;
        _btnStart.Location = new Point(0, 0);
        _btnStart.Name = "_btnStart";
        _btnStart.Size = new Size(656, 40);
        _btnStart.TabIndex = 6;
        _btnStart.Text = "啟動 Server";
        _btnStart.Click += _btnStart_Click;
        // 
        // _btnStop
        // 
        _btnStop.Dock = DockStyle.Top;
        _btnStop.Enabled = false;
        _btnStop.Location = new Point(0, 40);
        _btnStop.Name = "_btnStop";
        _btnStop.Size = new Size(656, 40);
        _btnStop.TabIndex = 5;
        _btnStop.Text = "停止 Server";
        _btnStop.Click += _btnStop_Click;
        // 
        // _files
        // 
        _files.Dock = DockStyle.Fill;
        _files.ItemHeight = 15;
        _files.Location = new Point(0, 0);
        _files.Name = "_files";
        _files.Size = new Size(255, 472);
        _files.TabIndex = 1;
        // 
        // _panelLeft
        // 
        _panelLeft.Controls.Add(_log);
        _panelLeft.Controls.Add(_btnPushFile);
        _panelLeft.Controls.Add(_btnBroadcast);
        _panelLeft.Controls.Add(_txtJson);
        _panelLeft.Controls.Add(_txtType);
        _panelLeft.Controls.Add(_btnStop);
        _panelLeft.Controls.Add(_btnStart);
        _panelLeft.Dock = DockStyle.Left;
        _panelLeft.Location = new Point(0, 0);
        _panelLeft.Name = "_panelLeft";
        _panelLeft.Size = new Size(656, 580);
        _panelLeft.TabIndex = 0;
        // 
        // _btnPushFile
        // 
        _btnPushFile.Dock = DockStyle.Top;
        _btnPushFile.Enabled = false;
        _btnPushFile.Location = new Point(0, 223);
        _btnPushFile.Name = "_btnPushFile";
        _btnPushFile.Size = new Size(656, 40);
        _btnPushFile.TabIndex = 1;
        _btnPushFile.Text = "推送檔案";
        _btnPushFile.Click += _btnPushFile_Click;
        // 
        // _btnBroadcast
        // 
        _btnBroadcast.Dock = DockStyle.Top;
        _btnBroadcast.Enabled = false;
        _btnBroadcast.Location = new Point(0, 183);
        _btnBroadcast.Name = "_btnBroadcast";
        _btnBroadcast.Size = new Size(656, 40);
        _btnBroadcast.TabIndex = 2;
        _btnBroadcast.Text = "廣播 JSON";
        _btnBroadcast.Click += _btnBroadcast_Click;
        // 
        // _txtJson
        // 
        _txtJson.Dock = DockStyle.Top;
        _txtJson.Location = new Point(0, 103);
        _txtJson.Multiline = true;
        _txtJson.Name = "_txtJson";
        _txtJson.PlaceholderText = "JSON Body";
        _txtJson.ScrollBars = ScrollBars.Vertical;
        _txtJson.Size = new Size(656, 80);
        _txtJson.TabIndex = 3;
        // 
        // _txtType
        // 
        _txtType.Dock = DockStyle.Top;
        _txtType.Location = new Point(0, 80);
        _txtType.Name = "_txtType";
        _txtType.PlaceholderText = "Type";
        _txtType.Size = new Size(656, 23);
        _txtType.TabIndex = 4;
        // 
        // splitContainer1
        // 
        splitContainer1.Dock = DockStyle.Right;
        splitContainer1.Location = new Point(662, 0);
        splitContainer1.Name = "splitContainer1";
        splitContainer1.Orientation = Orientation.Horizontal;
        // 
        // splitContainer1.Panel1
        // 
        splitContainer1.Panel1.Controls.Add(txtHost);
        splitContainer1.Panel1.Controls.Add(_btnApply);
        splitContainer1.Panel1.Controls.Add(txtPort);
        splitContainer1.Panel1.Controls.Add(lblPort);
        splitContainer1.Panel1.Controls.Add(lblHost);
        // 
        // splitContainer1.Panel2
        // 
        splitContainer1.Panel2.Controls.Add(_files);
        splitContainer1.Size = new Size(255, 580);
        splitContainer1.SplitterDistance = 104;
        splitContainer1.TabIndex = 2;
        // 
        // ServerForm
        // 
        ClientSize = new Size(917, 580);
        Controls.Add(splitContainer1);
        Controls.Add(_panelLeft);
        Name = "ServerForm";
        Text = "gRPC Server";
        _panelLeft.ResumeLayout(false);
        _panelLeft.PerformLayout();
        splitContainer1.Panel1.ResumeLayout(false);
        splitContainer1.Panel1.PerformLayout();
        splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
        splitContainer1.ResumeLayout(false);
        ResumeLayout(false);
    }
    private Label lblHost;
    private Label lblPort;
    private TextBox txtHost;
    private TextBox txtPort;
    private Button _btnApply;
    private SplitContainer splitContainer1;
}
