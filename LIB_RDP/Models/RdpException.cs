using System;

namespace LIB_RDP.Models
{
    /// <summary>
    /// RDP相關異常的基礎類別
    /// </summary>
    public class RdpException : Exception
    {
        public string ConnectionId { get; }
        public RdpErrorCode ErrorCode { get; }
        
        public RdpException(string message, RdpErrorCode errorCode = RdpErrorCode.Unknown, string connectionId = null) 
            : base(message)
        {
            ErrorCode = errorCode;
            ConnectionId = connectionId;
        }
        
        public RdpException(string message, Exception innerException, RdpErrorCode errorCode = RdpErrorCode.Unknown, string connectionId = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ConnectionId = connectionId;
        }
    }
    
    /// <summary>
    /// RDP錯誤代碼枚舉
    /// </summary>
    public enum RdpErrorCode
    {
        Unknown = 0,
        ConnectionTimeout = 1,
        AuthenticationFailed = 2,
        NetworkError = 3,
        ServerNotFound = 4,
        AccessDenied = 5,
        InsufficientResources = 6,
        InvalidConfiguration = 7,
        ClientError = 8
    }
    
    /// <summary>
    /// 連線超時異常
    /// </summary>
    public class RdpConnectionTimeoutException : RdpException
    {
        public int TimeoutSeconds { get; }
        
        public RdpConnectionTimeoutException(int timeoutSeconds, string connectionId = null)
            : base($"RDP連線超時 ({timeoutSeconds}秒)", RdpErrorCode.ConnectionTimeout, connectionId)
        {
            TimeoutSeconds = timeoutSeconds;
        }
    }
    
    /// <summary>
    /// 認證失敗異常
    /// </summary>
    public class RdpAuthenticationException : RdpException
    {
        public RdpAuthenticationException(string message, string connectionId = null)
            : base($"RDP認證失敗: {message}", RdpErrorCode.AuthenticationFailed, connectionId)
        {
        }
    }
    
    /// <summary>
    /// 網路錯誤異常
    /// </summary>
    public class RdpNetworkException : RdpException
    {
        public RdpNetworkException(string message, Exception innerException = null, string connectionId = null)
            : base($"RDP網路錯誤: {message}", innerException, RdpErrorCode.NetworkError, connectionId)
        {
        }
    }
}