using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LIB_RDP.Interfaces;
using LIB_RDP.Models;

namespace LIB_RDP.Core
{
    /// <summary>
    /// 增強版RDP連線管理器
    /// </summary>
    public class RdpManager : IRdpManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, IRdpConnection> _connections;
        private readonly RdpLogger _logger;
        private readonly Timer _healthCheckTimer;
        private readonly int _maxConnections;
        private bool _isDisposed;

        /// <summary>
        /// 連線數量限制，預設50個連線
        /// </summary>
        public int MaxConnections => _maxConnections;

        /// <summary>
        /// 目前活動連線數
        /// </summary>
        public int ActiveConnectionCount => _connections.Count(c => c.Value.IsConnected);

        /// <summary>
        /// 總連線數
        /// </summary>
        public int TotalConnectionCount => _connections.Count;

        public RdpManager(int maxConnections = 50)
        {
            _maxConnections = maxConnections;
            _connections = new ConcurrentDictionary<string, IRdpConnection>();
            _logger = RdpLogger.Instance;
            
            // 每30秒檢查一次連線健康狀態
            _healthCheckTimer = new Timer(PerformHealthCheck, null, 
                TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
                
            _logger.LogInfo($"RDP管理器已初始化，最大連線數: {maxConnections}");
        }

        public IRdpConnection CreateConnection()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RdpManager));

            if (_connections.Count >= _maxConnections)
            {
                _logger.LogWarning($"已達到最大連線數限制 ({_maxConnections})，無法建立新連線");
                throw new InvalidOperationException($"已達到最大連線數限制: {_maxConnections}");
            }

            var connection = new RdpConnection();
            var connectionId = connection.ConnectionId;
            
            if (_connections.TryAdd(connectionId, connection))
            {
                _logger.LogInfo($"已建立新的RDP連線", connectionId);
                return connection;
            }
            else
            {
                connection.Dispose();
                throw new InvalidOperationException("建立連線失敗：連線ID衝突");
            }
        }

        /// <summary>
        /// 建立連線並立即連接到指定主機
        /// </summary>
        public async Task<IRdpConnection> CreateAndConnectAsync(string hostName, string userName, string password, int timeoutSeconds = 30)
        {
            var connection = CreateConnection() as RdpConnection;
            try
            {
                bool success = await connection.ConnectAsync(hostName, userName, password, timeoutSeconds);
                if (!success)
                {
                    RemoveConnection(connection.ConnectionId);
                    throw new RdpConnectionTimeoutException(timeoutSeconds, connection.ConnectionId);
                }
                return connection;
            }
            catch
            {
                RemoveConnection(connection.ConnectionId);
                throw;
            }
        }

        public void RemoveConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                return;

            if (_connections.TryRemove(connectionId, out var connection))
            {
                _logger.LogInfo($"正在移除RDP連線", connectionId);
                try
                {
                    (connection as IDisposable)?.Dispose();
                    _logger.LogInfo($"RDP連線已成功移除", connectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"移除RDP連線時發生錯誤", ex, connectionId);
                }
            }
        }

        /// <summary>
        /// 根據主機名稱查找連線
        /// </summary>
        public IEnumerable<IRdpConnection> GetConnectionsByHost(string hostName)
        {
            return _connections.Values.Where(c => 
                (c as RdpConnection)?.GetHostName()?.Equals(hostName, StringComparison.OrdinalIgnoreCase) == true);
        }

        /// <summary>
        /// 獲取連線統計資訊
        /// </summary>
        public IEnumerable<RdpConnectionStats> GetConnectionStats()
        {
            return _connections.Values
                .OfType<RdpConnection>()
                .Select(c => c.GetConnectionStats());
        }

        /// <summary>
        /// 斷開所有連線
        /// </summary>
        public void DisconnectAll()
        {
            _logger.LogInfo($"正在斷開所有連線 (共 {_connections.Count} 個)");
            
            Parallel.ForEach(_connections.Values, connection =>
            {
                try
                {
                    connection.Disconnect();
                }
                catch (Exception ex)
                {
                    var connectionId = (connection as RdpConnection)?.ConnectionId;
                    _logger.LogError($"斷開連線時發生錯誤", ex, connectionId);
                }
            });
        }

        /// <summary>
        /// 清理無效連線
        /// </summary>
        public int CleanupInactiveConnections()
        {
            var inactiveConnections = _connections
                .Where(kvp => kvp.Value.State == RdpState.Error || kvp.Value.State == RdpState.Disconnected)
                .ToList();

            foreach (var kvp in inactiveConnections)
            {
                RemoveConnection(kvp.Key);
            }

            if (inactiveConnections.Any())
            {
                _logger.LogInfo($"已清理 {inactiveConnections.Count} 個無效連線");
            }

            return inactiveConnections.Count;
        }

        public IEnumerable<IRdpConnection> ActiveConnections => 
            _connections.Values.Where(c => c.IsConnected);

        /// <summary>
        /// 所有連線（包括非活動狀態）
        /// </summary>
        public IEnumerable<IRdpConnection> AllConnections => _connections.Values;

        /// <summary>
        /// 執行健康檢查
        /// </summary>
        private void PerformHealthCheck(object state)
        {
            try
            {
                int cleanedCount = CleanupInactiveConnections();
                
                // 記錄統計資訊
                _logger.LogDebug($"健康檢查完成 - 總連線: {TotalConnectionCount}, " +
                               $"活動連線: {ActiveConnectionCount}, 已清理: {cleanedCount}");

                // 清理舊日誌
                _logger.CleanOldLogs();
            }
            catch (Exception ex)
            {
                _logger.LogError("健康檢查時發生錯誤", ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _logger.LogInfo($"正在釋放RDP管理器，共有 {_connections.Count} 個連線");

            try
            {
                _healthCheckTimer?.Dispose();
                DisconnectAll();

                foreach (var connection in _connections.Values)
                {
                    (connection as IDisposable)?.Dispose();
                }
                _connections.Clear();

                _isDisposed = true;
                _logger.LogInfo("RDP管理器已成功釋放");
            }
            catch (Exception ex)
            {
                _logger.LogError("釋放RDP管理器時發生錯誤", ex);
            }
        }
    }
}