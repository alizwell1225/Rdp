using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LIB_RDP.Core;
using LIB_RDP.Interfaces;
using LIB_RDP.Models;

namespace LIB_RDP.Helpers
{
    /// <summary>
    /// RDP批次操作輔助類別，提供對多個連線的批次操作功能
    /// </summary>
    public class RdpBatchOperations
    {
        private readonly IRdpManager _manager;
        
        /// <summary>
        /// 建立批次操作實例
        /// </summary>
        /// <param name="manager">RDP管理器</param>
        public RdpBatchOperations(IRdpManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }
        
        /// <summary>
        /// 批次連線到多個主機
        /// </summary>
        /// <param name="connectionInfos">連線資訊清單</param>
        /// <param name="maxConcurrency">最大並發連線數（預設5）</param>
        /// <param name="progress">進度報告回呼（選填）</param>
        /// <param name="cancellationToken">取消令牌（選填）</param>
        /// <returns>連線結果清單</returns>
        public async Task<List<BatchConnectionResult>> ConnectMultipleAsync(
            List<(string HostName, string UserName, string Password)> connectionInfos,
            int maxConcurrency = 5,
            IProgress<BatchProgress> progress = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            if (connectionInfos == null || connectionInfos.Count == 0)
                throw new ArgumentException("連線資訊清單不能為空", nameof(connectionInfos));
                
            if (maxConcurrency <= 0 || maxConcurrency > 50)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "最大並發數必須在1到50之間");
            
            var logger = RdpLogger.Instance;
            logger.LogInfo($"開始批次連線，共 {connectionInfos.Count} 個主機，最大並發: {maxConcurrency}");
            
            var results = new List<BatchConnectionResult>();
            var semaphore = new System.Threading.SemaphoreSlim(maxConcurrency, maxConcurrency);
            var completed = 0;
            var tasks = new List<Task<BatchConnectionResult>>();
            
            foreach (var info in connectionInfos)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                    
                var task = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        var connection = await _manager.CreateAndConnectAsync(
                            info.HostName, info.UserName, info.Password);
                        
                        var result = new BatchConnectionResult
                        {
                            HostName = info.HostName,
                            Success = true,
                            Connection = connection,
                            Message = "連線成功"
                        };
                        
                        System.Threading.Interlocked.Increment(ref completed);
                        progress?.Report(new BatchProgress
                        {
                            TotalCount = connectionInfos.Count,
                            CompletedCount = completed,
                            CurrentHost = info.HostName,
                            IsSuccess = true
                        });
                        
                        return result;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"批次連線失敗: {info.HostName} - {ex.Message}");
                        
                        var result = new BatchConnectionResult
                        {
                            HostName = info.HostName,
                            Success = false,
                            Message = $"連線失敗: {ex.Message}",
                            Error = ex
                        };
                        
                        System.Threading.Interlocked.Increment(ref completed);
                        progress?.Report(new BatchProgress
                        {
                            TotalCount = connectionInfos.Count,
                            CompletedCount = completed,
                            CurrentHost = info.HostName,
                            IsSuccess = false,
                            ErrorMessage = ex.Message
                        });
                        
                        return result;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);
                
                tasks.Add(task);
            }
            
            results.AddRange(await Task.WhenAll(tasks));
            
            logger.LogInfo($"批次連線完成，成功: {results.Count(r => r.Success)}, 失敗: {results.Count(r => !r.Success)}");
            
            return results;
        }
        
        /// <summary>
        /// 批次斷開指定主機的所有連線
        /// </summary>
        /// <param name="hostNames">主機名稱清單</param>
        /// <returns>成功斷開的連線數</returns>
        public int DisconnectByHosts(List<string> hostNames)
        {
            int disconnectedCount = 0;
            var logger = RdpLogger.Instance;
            
            foreach (var hostName in hostNames)
            {
                var connections = _manager.GetConnectionsByHost(hostName);
                foreach (var connection in connections)
                {
                    try
                    {
                        connection.Disconnect();
                        disconnectedCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"斷開連線 {hostName} 時發生錯誤: {ex.Message}", connection.ConnectionId);
                    }
                }
            }
            
            return disconnectedCount;
        }
        
        /// <summary>
        /// 取得所有連線的健康狀態
        /// </summary>
        /// <returns>連線健康狀態清單</returns>
        public List<ConnectionHealthStatus> GetHealthStatus()
        {
            var healthStatuses = new List<ConnectionHealthStatus>();
            
            foreach (var connection in _manager.AllConnections)
            {
                var stats = connection.GetConnectionStats();
                var health = new ConnectionHealthStatus
                {
                    ConnectionId = connection.ConnectionId,
                    HostName = connection.GetHostName(),
                    State = connection.State,
                    IsHealthy = connection.IsConnected && connection.State == RdpState.Connected,
                    ConnectedDuration = stats.ConnectedDuration,
                    RetryCount = connection.RetryCount
                };
                
                healthStatuses.Add(health);
            }
            
            return healthStatuses;
        }
        
        /// <summary>
        /// 批次重新配置連線設定
        /// </summary>
        /// <param name="config">要套用的配置</param>
        /// <param name="connectionIds">要配置的連線ID清單，若為null則套用到所有連線</param>
        /// <returns>成功配置的連線數</returns>
        public int ReconfigureConnections(RdpConfig config, List<string> connectionIds = null)
        {
            int configuredCount = 0;
            var logger = RdpLogger.Instance;
            var connections = connectionIds != null
                ? _manager.AllConnections.Where(c => connectionIds.Contains(c.ConnectionId))
                : _manager.AllConnections;
            
            foreach (var connection in connections)
            {
                try
                {
                    connection.Configure(config);
                    configuredCount++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"配置連線 {connection.ConnectionId} 時發生錯誤: {ex.Message}", connection.ConnectionId);
                }
            }
            
            return configuredCount;
        }
        
        /// <summary>
        /// 取得連線統計摘要
        /// </summary>
        /// <returns>統計摘要</returns>
        public BatchStatisticsSummary GetStatisticsSummary()
        {
            var allStats = _manager.GetConnectionStats().ToList();
            
            return new BatchStatisticsSummary
            {
                TotalConnections = _manager.TotalConnectionCount,
                ActiveConnections = _manager.ActiveConnectionCount,
                DisconnectedConnections = allStats.Count(s => s.State == RdpState.Disconnected),
                ErrorConnections = allStats.Count(s => s.State == RdpState.Error),
                RetryingConnections = allStats.Count(s => s.State == RdpState.Retrying),
                AverageConnectionDuration = allStats.Any() 
                    ? TimeSpan.FromSeconds(allStats.Average(s => s.ConnectedDuration.TotalSeconds))
                    : TimeSpan.Zero
            };
        }
    }
    
    /// <summary>
    /// 批次連線結果
    /// </summary>
    public class BatchConnectionResult
    {
        public string HostName { get; set; }
        public bool Success { get; set; }
        public IRdpConnection Connection { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }
    }
    
    /// <summary>
    /// 連線健康狀態
    /// </summary>
    public class ConnectionHealthStatus
    {
        public string ConnectionId { get; set; }
        public string HostName { get; set; }
        public RdpState State { get; set; }
        public bool IsHealthy { get; set; }
        public TimeSpan ConnectedDuration { get; set; }
        public int RetryCount { get; set; }
    }
    
    /// <summary>
    /// 批次統計摘要
    /// </summary>
    public class BatchStatisticsSummary
    {
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public int DisconnectedConnections { get; set; }
        public int ErrorConnections { get; set; }
        public int RetryingConnections { get; set; }
        public TimeSpan AverageConnectionDuration { get; set; }
        
        public override string ToString()
        {
            return $"總連線: {TotalConnections}, 活動: {ActiveConnections}, " +
                   $"已斷線: {DisconnectedConnections}, 錯誤: {ErrorConnections}, " +
                   $"重試中: {RetryingConnections}, 平均連線時長: {AverageConnectionDuration:hh\\:mm\\:ss}";
        }
    }
    
    /// <summary>
    /// 批次操作進度資訊
    /// </summary>
    public class BatchProgress
    {
        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 已完成數量
        /// </summary>
        public int CompletedCount { get; set; }
        
        /// <summary>
        /// 當前處理的主機
        /// </summary>
        public string CurrentHost { get; set; }
        
        /// <summary>
        /// 當前操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 錯誤訊息（如果失敗）
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// 完成百分比（0-100）
        /// </summary>
        public double PercentageComplete => TotalCount > 0 
            ? (double)CompletedCount / TotalCount * 100 
            : 0;
        
        public override string ToString()
        {
            return $"進度: {CompletedCount}/{TotalCount} ({PercentageComplete:F1}%) - 當前: {CurrentHost}";
        }
    }
}
