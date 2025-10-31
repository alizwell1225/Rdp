using System;
using System.IO;
using System.Threading;

namespace LIB_RDP.Core
{
    /// <summary>
    /// RDP日誌記錄器
    /// </summary>
    public sealed class RdpLogger
    {
        private static readonly Lazy<RdpLogger> _instance = new Lazy<RdpLogger>(() => new RdpLogger());
        private readonly object _lockObject = new object();
        private readonly string _logFilePath;
        
        public static RdpLogger Instance => _instance.Value;
        
        private RdpLogger()
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RDP_Lib", "Logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"rdp_log_{DateTime.Now:yyyyMMdd}.txt");
        }
        
        public void LogInfo(string message, string connectionId = null)
        {
            WriteLog("INFO", message, connectionId);
        }
        
        public void LogWarning(string message, string connectionId = null)
        {
            WriteLog("WARN", message, connectionId);
        }
        
        public void LogError(string message, Exception exception = null, string connectionId = null)
        {
            string fullMessage = exception != null ? $"{message}\n異常詳情: {exception}" : message;
            WriteLog("ERROR", fullMessage, connectionId);
        }
        
        public void LogDebug(string message, string connectionId = null)
        {
            #if DEBUG
            WriteLog("DEBUG", message, connectionId);
            #endif
        }
        
        private void WriteLog(string level, string message, string connectionId)
        {
            try
            {
                lock (_lockObject)
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] " +
                                    $"[Thread-{Thread.CurrentThread.ManagedThreadId}] " +
                                    (string.IsNullOrEmpty(connectionId) ? "" : $"[{connectionId}] ") +
                                    $"{message}";
                    
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // 日誌記錄失敗時不拋出異常，避免影響主要功能
            }
        }
        
        /// <summary>
        /// 清理舊日誌文件（保留最近7天）
        /// </summary>
        public void CleanOldLogs()
        {
            try
            {
                string logDir = Path.GetDirectoryName(_logFilePath);
                var files = Directory.GetFiles(logDir, "rdp_log_*.txt");
                DateTime cutoffDate = DateTime.Now.AddDays(-7);
                
                foreach (string file in files)
                {
                    FileInfo info = new FileInfo(file);
                    if (info.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // 清理失敗時忽略
            }
        }
    }
}