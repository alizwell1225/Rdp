using System;
using LIB_RDP.Core;

namespace LIB_RDP.Models
{
    /// <summary>
    /// RDP連線配置檔案
    /// </summary>
    [Serializable]
    public class RdpConnectionProfile
    {        
        /// <summary>
        /// 排序Index
        /// </summary>
        public int Index { get; set; } = -1;
        /// <summary>
        /// 配置ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 主機地址
        /// </summary>
        public string HostName { get; set; }
        
        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 加密後的憑證字符串
        /// </summary>
        public string EncryptedCredentials { get; set; } = "";
        
        /// <summary>
        /// 連線配置
        /// </summary>
        public RdpConfig Config { get; set; } = new RdpConfig();
        
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 最後修改時間
        /// </summary>
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 最後使用時間
        /// </summary>
        public DateTime? LastUsedTime { get; set; }

        /// <summary>
        /// 使用次數
        /// </summary>
        public int UseCount { get; set; } = 0;

        /// <summary>
        /// 是否為收藏項目
        /// </summary>
        public bool IsFavorite { get; set; } = false;
        
        /// <summary>
        /// 群組名稱
        /// </summary>
        public string GroupName { get; set; } = "預設群組";

        /// <summary>
        /// 備註
        /// </summary>
        public string Notes { get; set; } = "";

        /// <summary>
        /// 是否自動連線
        /// </summary>
        public bool AutoConnect { get; set; } = false;
        
        /// <summary>
        /// 連線超時設定（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;
        
        // XmlSerializer 需要這個無參數建構子
        public RdpConnectionProfile()
        {
        }

        public RdpConnectionProfile(int index)
        {
            Index = index;
        }

        /// <summary>
        /// 從加密憑證建立SecureCredentials
        /// </summary>
        public SecureCredentials GetSecureCredentials()
        {
            if (string.IsNullOrEmpty(EncryptedCredentials))
                return null;
                
            return SecureCredentials.FromEncryptedString(EncryptedCredentials);
        }
        
        /// <summary>
        /// 設定安全憑證
        /// </summary>
        public void SetCredentials(string userName, string password, string domain = "")
        {
            UserName = userName;
            using (var credentials = new SecureCredentials(userName, password, domain))
            {
                EncryptedCredentials = credentials.ToEncryptedString();
            }
            LastModifiedTime = DateTime.Now;
        }
        
        /// <summary>
        /// 更新使用統計
        /// </summary>
        public void UpdateUsageStats()
        {
            UseCount++;
            LastUsedTime = DateTime.Now;
        }
        
        /// <summary>
        /// 驗證配置完整性
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(HostName) && 
                   !string.IsNullOrWhiteSpace(UserName) && 
                   !string.IsNullOrWhiteSpace(EncryptedCredentials);
        }
        
        public override string ToString()
        {
            return $"{Name} ({HostName})";
        }
        
        /// <summary>
        /// 複製配置（不包含統計資訊）
        /// </summary>
        public RdpConnectionProfile Clone()
        {
            return new RdpConnectionProfile(this.Index)
            {
                Index = Index,
                Id = Guid.NewGuid().ToString(),
                Name = Name + " (副本)",
                HostName = HostName,
                UserName = UserName,
                EncryptedCredentials = EncryptedCredentials,
                Config = new RdpConfig
                {
                    ScreenWidth = Config.ScreenWidth,
                    ScreenHeight = Config.ScreenHeight,
                    ColorDepth = Config.ColorDepth,
                    Domain = Config.Domain,
                    EnableCompression = Config.EnableCompression,
                    EnableBitmapPersistence = Config.EnableBitmapPersistence,
                    EnableCredSspSupport = Config.EnableCredSspSupport,
                    SmartSize = Config.SmartSize
                },
                GroupName = GroupName,
                Notes = Notes,
                AutoConnect = AutoConnect,
                ConnectionTimeout = ConnectionTimeout
            };
        }

        public void SetRdpConnection(RdpConnection rdpConn)
        {
            Id = rdpConn.ConnectionId;
            HostName= rdpConn.GetHostName();
            UserName = rdpConn.GetUserName();
            EncryptedCredentials = rdpConn.GetPassword();
        }

        public RdpConnection GetRdpConnection()
        {
            var rdpConn = new RdpConnection();
            rdpConn.SetConnectionId(Id);
            return rdpConn;
        }
    }
}