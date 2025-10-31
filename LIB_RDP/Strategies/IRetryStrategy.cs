using System;
using LIB_RDP.Models;

namespace LIB_RDP.Strategies
{
    /// <summary>
    /// 重試策略介面，定義連線重試的行為
    /// </summary>
    public interface IRetryStrategy
    {
        /// <summary>
        /// 計算下次重試的延遲時間（毫秒）
        /// </summary>
        /// <param name="attemptNumber">當前重試次數（從1開始）</param>
        /// <returns>延遲時間（毫秒）</returns>
        int GetRetryDelay(int attemptNumber);
        
        /// <summary>
        /// 最大重試次數
        /// </summary>
        int MaxRetryAttempts { get; }
        
        /// <summary>
        /// 是否應該重試此例外
        /// </summary>
        /// <param name="exception">發生的例外</param>
        /// <returns>是否應該重試</returns>
        bool ShouldRetry(Exception exception);
    }
    
    /// <summary>
    /// 指數退避重試策略
    /// </summary>
    public class ExponentialBackoffRetryStrategy : IRetryStrategy
    {
        private readonly int _maxRetryAttempts;
        private readonly int _initialDelayMs;
        private readonly int _maxDelayMs;
        private readonly double _multiplier;
        
        public ExponentialBackoffRetryStrategy(
            int maxRetryAttempts = 3,
            int initialDelayMs = 2000,
            int maxDelayMs = 8000,
            double multiplier = 2.0)
        {
            _maxRetryAttempts = maxRetryAttempts;
            _initialDelayMs = initialDelayMs;
            _maxDelayMs = maxDelayMs;
            _multiplier = multiplier;
        }
        
        public int MaxRetryAttempts => _maxRetryAttempts;
        
        public int GetRetryDelay(int attemptNumber)
        {
            if (attemptNumber <= 0)
                return 0;
                
            var delay = _initialDelayMs * Math.Pow(_multiplier, attemptNumber - 1);
            return (int)Math.Min(delay, _maxDelayMs);
        }
        
        public bool ShouldRetry(Exception exception)
        {
            // 不重試認證失敗
            if (exception is RdpAuthenticationException)
                return false;
                
            // 其他錯誤都重試
            return true;
        }
    }
    
    /// <summary>
    /// 固定延遲重試策略
    /// </summary>
    public class FixedDelayRetryStrategy : IRetryStrategy
    {
        private readonly int _maxRetryAttempts;
        private readonly int _delayMs;
        
        public FixedDelayRetryStrategy(int maxRetryAttempts = 3, int delayMs = 3000)
        {
            _maxRetryAttempts = maxRetryAttempts;
            _delayMs = delayMs;
        }
        
        public int MaxRetryAttempts => _maxRetryAttempts;
        
        public int GetRetryDelay(int attemptNumber)
        {
            return _delayMs;
        }
        
        public bool ShouldRetry(Exception exception)
        {
            if (exception is RdpAuthenticationException)
                return false;
                
            return true;
        }
    }
    
    /// <summary>
    /// 不重試策略
    /// </summary>
    public class NoRetryStrategy : IRetryStrategy
    {
        public int MaxRetryAttempts => 0;
        
        public int GetRetryDelay(int attemptNumber)
        {
            return 0;
        }
        
        public bool ShouldRetry(Exception exception)
        {
            return false;
        }
    }
}
