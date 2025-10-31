using System;

namespace LIB_RDP.Models
{
    /// <summary>
    /// RDP狀態變更事件參數
    /// </summary>
    public class RdpStateEventArgs : EventArgs
    {
        public RdpState State { get; }
        public string ConnectionId { get; }
        public DateTime Timestamp { get; }
        public string Message { get; set; }

        public RdpStateEventArgs(RdpState state, string connectionId = null, string message = null)
        {
            State = state;
            ConnectionId = connectionId;
            Timestamp = DateTime.Now;
            Message = message;
        }
    }
}