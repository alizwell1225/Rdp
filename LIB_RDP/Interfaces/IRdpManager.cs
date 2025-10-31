using System.Collections.Generic;
using System.Threading.Tasks;
using LIB_RDP.Models;

namespace LIB_RDP.Interfaces
{
    /// <summary>
    /// RDP連線管理器介面
    /// </summary>
    public interface IRdpManager
    {
        /// <summary>
        /// 建立新的RDP連線
        /// </summary>
        /// <returns>RDP連線實例</returns>
        IRdpConnection CreateConnection();
        
        /// <summary>
        /// 建立連線並立即連接到指定主機
        /// </summary>
        /// <param name="hostName">主機名稱或IP位址</param>
        /// <param name="userName">使用者名稱</param>
        /// <param name="password">密碼</param>
        /// <param name="timeoutSeconds">連線超時秒數</param>
        /// <returns>已連接的RDP連線</returns>
        Task<IRdpConnection> CreateAndConnectAsync(string hostName, string userName, string password, int timeoutSeconds = 30);
        
        /// <summary>
        /// 根據連線ID移除連線
        /// </summary>
        /// <param name="connectionId">連線ID</param>
        void RemoveConnection(string connectionId);
        
        /// <summary>
        /// 根據主機名稱查找連線
        /// </summary>
        /// <param name="hostName">主機名稱</param>
        /// <returns>符合條件的連線集合</returns>
        IEnumerable<IRdpConnection> GetConnectionsByHost(string hostName);
        
        /// <summary>
        /// 獲取所有連線的統計資訊
        /// </summary>
        /// <returns>連線統計資訊集合</returns>
        IEnumerable<RdpConnectionStats> GetConnectionStats();
        
        /// <summary>
        /// 斷開所有連線
        /// </summary>
        void DisconnectAll();
        
        /// <summary>
        /// 清理無效連線
        /// </summary>
        /// <returns>清理的連線數量</returns>
        int CleanupInactiveConnections();
        
        /// <summary>
        /// 目前活動中的連線集合
        /// </summary>
        IEnumerable<IRdpConnection> ActiveConnections { get; }
        
        /// <summary>
        /// 所有連線（包括非活動狀態）
        /// </summary>
        IEnumerable<IRdpConnection> AllConnections { get; }
        
        /// <summary>
        /// 連線數量限制
        /// </summary>
        int MaxConnections { get; }
        
        /// <summary>
        /// 目前活動連線數
        /// </summary>
        int ActiveConnectionCount { get; }
        
        /// <summary>
        /// 總連線數
        /// </summary>
        int TotalConnectionCount { get; }
    }
} 