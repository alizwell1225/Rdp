using System;

namespace LIB_RDP.Models
{
    /// <summary>
    /// RDP連線統計資訊
    /// </summary>
    public class RdpConnectionStats
    {
        /// <summary>
        /// 連線ID
        /// </summary>
        public string ConnectionId { get; set; }
        
        /// <summary>
        /// 主機名稱
        /// </summary>
        public string HostName { get; set; }
        
        /// <summary>
        /// 是否已連線
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// 連線狀態
        /// </summary>
        public RdpState State { get; set; }
        
        /// <summary>
        /// 已連線時間
        /// </summary>
        public TimeSpan ConnectedDuration { get; set; }
        
        /// <summary>
        /// 連線建立時間
        /// </summary>
        public DateTime? ConnectionStartTime { get; set; }
        
        /// <summary>
        /// 最後活動時間
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 重連次數
        /// </summary>
        public int ReconnectAttempts { get; set; }
        
        /// <summary>
        /// 網路延遲（毫秒）
        /// </summary>
        public double? NetworkLatency { get; set; }
        
        /// <summary>
        /// 頻寬使用率（KB/s）
        /// </summary>
        public double? BandwidthUsage { get; set; }
        
        public override string ToString()
        {
            return $"連線 {ConnectionId} - {HostName} ({State}) - 持續時間: {ConnectedDuration:hh\\:mm\\:ss}";
        }
    }
}