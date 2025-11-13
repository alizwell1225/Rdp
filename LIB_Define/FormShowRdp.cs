using LIB_RDP.Core;
using LIB_RDP.Models;
using LIB_RDP.UI;

namespace LIB_Define
{
    public partial class FormShowRdp : Form
    {
        public event EventHandler<RdpStateEventArgs> ConnectionStateChanged;
        private RdpConnection _connection;

        public FormShowRdp()
        {
            InitializeComponent();
            uc.ActSinkViewer += CloseForm;
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
                _connection.Disconnect();
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
            MinimumSize = new Size(1920, 1080);
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
                _connection.Configure(new RdpConfig
                { ScreenWidth = uc.InnerViewer.Width, ScreenHeight = uc.InnerViewer.Height, ColorDepth = 64 });

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
            uc.SetRdpConnection(_connection);
            ToggleViewerSize();
        }


        private void FormShowRdp_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseForm(0);
        }
    }
}
