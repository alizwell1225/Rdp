using LIB_RDP.Core;
using LIB_RDP.Models;
using LIB_RDP.UI;
using static System.Windows.Forms.DataFormats;

namespace LIB_Define.RDP
{
    public partial class FormShow_Rdp : Form
    {
        public event EventHandler<RdpStateEventArgs> ConnectionStateChanged;
        private RdpConnection _connection;

        public FormShow_Rdp()
        {
            InitializeComponent();
            uc.ActClose += CloseForm;
        }

        private int Index { get; set; }= -1;

        public FormShow_Rdp(int index)
        {
            Index = index;
            InitializeComponent();
            uc.ActClose += CloseForm;
            uc.ActMaxViewer += Uc_ActMaxViewer;
            uc.ActMidViewer += Uc_ActMidViewer;
        }

        void Relayout(bool title)
        {
            this.SuspendLayout();

            this.FormBorderStyle = (title ? FormBorderStyle.Sizable : FormBorderStyle.None);

            // 將視窗大小重設一次（觸發 Resize）
            var sz = this.Size;
            this.Size = new Size(sz.Width + 1, sz.Height + 1);
            this.Size = sz;

            this.ResumeLayout(true);

        }

        private void Uc_ActMidViewer(int obj, bool title, bool fixSize)
        {
            this.WindowState = FormWindowState.Normal;
            Relayout(title);
            
            ToggleViewerSize();
        }

        private void Uc_ActMaxViewer(int obj,bool title,bool fixSize)
        {
            this.WindowState = FormWindowState.Maximized;
           
            //uc.SetMax(this.Width, this.Height);
            Relayout(title);
            uc.SetSmartSize(fixSize);
            ToggleViewerSize();
        }

        private void CloseForm(int obj)
        {
            StopRdp();
            Dispose();
            GC.Collect();
        }

        void StopRdp()
        {
            try
            {
                _connection?.Disconnect();
            }
            catch (Exception e)
            {
            }
        }

        public void SetRdpConnection(RdpConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.ConnectionStateChanged += Connection_StateChanged;

            // 設定視窗的最小尺寸
            // 應用主題
            ThemeManager.ApplyTheme(this);
            BackColor = ThemeManager.BackgroundColor;
        }

        private void Connection_StateChanged(object sender, EventArgs e)
        {
            if (_connection != null) OnConnectionStateChanged(_connection.State);
        }

        protected virtual void OnConnectionStateChanged(RdpState state)
        {
            var args = new RdpStateEventArgs(state, _connection?.ConnectionId);
            ConnectionStateChanged?.Invoke(this, args);
        }

        private void ToggleViewerSize()
        {
            if (_connection != null)
            {
                _connection.Configure(new RdpConfig
                { ScreenWidth = uc.InnerViewer.Width, ScreenHeight = uc.InnerViewer.Height, ColorDepth = 64});

            }

            if (!uc.IsFullScreen)
            {
                uc.SaveCurrentState();
                uc.SetMax(this.Width, this.Height);
                uc.Dock = DockStyle.Fill;
                uc.BringToFront();
            }
        }

        private void FormShowRdp_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            //this.FormBorderStyle = FormBorderStyle.None;
            if (Index!=-1)
            {
                this.Text = $"ROM PC- {Index + 1:D2}";
            }
            uc.SetRdpConnection(_connection);
            ToggleViewerSize();

        }


        private void FormShowRdp_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseForm(0);
        }
    }
}
