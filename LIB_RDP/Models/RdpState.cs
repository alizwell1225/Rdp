namespace LIB_RDP.Models
{
    public enum RdpState
    {
        Disconnected,
        Connecting,
        Connected,
        Retrying,   // 重試中
        Error
    }
} 