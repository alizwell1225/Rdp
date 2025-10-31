using System;
using System.Threading.Tasks;
using LIB_RDP.Core;
using LIB_RDP.Interfaces;
using LIB_RDP.Models;

namespace LIB_RDP.Builders
{
    /// <summary>
    /// RDP連線建構器，提供流暢的API來建立和配置RDP連線
    /// </summary>
    public class RdpConnectionBuilder
    {
        private string _hostName;
        private string _userName;
        private string _password;
        private string _domain;
        private int _screenWidth = 1920;
        private int _screenHeight = 1080;
        private int _colorDepth = 32;
        private bool _enableCredSspSupport = true;
        private bool _enableCompression = true;
        private bool _enableBitmapPersistence = true;
        private int _connectionTimeout = 30;
        private RdpConnectionProfile _profile;
        
        /// <summary>
        /// 設定主機名稱或IP位址
        /// </summary>
        public RdpConnectionBuilder WithHost(string hostName)
        {
            _hostName = hostName;
            return this;
        }
        
        /// <summary>
        /// 設定使用者憑證
        /// </summary>
        public RdpConnectionBuilder WithCredentials(string userName, string password, string domain = "")
        {
            _userName = userName;
            _password = password;
            _domain = domain;
            return this;
        }
        
        /// <summary>
        /// 設定畫面解析度
        /// </summary>
        public RdpConnectionBuilder WithResolution(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
            return this;
        }
        
        /// <summary>
        /// 設定顏色深度
        /// </summary>
        public RdpConnectionBuilder WithColorDepth(int depth)
        {
            _colorDepth = depth;
            return this;
        }
        
        /// <summary>
        /// 設定連線超時時間（秒）
        /// </summary>
        public RdpConnectionBuilder WithTimeout(int timeoutSeconds)
        {
            _connectionTimeout = timeoutSeconds;
            return this;
        }
        
        /// <summary>
        /// 啟用或停用CredSSP支援
        /// </summary>
        public RdpConnectionBuilder WithCredSspSupport(bool enable)
        {
            _enableCredSspSupport = enable;
            return this;
        }
        
        /// <summary>
        /// 啟用或停用壓縮
        /// </summary>
        public RdpConnectionBuilder WithCompression(bool enable)
        {
            _enableCompression = enable;
            return this;
        }
        
        /// <summary>
        /// 啟用或停用點陣圖持久化
        /// </summary>
        public RdpConnectionBuilder WithBitmapPersistence(bool enable)
        {
            _enableBitmapPersistence = enable;
            return this;
        }
        
        /// <summary>
        /// 從連線配置載入設定
        /// </summary>
        public RdpConnectionBuilder FromProfile(RdpConnectionProfile profile)
        {
            _profile = profile;
            _hostName = profile.HostName;
            _userName = profile.UserName;
            _connectionTimeout = profile.ConnectionTimeout;
            
            if (profile.Config != null)
            {
                _screenWidth = profile.Config.ScreenWidth;
                _screenHeight = profile.Config.ScreenHeight;
                _colorDepth = profile.Config.ColorDepth;
                _domain = profile.Config.Domain;
                _enableCredSspSupport = profile.Config.EnableCredSspSupport;
                _enableCompression = profile.Config.EnableCompression;
                _enableBitmapPersistence = profile.Config.EnableBitmapPersistence;
            }
            
            return this;
        }
        
        /// <summary>
        /// 建立RDP連線（不自動連接）
        /// </summary>
        public IRdpConnection Build()
        {
            ValidateConfiguration();
            
            var connection = new RdpConnection();
            
            var config = new RdpConfig
            {
                ScreenWidth = _screenWidth,
                ScreenHeight = _screenHeight,
                ColorDepth = _colorDepth,
                Domain = _domain,
                EnableCredSspSupport = _enableCredSspSupport,
                EnableCompression = _enableCompression,
                EnableBitmapPersistence = _enableBitmapPersistence
            };
            
            connection.Configure(config);
            return connection;
        }
        
        /// <summary>
        /// 建立RDP連線並立即連接
        /// </summary>
        public bool BuildAndConnect(out IRdpConnection connection)
        {
            connection = Build();
            return connection.Connect(_hostName, _userName, _password);
        }
        
        /// <summary>
        /// 建立RDP連線並非同步連接
        /// </summary>
        public async Task<IRdpConnection> BuildAndConnectAsync()
        {
            ValidateConfiguration();
            
            var connection = Build();
            bool success = await connection.ConnectAsync(_hostName, _userName, _password, _connectionTimeout);
            
            if (!success)
            {
                connection.Dispose();
                throw new RdpException($"無法連線到主機 {_hostName}", RdpErrorCode.ConnectionTimeout);
            }
            
            return connection;
        }
        
        /// <summary>
        /// 驗證配置完整性
        /// </summary>
        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_hostName))
                throw new RdpException("主機名稱不能為空", RdpErrorCode.InvalidConfiguration);
            
            if (string.IsNullOrWhiteSpace(_userName))
                throw new RdpException("使用者名稱不能為空", RdpErrorCode.InvalidConfiguration);
            
            if (_screenWidth <= 0 || _screenHeight <= 0)
                throw new RdpException("畫面解析度必須大於0", RdpErrorCode.InvalidConfiguration);
            
            if (_colorDepth != 8 && _colorDepth != 15 && _colorDepth != 16 && _colorDepth != 24 && _colorDepth != 32)
                throw new RdpException("無效的顏色深度值", RdpErrorCode.InvalidConfiguration);
        }
        
        /// <summary>
        /// 重置所有設定為預設值
        /// </summary>
        public RdpConnectionBuilder Reset()
        {
            _hostName = null;
            _userName = null;
            _password = null;
            _domain = null;
            _screenWidth = 1920;
            _screenHeight = 1080;
            _colorDepth = 32;
            _enableCredSspSupport = true;
            _enableCompression = true;
            _enableBitmapPersistence = true;
            _connectionTimeout = 30;
            _profile = null;
            return this;
        }
    }
}
