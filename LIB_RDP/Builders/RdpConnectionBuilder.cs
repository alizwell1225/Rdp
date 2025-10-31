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
        /// <param name="hostName">主機名稱或IP位址</param>
        /// <returns>建構器實例</returns>
        /// <exception cref="ArgumentException">當主機名稱為空時拋出</exception>
        public RdpConnectionBuilder WithHost(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("主機名稱不能為空", nameof(hostName));
                
            _hostName = hostName;
            return this;
        }
        
        /// <summary>
        /// 設定使用者憑證
        /// </summary>
        /// <param name="userName">使用者名稱</param>
        /// <param name="password">密碼</param>
        /// <param name="domain">網域（選填）</param>
        /// <returns>建構器實例</returns>
        /// <exception cref="ArgumentException">當使用者名稱為空時拋出</exception>
        public RdpConnectionBuilder WithCredentials(string userName, string password, string domain = "")
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("使用者名稱不能為空", nameof(userName));
                
            _userName = userName;
            _password = password ?? string.Empty;
            _domain = domain ?? string.Empty;
            return this;
        }
        
        /// <summary>
        /// 設定畫面解析度
        /// </summary>
        /// <param name="width">寬度（像素）</param>
        /// <param name="height">高度（像素）</param>
        /// <returns>建構器實例</returns>
        /// <exception cref="ArgumentOutOfRangeException">當解析度超出有效範圍時拋出</exception>
        public RdpConnectionBuilder WithResolution(int width, int height)
        {
            if (width <= 0 || width > 4096)
                throw new ArgumentOutOfRangeException(nameof(width), "寬度必須在1到4096之間");
            if (height <= 0 || height > 2160)
                throw new ArgumentOutOfRangeException(nameof(height), "高度必須在1到2160之間");
                
            _screenWidth = width;
            _screenHeight = height;
            return this;
        }
        
        /// <summary>
        /// 設定顏色深度
        /// </summary>
        /// <param name="depth">顏色深度（8, 15, 16, 24, 或 32 位元）</param>
        /// <returns>建構器實例</returns>
        /// <exception cref="ArgumentException">當顏色深度無效時拋出</exception>
        public RdpConnectionBuilder WithColorDepth(int depth)
        {
            if (depth != 8 && depth != 15 && depth != 16 && depth != 24 && depth != 32 && depth != 64)
                throw new ArgumentException("顏色深度必須是 8, 15, 16, 24, 32 或 64", nameof(depth));
                
            _colorDepth = depth;
            return this;
        }
        
        /// <summary>
        /// 設定連線超時時間（秒）
        /// </summary>
        /// <param name="timeoutSeconds">超時秒數（5-300秒）</param>
        /// <returns>建構器實例</returns>
        /// <exception cref="ArgumentOutOfRangeException">當超時值超出有效範圍時拋出</exception>
        public RdpConnectionBuilder WithTimeout(int timeoutSeconds)
        {
            if (timeoutSeconds < 5 || timeoutSeconds > 300)
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "連線超時必須在5到300秒之間");
                
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
