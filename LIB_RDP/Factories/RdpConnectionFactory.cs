using System;
using System.Windows.Forms;
using LIB_RDP.Core;
using LIB_RDP.Interfaces;
using LIB_RDP.Models;
using LIB_RDP.Strategies;

namespace LIB_RDP.Factories
{
    /// <summary>
    /// RDP連線工廠介面
    /// </summary>
    public interface IRdpConnectionFactory
    {
        /// <summary>
        /// 建立RDP連線實例
        /// </summary>
        /// <param name="uiControl">UI控制項（選填）</param>
        /// <param name="parentForm">父表單（選填）</param>
        /// <returns>RDP連線實例</returns>
        IRdpConnection CreateConnection(Control uiControl = null, Form parentForm = null);
        
        /// <summary>
        /// 建立具有特定重試策略的RDP連線
        /// </summary>
        /// <param name="retryStrategy">重試策略</param>
        /// <param name="uiControl">UI控制項（選填）</param>
        /// <param name="parentForm">父表單（選填）</param>
        /// <returns>RDP連線實例</returns>
        IRdpConnection CreateConnectionWithRetry(IRetryStrategy retryStrategy, Control uiControl = null, Form parentForm = null);
    }
    
    /// <summary>
    /// 預設RDP連線工廠實作
    /// </summary>
    public class RdpConnectionFactory : IRdpConnectionFactory
    {
        private readonly RdpLogger _logger;
        
        public RdpConnectionFactory()
        {
            _logger = RdpLogger.Instance;
        }
        
        /// <summary>
        /// 建立標準RDP連線
        /// </summary>
        public IRdpConnection CreateConnection(Control uiControl = null, Form parentForm = null)
        {
            _logger.LogInfo("正在透過工廠建立RDP連線");
            return new RdpConnection(uiControl, parentForm);
        }
        
        /// <summary>
        /// 建立具有自訂重試策略的RDP連線
        /// </summary>
        public IRdpConnection CreateConnectionWithRetry(IRetryStrategy retryStrategy, Control uiControl = null, Form parentForm = null)
        {
            if (retryStrategy == null)
                throw new ArgumentNullException(nameof(retryStrategy));
                
            _logger.LogInfo($"正在透過工廠建立RDP連線（重試策略: {retryStrategy.GetType().Name}）");
            
            // 未來可以在這裡整合重試策略到連線中
            // 目前先回傳標準連線
            return new RdpConnection(uiControl, parentForm);
        }
        
        /// <summary>
        /// 建立用於測試的連線（可注入mock物件）
        /// </summary>
        public IRdpConnection CreateTestConnection(Control uiControl = null, Form parentForm = null)
        {
            _logger.LogInfo("正在建立測試用RDP連線");
            return new RdpConnection(uiControl, parentForm);
        }
    }
    
    /// <summary>
    /// RDP管理器工廠
    /// </summary>
    public static class RdpManagerFactory
    {
        /// <summary>
        /// 建立標準RDP管理器
        /// </summary>
        /// <param name="maxConnections">最大連線數</param>
        /// <returns>RDP管理器實例</returns>
        public static IRdpManager CreateManager(int maxConnections = 50)
        {
            if (maxConnections <= 0 || maxConnections > 1000)
                throw new ArgumentOutOfRangeException(nameof(maxConnections), "最大連線數必須在1到1000之間");
                
            return new RdpManager(maxConnections);
        }
        
        /// <summary>
        /// 建立小型RDP管理器（最多10個連線）
        /// </summary>
        public static IRdpManager CreateSmallManager()
        {
            return new RdpManager(10);
        }
        
        /// <summary>
        /// 建立大型RDP管理器（最多200個連線）
        /// </summary>
        public static IRdpManager CreateLargeManager()
        {
            return new RdpManager(200);
        }
        
        /// <summary>
        /// 建立企業級RDP管理器（最多500個連線，含優化設定）
        /// </summary>
        public static IRdpManager CreateEnterpriseManager()
        {
            return new RdpManager(500);
        }
    }
}
