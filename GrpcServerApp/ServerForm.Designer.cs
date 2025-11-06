using System.Windows.Forms;

namespace GrpcServerApp;

public partial class ServerForm
{
    private Button _btnStart;
    private Button _btnStop;
    private ListBox _files;
    private Panel _panelLeft;
    private TextBox _txtType;
    private TextBox _txtJson;
    private Button _btnBroadcast;
    private Button _btnPushFile;
    
    // Server monitoring controls
    private GroupBox _grpServerStats;
    private Label _lblServerStats;
    private Button _btnResetStats;

    // Server stress test controls
    private GroupBox _grpStressTest;
    private ComboBox _cmbStressType;
    private TextBox _txtStressInterval;
    private TextBox _txtStressSize;
    private TextBox _txtStressIterations;
    private CheckBox _chkStressUnlimited;
    private NumericUpDown _numRetryCount;
    private CheckBox _chkUseAckMode;
    private Button _btnStartStressTest;
    private Label _lblStressStats;

    private void InitializeComponent()
    {
        lblHost = new Label();
        lblPort = new Label();
        txtHost = new TextBox();
        txtPort = new TextBox();
        _btnApply = new Button();
        _btnStart = new Button();
        _btnStop = new Button();
        _files = new ListBox();
        _panelLeft = new Panel();
        _grpStressTest = new GroupBox();
        _lblStressStats = new Label();
        _btnStartStressTest = new Button();
        _chkUseAckMode = new CheckBox();
        _numRetryCount = new NumericUpDown();
        _chkStressUnlimited = new CheckBox();
        _txtStressIterations = new TextBox();
        _txtStressSize = new TextBox();
        _txtStressInterval = new TextBox();
        _cmbStressType = new ComboBox();
        lblRetry = new Label();
        _grpServerStats = new GroupBox();
        _btnResetStats = new Button();
        _lblServerStats = new Label();
        _btnPushFile = new Button();
        _btnBroadcast = new Button();
        _txtJson = new TextBox();
        _txtType = new TextBox();
        splitContainer1 = new SplitContainer();
        btnLog = new Button();
        _log = new TextBox();
        _panelLeft.SuspendLayout();
        _grpStressTest.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_numRetryCount).BeginInit();
        _grpServerStats.SuspendLayout();
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
        _btnApply.Click += btnApply_Click;
        // 
        // _btnStart
        // 
        _btnStart.Dock = DockStyle.Top;
        _btnStart.Location = new Point(0, 0);
        _btnStart.Name = "_btnStart";
        _btnStart.Size = new Size(499, 40);
        _btnStart.TabIndex = 6;
        _btnStart.Text = "啟動 Server";
        _btnStart.Click += btnStart_Click;
        // 
        // _btnStop
        // 
        _btnStop.Dock = DockStyle.Top;
        _btnStop.Enabled = false;
        _btnStop.Location = new Point(0, 40);
        _btnStop.Name = "_btnStop";
        _btnStop.Size = new Size(499, 40);
        _btnStop.TabIndex = 5;
        _btnStop.Text = "停止 Server";
        _btnStop.Click += btnStop_Click;
        // 
        // _files
        // 
        _files.Dock = DockStyle.Fill;
        _files.ItemHeight = 15;
        _files.Location = new Point(0, 0);
        _files.Name = "_files";
        _files.Size = new Size(252, 472);
        _files.TabIndex = 1;
        // 
        // _panelLeft
        // 
        _panelLeft.Controls.Add(_grpStressTest);
        _panelLeft.Controls.Add(_grpServerStats);
        _panelLeft.Controls.Add(_btnPushFile);
        _panelLeft.Controls.Add(_btnBroadcast);
        _panelLeft.Controls.Add(_txtJson);
        _panelLeft.Controls.Add(_txtType);
        _panelLeft.Controls.Add(_btnStop);
        _panelLeft.Controls.Add(_btnStart);
        _panelLeft.Dock = DockStyle.Left;
        _panelLeft.Location = new Point(0, 0);
        _panelLeft.Name = "_panelLeft";
        _panelLeft.Size = new Size(499, 580);
        _panelLeft.TabIndex = 0;
        // 
        // _grpStressTest
        // 
        _grpStressTest.Controls.Add(_lblStressStats);
        _grpStressTest.Controls.Add(_btnStartStressTest);
        _grpStressTest.Controls.Add(_chkUseAckMode);
        _grpStressTest.Controls.Add(_numRetryCount);
        _grpStressTest.Controls.Add(_chkStressUnlimited);
        _grpStressTest.Controls.Add(_txtStressIterations);
        _grpStressTest.Controls.Add(_txtStressSize);
        _grpStressTest.Controls.Add(_txtStressInterval);
        _grpStressTest.Controls.Add(_cmbStressType);
        _grpStressTest.Controls.Add(lblRetry);
        _grpStressTest.Dock = DockStyle.Top;
        _grpStressTest.Location = new Point(0, 373);
        _grpStressTest.Name = "_grpStressTest";
        _grpStressTest.Size = new Size(499, 190);
        _grpStressTest.TabIndex = 6;
        _grpStressTest.TabStop = false;
        _grpStressTest.Text = "壓力測試 (Server端測試)";
        // 
        // _lblStressStats
        // 
        _lblStressStats.Location = new Point(10, 105);
        _lblStressStats.Name = "_lblStressStats";
        _lblStressStats.Size = new Size(480, 75);
        _lblStressStats.TabIndex = 6;
        _lblStressStats.Text = "執行: 0 | 成功: 0 | 失敗: 0 | 成功率: 0% | 平均: 0ms | 總時間: 00:00:00";
        _lblStressStats.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _btnStartStressTest
        // 
        _btnStartStressTest.Location = new Point(10, 61);
        _btnStartStressTest.Name = "_btnStartStressTest";
        _btnStartStressTest.Size = new Size(150, 35);
        _btnStartStressTest.TabIndex = 5;
        _btnStartStressTest.Text = "開始壓測";
        _btnStartStressTest.Click += btnStartStressTest_Click;
        // 
        // _chkUseAckMode
        // 
        _chkUseAckMode.Location = new Point(280, 71);
        _chkUseAckMode.Name = "_chkUseAckMode";
        _chkUseAckMode.Size = new Size(100, 23);
        _chkUseAckMode.TabIndex = 9;
        _chkUseAckMode.Text = "使用確認模式";
        // 
        // _numRetryCount
        // 
        _numRetryCount.BorderStyle = BorderStyle.FixedSingle;
        _numRetryCount.Location = new Point(170, 71);
        _numRetryCount.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        _numRetryCount.Name = "_numRetryCount";
        _numRetryCount.Size = new Size(100, 23);
        _numRetryCount.TabIndex = 7;
        _numRetryCount.TextAlign = HorizontalAlignment.Center;
        // 
        // _chkStressUnlimited
        // 
        _chkStressUnlimited.Location = new Point(390, 70);
        _chkStressUnlimited.Name = "_chkStressUnlimited";
        _chkStressUnlimited.Size = new Size(90, 23);
        _chkStressUnlimited.TabIndex = 4;
        _chkStressUnlimited.Text = "無限制";
        // 
        // _txtStressIterations
        // 
        _txtStressIterations.Location = new Point(390, 25);
        _txtStressIterations.Name = "_txtStressIterations";
        _txtStressIterations.PlaceholderText = "次數";
        _txtStressIterations.Size = new Size(100, 23);
        _txtStressIterations.TabIndex = 3;
        _txtStressIterations.Text = "100";
        // 
        // _txtStressSize
        // 
        _txtStressSize.Location = new Point(280, 25);
        _txtStressSize.Name = "_txtStressSize";
        _txtStressSize.PlaceholderText = "大小(KB)";
        _txtStressSize.Size = new Size(100, 23);
        _txtStressSize.TabIndex = 2;
        _txtStressSize.Text = "10";
        // 
        // _txtStressInterval
        // 
        _txtStressInterval.Location = new Point(170, 25);
        _txtStressInterval.Name = "_txtStressInterval";
        _txtStressInterval.PlaceholderText = "間隔(ms)";
        _txtStressInterval.Size = new Size(100, 23);
        _txtStressInterval.TabIndex = 1;
        _txtStressInterval.Text = "1000";
        // 
        // _cmbStressType
        // 
        _cmbStressType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbStressType.Items.AddRange(new object[] { "廣播 JSON", "推送檔案", "混合測試" });
        _cmbStressType.Location = new Point(10, 25);
        _cmbStressType.Name = "_cmbStressType";
        _cmbStressType.Size = new Size(150, 23);
        _cmbStressType.TabIndex = 0;
        // 
        // lblRetry
        // 
        lblRetry.Location = new Point(170, 54);
        lblRetry.Name = "lblRetry";
        lblRetry.Size = new Size(100, 15);
        lblRetry.TabIndex = 8;
        lblRetry.Text = "重試次數";
        lblRetry.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _grpServerStats
        // 
        _grpServerStats.Controls.Add(_btnResetStats);
        _grpServerStats.Controls.Add(_lblServerStats);
        _grpServerStats.Dock = DockStyle.Top;
        _grpServerStats.Location = new Point(0, 263);
        _grpServerStats.Name = "_grpServerStats";
        _grpServerStats.Size = new Size(499, 110);
        _grpServerStats.TabIndex = 5;
        _grpServerStats.TabStop = false;
        _grpServerStats.Text = "伺服器統計資訊";
        // 
        // _btnResetStats
        // 
        _btnResetStats.Location = new Point(10, 70);
        _btnResetStats.Name = "_btnResetStats";
        _btnResetStats.Size = new Size(120, 30);
        _btnResetStats.TabIndex = 1;
        _btnResetStats.Text = "重置統計";
        _btnResetStats.Click += _btnResetStats_Click;
        // 
        // _lblServerStats
        // 
        _lblServerStats.Location = new Point(10, 25);
        _lblServerStats.Name = "_lblServerStats";
        _lblServerStats.Size = new Size(607, 40);
        _lblServerStats.TabIndex = 0;
        _lblServerStats.Text = "運行時間: 00:00:00 | 總請求: 0 (0/秒) | 總流量: 0 MB (0 MB/秒) | 連線數: 0";
        _lblServerStats.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _btnPushFile
        // 
        _btnPushFile.Dock = DockStyle.Top;
        _btnPushFile.Enabled = false;
        _btnPushFile.Location = new Point(0, 223);
        _btnPushFile.Name = "_btnPushFile";
        _btnPushFile.Size = new Size(499, 40);
        _btnPushFile.TabIndex = 1;
        _btnPushFile.Text = "推送檔案";
        _btnPushFile.Click += btnPushFile_Click;
        // 
        // _btnBroadcast
        // 
        _btnBroadcast.Dock = DockStyle.Top;
        _btnBroadcast.Enabled = false;
        _btnBroadcast.Location = new Point(0, 183);
        _btnBroadcast.Name = "_btnBroadcast";
        _btnBroadcast.Size = new Size(499, 40);
        _btnBroadcast.TabIndex = 2;
        _btnBroadcast.Text = "廣播 JSON";
        _btnBroadcast.Click += btnBroadcast_Click;
        // 
        // _txtJson
        // 
        _txtJson.Dock = DockStyle.Top;
        _txtJson.Location = new Point(0, 103);
        _txtJson.Multiline = true;
        _txtJson.Name = "_txtJson";
        _txtJson.PlaceholderText = "JSON Body";
        _txtJson.ScrollBars = ScrollBars.Vertical;
        _txtJson.Size = new Size(499, 80);
        _txtJson.TabIndex = 3;
        // 
        // _txtType
        // 
        _txtType.Dock = DockStyle.Top;
        _txtType.Location = new Point(0, 80);
        _txtType.Name = "_txtType";
        _txtType.PlaceholderText = "Type";
        _txtType.Size = new Size(499, 23);
        _txtType.TabIndex = 4;
        // 
        // splitContainer1
        // 
        splitContainer1.Dock = DockStyle.Right;
        splitContainer1.Location = new Point(885, 0);
        splitContainer1.Name = "splitContainer1";
        splitContainer1.Orientation = Orientation.Horizontal;
        // 
        // splitContainer1.Panel1
        // 
        splitContainer1.Panel1.Controls.Add(txtHost);
        splitContainer1.Panel1.Controls.Add(btnLog);
        splitContainer1.Panel1.Controls.Add(_btnApply);
        splitContainer1.Panel1.Controls.Add(txtPort);
        splitContainer1.Panel1.Controls.Add(lblPort);
        splitContainer1.Panel1.Controls.Add(lblHost);
        // 
        // splitContainer1.Panel2
        // 
        splitContainer1.Panel2.Controls.Add(_files);
        splitContainer1.Size = new Size(252, 580);
        splitContainer1.SplitterDistance = 104;
        splitContainer1.TabIndex = 2;
        // 
        // btnLog
        // 
        btnLog.Location = new Point(162, 62);
        btnLog.Name = "btnLog";
        btnLog.Size = new Size(64, 26);
        btnLog.TabIndex = 5;
        btnLog.Text = "Log";
        btnLog.Click += btnLog_Click;
        // 
        // _log
        // 
        _log.Dock = DockStyle.Fill;
        _log.Location = new Point(499, 0);
        _log.Multiline = true;
        _log.Name = "_log";
        _log.ScrollBars = ScrollBars.Vertical;
        _log.Size = new Size(386, 580);
        _log.TabIndex = 3;
        // 
        // ServerForm
        // 
        ClientSize = new Size(1137, 580);
        Controls.Add(_log);
        Controls.Add(splitContainer1);
        Controls.Add(_panelLeft);
        Name = "ServerForm";
        Text = "gRPC Server";
        Load += ServerForm_Load;
        _panelLeft.ResumeLayout(false);
        _panelLeft.PerformLayout();
        _grpStressTest.ResumeLayout(false);
        _grpStressTest.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_numRetryCount).EndInit();
        _grpServerStats.ResumeLayout(false);
        splitContainer1.Panel1.ResumeLayout(false);
        splitContainer1.Panel1.PerformLayout();
        splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
        splitContainer1.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
    private Label lblHost;
    private Label lblPort;
    private TextBox txtHost;
    private TextBox txtPort;
    private Button _btnApply;
    private SplitContainer splitContainer1;
    private Label lblRetry;
    private TextBox _log;
    private Button btnLog;
}
