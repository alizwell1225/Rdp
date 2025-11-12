using LIB_RDP.Interfaces;
using LIB_RDP.Models;
using LIB_RDP.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LIB_Log;
using Timer = System.Threading.Timer;

namespace LIB_RDP.Core
{
    /// <summary>
    /// 增強版RDP連線管理器
    /// </summary>
    public class RdpManager : IRdpManager, IDisposable
    {
        private ConcurrentDictionary<string, IRdpConnection> _connections;
        //private readonly RdpLogger _logger;
        private Timer _healthCheckTimer;
        private int _maxConnections;
        private bool _isDisposed;

        //public ProxySettings RdpProxySettings;
        public RdpConfigurationManager ConfigurationManager;
        public Logger _logger;

        /// <summary>
        /// 連線數量限制，預設12個連線
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

        public RdpManager(int maxConnections = 12)
        {
            if (maxConnections==0)
            {
                throw new Exception("Connect Count Error");
            }
            _logger = new Logger(Path.Combine(AppContext.BaseDirectory, "Log"), "RDP");
            //RdpProxySettings = new ProxySettings(maxConnections);
            ConfigurationManager = RdpConfigurationManager.Instance;
            ConfigurationManager.SetLog(_logger);

            _maxConnections = maxConnections;
            //_connections = new ConcurrentDictionary<string, IRdpConnection>();

            //LoadConnectData();
            // 每30秒檢查一次連線健康狀態
            _healthCheckTimer = new Timer(PerformHealthCheck, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            _logger.Info($"RDP管理器已初始化，最大連線數: {maxConnections}");
        }



        public void LoadConnectData()
        {
            var datas  = ConfigurationManager.LoadAllConnections();
            try
            {
                if (_connections==null)
                    _connections = new ConcurrentDictionary<string, IRdpConnection>();
                if (datas == null || datas.Count == 0)
                {
                    for (int i = 0; i < _maxConnections; i++)
                    {
                        var rdpConn = new RdpConnection(logger: _logger);
                        _connections.TryAdd(rdpConn.ConnectionId, rdpConn);
                        var config = new RdpConnectionProfile(i);
                        config.SetRdpConnection(rdpConn);
                        ConfigurationManager.SaveConnection(config);
                        _connections.TryAdd(rdpConn.ConnectionId, rdpConn);
                    }
                }
                else
                {
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var config = datas[i].Config;
                        var rdpConn = new RdpConnection(logger: _logger);
                        rdpConn.Configure(config);
                        rdpConn.LoadConfiguration(datas[i]);
                        _connections.TryAdd(rdpConn.ConnectionId, rdpConn);
                    }
                }
                
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public IRdpConnection GetConnection(int index)
        {
            LoadConnectData();
            var conn = ConfigurationManager.FindConnIndex(index);
            if (conn == null)
                return null;
            if (_connections.ContainsKey(conn.Id))
                return _connections[conn.Id];
            return null;
        }

        public IRdpConnection CreateConnection()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RdpManager));

            if (_connections.Count >= _maxConnections)
            {
                _logger.Warn($"已達到最大連線數限制 ({_maxConnections})，無法建立新連線");
                throw new InvalidOperationException($"已達到最大連線數限制: {_maxConnections}");
            }

            var connection = new RdpConnection();
            var connectionId = connection.ConnectionId;
            
            if (_connections.TryAdd(connectionId, connection))
            {
                _logger.Info($"已建立新的RDP連線 "+ connectionId);
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
                _logger.Info($"正在移除RDP連線", connectionId);
                try
                {
                    (connection as IDisposable)?.Dispose();
                    _logger.Info($"RDP連線已成功移除", connectionId);
                }
                catch (Exception ex)
                {
                    _logger.Error($"移除RDP連線時發生錯誤:{connectionId}", ex);
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
            _logger.Info($"正在斷開所有連線 (共 {_connections?.Count} 個)");
            if (_connections == null)
                return;
            Parallel.ForEach(_connections?.Values, connection =>
            {
                try
                {
                    connection?.Disconnect();
                }
                catch (Exception ex)
                {
                    var connectionId = (connection as RdpConnection)?.ConnectionId;
                    _logger.Error($"斷開連線時發生錯誤:{connectionId}", ex );
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
                _logger.Info($"已清理 {inactiveConnections.Count} 個無效連線");
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
                _logger.Debug($"健康檢查完成 - 總連線: {TotalConnectionCount}, " + $"活動連線: {ActiveConnectionCount}, 已清理: {cleanedCount}");
            }
            catch (Exception ex)
            {
                _logger.Error("健康檢查時發生錯誤", ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _logger.Info($"正在釋放RDP管理器，共有 {_connections?.Count} 個連線");

            try
            {
                _healthCheckTimer?.Dispose();
                DisconnectAll();
                if (_connections == null)
                    return;
                foreach (var connection in _connections?.Values)
                {
                    (connection as IDisposable)?.Dispose();
                }
                _connections.Clear();

                _isDisposed = true;
                _logger.Info("RDP管理器已成功釋放");
            }
            catch (Exception ex)
            {
                _logger.Error("釋放RDP管理器時發生錯誤", ex);
            }
        }
    }
}