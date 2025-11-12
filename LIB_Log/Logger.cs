namespace LIB_Log;

public class Logger : LoggerBase
{
    private string FileName = "";
    /// <summary>
    /// Creates a new GrpcLogger instance with the specified configuration
    /// </summary>
    /// <param name="config">Configuration containing log settings</param>
    /// <param name="directory"></param>
    /// <param name="fileName"></param>
    /// <param name="maxLogEntriesPerFile"></param>
    /// <param name="maxLogRetentionDays"></param>
    public Logger(string directory, string fileName, int maxLogEntriesPerFile = 10000, int maxLogRetentionDays = 60) :
        base(GetLogDirectory(directory), GetLogFileName(fileName), GetLogMaxLogPerFile(maxLogEntriesPerFile), GetLogRetentionDays(maxLogRetentionDays))
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException("log Name is Null");
        }
        FileName=fileName;
        EnableWriteLog = true;
        EnableConsoleLog = true;
        ForceAbandonOnException = true;
    }

    private static string GetLogDirectory(string LogFilePath)
    {
        var logPath = LogFilePath;
        var directory = Path.GetDirectoryName(logPath);
        return !string.IsNullOrWhiteSpace(directory)
            ? directory
            : AppContext.BaseDirectory;
    }

    private static string GetLogFileName(string LogFilePath)
    {
        var logPath = LogFilePath;
        var fileName = Path.GetFileName(logPath);
        return !string.IsNullOrWhiteSpace(fileName)
            ? fileName
            : "temp.log";
    }

    private static int GetLogMaxLogPerFile(int maxLog = 10000)
    {
        return (maxLog < 0) ? 60 : maxLog;
    }

    private static int GetLogRetentionDays(int day = 60)
    {
        return (day < 0) ? 60 : day;
    }
}