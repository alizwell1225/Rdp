using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LIB_RDP.Builders;
using LIB_RDP.Core;
using LIB_RDP.Helpers;
using LIB_RDP.Interfaces;
using LIB_RDP.Models;

namespace RDP_DEMO.Examples
{
    /// <summary>
    /// 展示如何使用增強的 LIB_RDP API 的範例類別
    /// </summary>
    public class EnhancedApiUsageExamples
    {
        private readonly IRdpManager _manager;
        private readonly RdpBatchOperations _batchOps;
        private readonly IRdpConfigurationManager _configManager;

        public EnhancedApiUsageExamples()
        {
            _manager = new RdpManager(maxConnections: 50);
            _batchOps = new RdpBatchOperations(_manager);
            _configManager = RdpConfigurationManager.Instance;
        }

        /// <summary>
        /// 範例1：使用 Builder 模式建立單一連線
        /// </summary>
        public async Task<IRdpConnection> Example1_UseBuilderPattern(string host, string user, string pass)
        {
            try
            {
                // 使用流暢的 Builder API
                var connection = await new RdpConnectionBuilder()
                    .WithHost(host)
                    .WithCredentials(user, pass)
                    .WithResolution(1920, 1080)
                    .WithColorDepth(32)
                    .WithTimeout(30)
                    .WithCompression(true)
                    .BuildAndConnectAsync();

                Console.WriteLine($"成功連線到 {host}");
                return connection;
            }
            catch (RdpException ex)
            {
                Console.WriteLine($"連線失敗 [{ex.ErrorCode}]: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 範例2：批次連線到多個主機
        /// </summary>
        public async Task<List<IRdpConnection>> Example2_BatchConnect(
            List<(string Host, string User, string Pass)> hosts)
        {
            var results = await _batchOps.ConnectMultipleAsync(hosts);
            
            var successfulConnections = new List<IRdpConnection>();
            
            foreach (var result in results)
            {
                if (result.Success)
                {
                    Console.WriteLine($"✓ {result.HostName}: {result.Message}");
                    successfulConnections.Add(result.Connection);
                }
                else
                {
                    Console.WriteLine($"✗ {result.HostName}: {result.Message}");
                }
            }
            
            return successfulConnections;
        }

        /// <summary>
        /// 範例3：監控連線健康狀態
        /// </summary>
        public void Example3_MonitorHealth()
        {
            var healthStatuses = _batchOps.GetHealthStatus();
            
            Console.WriteLine("=== 連線健康狀態 ===");
            foreach (var status in healthStatuses)
            {
                string icon = status.IsHealthy ? "✓" : "✗";
                Console.WriteLine($"{icon} [{status.ConnectionId}] {status.HostName}");
                Console.WriteLine($"   狀態: {status.State}");
                Console.WriteLine($"   連線時長: {status.ConnectedDuration:hh\\:mm\\:ss}");
                Console.WriteLine($"   重試次數: {status.RetryCount}");
            }
            
            // 取得統計摘要
            var summary = _batchOps.GetStatisticsSummary();
            Console.WriteLine($"\n=== 統計摘要 ===\n{summary}");
        }

        /// <summary>
        /// 範例4：使用配置管理器儲存和載入連線設定
        /// </summary>
        public async Task<IRdpConnection> Example4_UseConfigurationManager(string profileName)
        {
            // 載入所有配置
            var profiles = _configManager.LoadAllConnections();
            var profile = profiles.FirstOrDefault(p => p.Name == profileName);
            
            if (profile == null)
            {
                Console.WriteLine($"找不到配置: {profileName}");
                return null;
            }
            
            // 使用配置建立連線
            var credentials = profile.GetSecureCredentials();
            var connection = await new RdpConnectionBuilder()
                .FromProfile(profile)
                .WithCredentials(profile.UserName, credentials.GetPassword())
                .BuildAndConnectAsync();
            
            // 更新使用統計
            profile.UpdateUsageStats();
            _configManager.SaveConnection(profile);
            
            return connection;
        }

        /// <summary>
        /// 範例5：建立和儲存新的連線配置
        /// </summary>
        public void Example5_CreateAndSaveProfile(
            string name, string host, string user, string pass, string group = "預設群組")
        {
            var profile = new RdpConnectionProfile
            {
                Name = name,
                HostName = host,
                UserName = user,
                GroupName = group,
                Config = new RdpConfig
                {
                    ScreenWidth = 1920,
                    ScreenHeight = 1080,
                    ColorDepth = 32,
                    EnableCompression = true,
                    EnableBitmapPersistence = true
                }
            };
            
            // 設定加密的憑證
            profile.SetCredentials(user, pass);
            
            // 儲存到本地
            _configManager.SaveConnection(profile);
            
            Console.WriteLine($"已儲存配置: {name}");
        }

        /// <summary>
        /// 範例6：使用事件處理連線狀態變化
        /// </summary>
        public async Task Example6_UseEvents(string host, string user, string pass)
        {
            var connection = _manager.CreateConnection();
            
            // 註冊狀態變更事件
            connection.ConnectionStateChanged += (sender, e) =>
            {
                var conn = sender as IRdpConnection;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 狀態變更: {conn.State}");
                
                switch (conn.State)
                {
                    case RdpState.Connecting:
                        Console.WriteLine("正在連線...");
                        break;
                    case RdpState.Connected:
                        Console.WriteLine("連線成功！");
                        break;
                    case RdpState.Retrying:
                        Console.WriteLine($"重試中 (第 {conn.RetryCount} 次)...");
                        break;
                    case RdpState.Error:
                        Console.WriteLine("連線錯誤");
                        break;
                }
            };
            
            // 註冊超時事件
            connection.ConnectionTimeoutOccurred += (sender, ex) =>
            {
                Console.WriteLine($"連線超時: {ex.TimeoutSeconds} 秒");
            };
            
            // 配置並連線
            connection.Configure(new RdpConfig { ScreenWidth = 1920, ScreenHeight = 1080 });
            await connection.ConnectAsync(host, user, pass);
        }

        /// <summary>
        /// 範例7：查詢特定主機的所有連線
        /// </summary>
        public void Example7_FindConnectionsByHost(string hostName)
        {
            var connections = _manager.GetConnectionsByHost(hostName);
            
            Console.WriteLine($"主機 {hostName} 的連線：");
            foreach (var conn in connections)
            {
                var stats = conn.GetConnectionStats();
                Console.WriteLine($"  - 連線ID: {conn.ConnectionId}");
                Console.WriteLine($"    狀態: {conn.State}");
                Console.WriteLine($"    連線時長: {stats.ConnectedDuration:hh\\:mm\\:ss}");
            }
        }

        /// <summary>
        /// 範例8：定期清理無效連線
        /// </summary>
        public void Example8_PeriodicCleanup()
        {
            // 建立定時器，每5分鐘清理一次
            var timer = new System.Timers.Timer(300000); // 5分鐘
            
            timer.Elapsed += (sender, e) =>
            {
                Console.WriteLine($"[{DateTime.Now}] 執行健康檢查...");
                
                // 清理無效連線
                int cleanedCount = _manager.CleanupInactiveConnections();
                if (cleanedCount > 0)
                {
                    Console.WriteLine($"已清理 {cleanedCount} 個無效連線");
                }
                
                // 顯示統計
                var summary = _batchOps.GetStatisticsSummary();
                Console.WriteLine(summary);
            };
            
            timer.Start();
            Console.WriteLine("已啟動定期清理任務");
        }

        /// <summary>
        /// 範例9：批次重新配置連線設定
        /// </summary>
        public void Example9_BatchReconfigure()
        {
            // 建立新的配置
            var newConfig = new RdpConfig
            {
                ScreenWidth = 2560,
                ScreenHeight = 1440,
                ColorDepth = 32,
                EnableCompression = true
            };
            
            // 套用到所有連線
            int configuredCount = _batchOps.ReconfigureConnections(newConfig);
            Console.WriteLine($"已重新配置 {configuredCount} 個連線");
        }

        /// <summary>
        /// 範例10：匯出和匯入連線配置
        /// </summary>
        public void Example10_ExportImportConfigurations()
        {
            // 匯出當前所有配置
            string exportPath = "rdp_connections_backup.json";
            _configManager.ExportConnections(exportPath);
            Console.WriteLine($"已匯出配置到: {exportPath}");
            
            // 從檔案匯入配置
            string importPath = "rdp_connections_backup.json";
            int importedCount = _configManager.ImportConnections(importPath);
            Console.WriteLine($"已匯入 {importedCount} 個配置");
        }

        /// <summary>
        /// 完整的示範：整合所有功能
        /// </summary>
        public async Task CompleteDemo()
        {
            Console.WriteLine("=== LIB_RDP 增強 API 完整示範 ===\n");
            
            // 1. 建立連線配置
            Example5_CreateAndSaveProfile("測試主機1", "192.168.1.100", "admin", "pass1", "測試群組");
            Example5_CreateAndSaveProfile("測試主機2", "192.168.1.101", "admin", "pass2", "測試群組");
            
            // 2. 批次連線
            var hosts = new List<(string, string, string)>
            {
                ("192.168.1.100", "admin", "pass1"),
                ("192.168.1.101", "admin", "pass2")
            };
            await Example2_BatchConnect(hosts);
            
            // 3. 監控健康狀態
            Example3_MonitorHealth();
            
            // 4. 啟動定期清理
            Example8_PeriodicCleanup();
            
            Console.WriteLine("\n示範完成！");
        }

        /// <summary>
        /// 清理資源
        /// </summary>
        public void Cleanup()
        {
            _manager.DisconnectAll();
            (_manager as IDisposable)?.Dispose();
        }
    }
}
