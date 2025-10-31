using System;
using System.Collections.Generic;
using LIB_RDP.Interfaces;
using LIB_RDP.Models;

namespace LIB_RDP.Observers
{
    /// <summary>
    /// 連線狀態觀察者介面
    /// </summary>
    public interface IConnectionObserver
    {
        /// <summary>
        /// 當連線狀態改變時通知
        /// </summary>
        /// <param name="connection">RDP連線</param>
        /// <param name="oldState">舊狀態</param>
        /// <param name="newState">新狀態</param>
        void OnConnectionStateChanged(IRdpConnection connection, RdpState oldState, RdpState newState);
        
        /// <summary>
        /// 當連線超時時通知
        /// </summary>
        /// <param name="connection">RDP連線</param>
        /// <param name="exception">超時例外</param>
        void OnConnectionTimeout(IRdpConnection connection, RdpConnectionTimeoutException exception);
    }
    
    /// <summary>
    /// 連線狀態主題（可被觀察的對象）
    /// </summary>
    public interface IConnectionSubject
    {
        /// <summary>
        /// 附加觀察者
        /// </summary>
        void Attach(IConnectionObserver observer);
        
        /// <summary>
        /// 移除觀察者
        /// </summary>
        void Detach(IConnectionObserver observer);
        
        /// <summary>
        /// 通知所有觀察者
        /// </summary>
        void NotifyObservers();
    }
    
    /// <summary>
    /// 連線狀態觀察者管理器
    /// </summary>
    public class ConnectionObserverManager : IConnectionSubject
    {
        private readonly List<IConnectionObserver> _observers = new List<IConnectionObserver>();
        private readonly object _lock = new object();
        
        public void Attach(IConnectionObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));
                
            lock (_lock)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }
        }
        
        public void Detach(IConnectionObserver observer)
        {
            if (observer == null)
                return;
                
            lock (_lock)
            {
                _observers.Remove(observer);
            }
        }
        
        public void NotifyObservers()
        {
            // 由具體實作決定通知內容
        }
        
        /// <summary>
        /// 通知狀態變更
        /// </summary>
        public void NotifyStateChanged(IRdpConnection connection, RdpState oldState, RdpState newState)
        {
            List<IConnectionObserver> observersCopy;
            lock (_lock)
            {
                observersCopy = new List<IConnectionObserver>(_observers);
            }
            
            foreach (var observer in observersCopy)
            {
                try
                {
                    observer.OnConnectionStateChanged(connection, oldState, newState);
                }
                catch (Exception ex)
                {
                    // 記錄錯誤但不影響其他觀察者
                    var logger = Core.RdpLogger.Instance;
                    logger.LogError($"觀察者通知失敗: {observer.GetType().Name}", ex);
                }
            }
        }
        
        /// <summary>
        /// 通知連線超時
        /// </summary>
        public void NotifyTimeout(IRdpConnection connection, RdpConnectionTimeoutException exception)
        {
            List<IConnectionObserver> observersCopy;
            lock (_lock)
            {
                observersCopy = new List<IConnectionObserver>(_observers);
            }
            
            foreach (var observer in observersCopy)
            {
                try
                {
                    observer.OnConnectionTimeout(connection, exception);
                }
                catch (Exception ex)
                {
                    var logger = Core.RdpLogger.Instance;
                    logger.LogError($"觀察者超時通知失敗: {observer.GetType().Name}", ex);
                }
            }
        }
        
        /// <summary>
        /// 取得目前觀察者數量
        /// </summary>
        public int ObserverCount
        {
            get
            {
                lock (_lock)
                {
                    return _observers.Count;
                }
            }
        }
    }
    
    /// <summary>
    /// 日誌記錄觀察者 - 將連線狀態變更寫入日誌
    /// </summary>
    public class LoggingConnectionObserver : IConnectionObserver
    {
        private readonly Core.RdpLogger _logger;
        
        public LoggingConnectionObserver()
        {
            _logger = Core.RdpLogger.Instance;
        }
        
        public void OnConnectionStateChanged(IRdpConnection connection, RdpState oldState, RdpState newState)
        {
            _logger.LogInfo($"連線狀態變更: {oldState} -> {newState}", connection.ConnectionId);
        }
        
        public void OnConnectionTimeout(IRdpConnection connection, RdpConnectionTimeoutException exception)
        {
            _logger.LogWarning($"連線超時: {exception.TimeoutSeconds}秒", connection.ConnectionId);
        }
    }
    
    /// <summary>
    /// 統計收集觀察者 - 收集連線統計資訊
    /// </summary>
    public class StatisticsConnectionObserver : IConnectionObserver
    {
        private int _totalStateChanges = 0;
        private int _totalTimeouts = 0;
        private readonly Dictionary<RdpState, int> _stateChangeCounts = new Dictionary<RdpState, int>();
        private readonly object _lock = new object();
        
        public void OnConnectionStateChanged(IRdpConnection connection, RdpState oldState, RdpState newState)
        {
            lock (_lock)
            {
                _totalStateChanges++;
                
                if (!_stateChangeCounts.ContainsKey(newState))
                {
                    _stateChangeCounts[newState] = 0;
                }
                _stateChangeCounts[newState]++;
            }
        }
        
        public void OnConnectionTimeout(IRdpConnection connection, RdpConnectionTimeoutException exception)
        {
            lock (_lock)
            {
                _totalTimeouts++;
            }
        }
        
        /// <summary>
        /// 取得統計資訊
        /// </summary>
        public string GetStatistics()
        {
            lock (_lock)
            {
                return $"總狀態變更: {_totalStateChanges}, 總超時: {_totalTimeouts}";
            }
        }
        
        /// <summary>
        /// 重置統計
        /// </summary>
        public void ResetStatistics()
        {
            lock (_lock)
            {
                _totalStateChanges = 0;
                _totalTimeouts = 0;
                _stateChangeCounts.Clear();
            }
        }
    }
}
