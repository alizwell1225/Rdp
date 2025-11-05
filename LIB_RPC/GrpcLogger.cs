using LIB_Log;

namespace LIB_RPC
{
    /// <summary>
    /// GrpcLogger with enhanced logging capabilities including file rotation and configurable settings
    /// </summary>
    public sealed class GrpcLogger : LoggerBase
    {
        private readonly GrpcConfig _config;

        /// <summary>
        /// Creates a new GrpcLogger instance with the specified configuration
        /// </summary>
        /// <param name="config">Configuration containing log settings</param>
        public GrpcLogger(GrpcConfig config) 
            : base(
                GetLogDirectory(config),
                GetLogFileName(config),
                config.MaxLogEntriesPerFile,
                config.MaxLogRetentionDays)
        {
            _config = config;
            EnableConsoleLog = config.EnableConsoleLog;
            ForceAbandonOnException = config.ForceAbandonLogOnException;
        }

        private static string GetLogDirectory(GrpcConfig config)
        {
            var logPath = config.LogFilePath;
            var directory = Path.GetDirectoryName(logPath);
            return !string.IsNullOrWhiteSpace(directory) 
                ? directory 
                : AppContext.BaseDirectory;
        }

        private static string GetLogFileName(GrpcConfig config)
        {
            var logPath = config.LogFilePath;
            var fileName = Path.GetFileName(logPath);
            return !string.IsNullOrWhiteSpace(fileName) 
                ? fileName 
                : "temp.log";
        }
    }
}
