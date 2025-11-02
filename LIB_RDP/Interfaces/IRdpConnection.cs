using System;
using System.Drawing;
using System.Threading.Tasks;
using LIB_RDP.Models;

namespace LIB_RDP.Interfaces
{
    /// <summary>
    /// RDP連線介面
    /// </summary>
    public interface IRdpConnection : IDisposable
    {
        /// <summary>
        /// 同步連線到遠端主機
        /// </summary>
        /// <param name="hostName">主機名稱或IP位址</param>
        /// <param name="userName">使用者名稱</param>
        /// <param name="password">密碼</param>
        /// <returns>是否成功發起連線請求</returns>
        bool Connect(string hostName, string userName, string password);
        
        /// <summary>
        /// 非同步連線到遠端主機，等待連線驗證完成
        /// </summary>
        /// <param name="hostName">主機名稱或IP位址</param>
        /// <param name="userName">使用者名稱</param>
        /// <param name="password">密碼</param>
        /// <param name="timeoutSeconds">連線超時秒數</param>
        /// <returns>是否成功連線並通過驗證</returns>
        Task<bool> ConnectAsync(string hostName, string userName, string password, int timeoutSeconds = 30);
        
        /// <summary>
        /// 斷開連線
        /// </summary>
        void Disconnect();
        
        /// <summary>
        /// 設定連線配置
        /// </summary>
        /// <param name="config">RDP配置</param>
        void Configure(RdpConfig config);
        
        /// <summary>
        /// 取得連線配置
        /// </summary>
        /// <returns>目前的RDP配置</returns>
        RdpConfig GetRdpConfig();
        
        /// <summary>
        /// 取得連線統計資訊
        /// </summary>
        /// <returns>連線統計資訊</returns>
        RdpConnectionStats GetConnectionStats();
        
        /// <summary>
        /// 取得主機名稱
        /// </summary>
        /// <returns>主機名稱</returns>
        string GetHostName();
        
        /// <summary>
        /// 取得使用者名稱
        /// </summary>
        /// <returns>使用者名稱</returns>
        string GetUserName();
        
        /// <summary>
        /// 是否已連線
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// 連線狀態
        /// </summary>
        RdpState State { get; }
        
        /// <summary>
        /// 連線ID
        /// </summary>
        string ConnectionId { get; }
        
        /// <summary>
        /// 目前重試次數
        /// </summary>
        int RetryCount { get; }
        
        /// <summary>
        /// 是否正在重試
        /// </summary>
        bool IsRetrying { get; }
        
        /// <summary>
        /// 擷取遠端畫面截圖
        /// </summary>
        /// <returns>畫面截圖的Bitmap，若失敗則回傳null</returns>
        Bitmap GetScreenshot();
        
        /// <summary>
        /// 連線狀態變更事件
        /// </summary>
        event EventHandler ConnectionStateChanged;
        
        /// <summary>
        /// 連線超時事件
        /// </summary>
        event EventHandler<RdpConnectionTimeoutException> ConnectionTimeoutOccurred;
    }
} 