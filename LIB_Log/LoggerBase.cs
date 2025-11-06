using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace LIB_Log
{
    /// <summary>
    /// Base class for high-performance asynchronous logging with file rotation support
    /// </summary>
    public abstract class LoggerBase : IDisposable
    {
        /// <summary>
        /// Log format pattern for parsing compatibility
        /// Format: yyyy-MM-dd HH:mm:ss.fff [LEVEL] Message
        /// </summary>
        public const string LogFormatPattern = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly BlockingCollection<LogEntry> _queue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;
        private  string _baseLogDirectory;
        private  string _fileNameTemplate;
        private  int _maxEntriesPerFile;
        private  int _maxRetentionDays;
        private int _currentFileEntries;
        private int _currentFileVersion;
        private StreamWriter? _currentWriter;
        private FileStream? _currentStream;
        private readonly object _fileLock = new();
        private bool _disposed;
        private string _currentDateFolder = string.Empty;

        /// <summary>
        /// Event raised when a log line is written
        /// </summary>
        public event Action<string>? OnLine;

        /// <summary>
        /// Gets or sets whether to write logs to console
        /// </summary>
        public bool EnableConsoleLog { get; set; } = true;
        public bool EnableWriteLog { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to force abandon writing on exception
        /// </summary>
        public bool ForceAbandonOnException { get; set; } = false;

        protected LoggerBase(string logDirectory, string fileNameTemplate, int maxEntriesPerFile = 10000, int maxRetentionDays = 60)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
                throw new ArgumentException("Log directory cannot be null or empty", nameof(logDirectory));
            
            if (string.IsNullOrWhiteSpace(fileNameTemplate))
                throw new ArgumentException("File name template cannot be null or empty", nameof(fileNameTemplate));

            if (maxEntriesPerFile <= 0)
                throw new ArgumentException("Max entries per file must be positive", nameof(maxEntriesPerFile));

            if (maxRetentionDays <= 0)
                throw new ArgumentException("Max retention days must be positive", nameof(maxRetentionDays));

            _baseLogDirectory = logDirectory;
            _fileNameTemplate = fileNameTemplate;
            _maxEntriesPerFile = maxEntriesPerFile;
            _maxRetentionDays = maxRetentionDays;
            _currentFileEntries = 0;
            _currentFileVersion = 0;

            // Ensure base log directory exists
            if (Directory.Exists(_baseLogDirectory) == false)
                Directory.CreateDirectory(_baseLogDirectory);

            // Detect and resume from last version
            DetectLastVersion();

            // Cleanup old logs
            CleanupOldLogs();

            _worker = Task.Run(ProcessAsync);
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public void Info(string message) => Write(LogLevel.Info, message);

        /// <summary>
        /// Logs an error message
        /// </summary>
        public void Error(string message) => Write(LogLevel.Error, message);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void Warn(string message) => Write(LogLevel.Warn, message);

        /// <summary>
        /// Logs a debug message
        /// </summary>
        public void Debug(string message) => Write(LogLevel.Debug, message);

        /// <summary>
        /// Writes a log entry
        /// </summary>
        protected void Write(LogLevel level, string message)
        {
            bool isDateTime = false; 
            
            string[] formats = {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss"
            };
            LogEntry entry = null;
            try
            {
                #region Region1                
                // 先取出前 23 個字元 (yyyy-MM-dd HH:mm:ss.fff 長度剛好 23)
                //string possibleDate = message.Length >= 23 ? message.Substring(0, 23) : string.Empty;

                //isDateTime = DateTime.TryParseExact(
                //    possibleDate,
                //    "yyyy-MM-dd HH:mm:ss.fff",
                //    System.Globalization.CultureInfo.InvariantCulture,
                //    System.Globalization.DateTimeStyles.None,
                //    out DateTime timestamp
                #endregion

                #region Region2
                //DateTime.TryParseExact(
                //    message.Substring(0, Math.Min(23, message.Length)),
                //    formats,
                //    System.Globalization.CultureInfo.InvariantCulture,
                //    System.Globalization.DateTimeStyles.None,
                //    out DateTime timestamp
                //);
                #endregion

                #region Region3
                //// 嘗試取前面 23 個字元（有毫秒）或 19 個（無毫秒）
                //int checkLength = Math.Min(23, message.Length);
                //string possibleDate = message.Substring(0, checkLength);

                //if (DateTime.TryParseExact(
                //        possibleDate,
                //        formats,
                //        CultureInfo.InvariantCulture,
                //        DateTimeStyles.None,
                //        out DateTime timestamp))
                //{
                //    // 刪除日期時間部分
                //    // 先找出實際日期字串長度（因為有可能是 19 或 23）
                //    string matchedFormat = timestamp.Millisecond > 0 ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss";
                //    int lengthToRemove = matchedFormat.Length;

                //    // 去掉時間部分與可能的空白
                //    string remainingText = message.Substring(lengthToRemove).TrimStart();

                //    // 建立 LogRecord 物件
                //    entry = new LogEntry
                //    {
                //        Timestamp = timestamp,
                //        Level = level,
                //        Message = remainingText
                //    };
                //}
                //else
                //{
                //    entry = new LogEntry
                //    {
                //        Timestamp = DateTime.Now,
                //        Level = level,
                //        Message = message
                //    };
                //}
                #endregion

                // --- 1.嘗試解析前面的日期時間 ---
                string possibleDate = message.Length >= 19 ? message.Substring(0, Math.Min(23, message.Length)) : string.Empty;
                string line = message;
                if (DateTime.TryParseExact(
                        possibleDate,
                        formats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime timestamp))
                {
                    entry = new LogEntry();
                    entry.Timestamp = timestamp;

                    // 移除日期部分 + 任何空白
                    line = line.Substring(possibleDate.Contains('.') ? 23 : 19).TrimStart();


                    // --- 2.擷取第一個 [ ] 中的 Level ---
                    var matchLevel = Regex.Match(line, @"^\[(.*?)\]\s*");
                    if (matchLevel.Success)
                    {
                        string levelText = matchLevel.Groups[1].Value;
                        if (Enum.TryParse(levelText, ignoreCase: true, out LogLevel _level))
                        {
                            entry.Level = _level; // 取中括號內文字
                            line = line.Substring(matchLevel.Value.Length); // 移除整個 [Info]
                        }
                        else
                        {
                            entry.Level = level;
                        }
                    }

                    //// --- 3.擷取第二個 [ ] 中的 SubTitle（如果有）---
                    //var matchSub = Regex.Match(line, @"^\[(.*?)\]\s*");
                    //if (matchSub.Success)
                    //{
                    //    entry.subTitle = matchSub.Groups[1].Value;
                    //    line = line.Substring(matchSub.Value.Length);
                    //}

                    // --- 4.剩下的就是訊息 ---
                    entry.Message = line.Trim();
                }
                else
                {
                    entry = new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = level,
                        Message = message
                    };
                }
            }
            catch (Exception e)
            {
            }
            try
            {
                if (entry!=null)
                {
                    if (EnableWriteLog)
                        _queue.Add(entry);

                    var line = FormatLogEntry(entry);
                    if (EnableConsoleLog)
                        Console.WriteLine(line);

                    OnLine?.Invoke(line);
                }
            }
            catch (Exception ex)
            {
                if (!ForceAbandonOnException)
                {
                    // Try to log to console at least
                    Console.WriteLine($"[LOGGER ERROR] Failed to queue log: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Formats a log entry into a string
        /// </summary>
        protected virtual string FormatLogEntry(LogEntry entry)
        {
            return $"{entry.Timestamp.ToString(LogFormatPattern)} [{entry.Level}] {entry.Message}";
        }

        /// <summary>
        /// Processes the log queue asynchronously
        /// </summary>
        private async Task ProcessAsync()
        {
            try
            {
                foreach (var entry in _queue.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        await WriteToFileAsync(entry);
                    }
                    catch (Exception ex)
                    {
                        if (!ForceAbandonOnException)
                        {
                            Console.WriteLine($"[LOGGER ERROR] Failed to write log: {ex.Message}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when disposing
            }
            finally
            {
                CloseCurrentFile();
            }
        }

        /// <summary>
        /// Writes a log entry to file with automatic rotation
        /// </summary>
        private async Task WriteToFileAsync(LogEntry entry)
        {
            StreamWriter? writer;
            lock (_fileLock)
            {
                // Check if we need to rotate the file
                if (_currentWriter == null || _currentFileEntries >= _maxEntriesPerFile)
                {
                    CloseCurrentFile();
                    OpenNewFile();
                }
                writer = _currentWriter;
            }

            if (writer == null)
            {
                throw new InvalidOperationException("Failed to open log file");
            }

            var line = FormatLogEntry(entry);
            await writer.WriteLineAsync(line);
            await writer.FlushAsync();
            
            Interlocked.Increment(ref _currentFileEntries);
        }

        /// <summary>
        /// Opens a new log file with version number
        /// </summary>
        private void OpenNewFile()
        {
            var fileName = GetNextFileName();
            var currentDateDir = GetCurrentDateDirectory();
            var filePath = Path.Combine(currentDateDir, fileName);

            _currentStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _currentWriter = new StreamWriter(_currentStream, Encoding.UTF8);
            Interlocked.Exchange(ref _currentFileEntries, 0);
            _currentFileVersion++;
        }

        /// <summary>
        /// Gets the current date-based directory path and ensures it exists
        /// </summary>
        private string GetCurrentDateDirectory()
        {
            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            
            // Check if we need to switch to a new date folder
            if (_currentDateFolder != dateFolder)
            {
                _currentDateFolder = dateFolder;
                // Reset version when switching to a new day
                _currentFileVersion = 0;
                
                // Detect existing versions in the new date folder
                DetectLastVersion();
            }

            var dateDirPath = Path.Combine(_baseLogDirectory, dateFolder);
            if (!Directory.Exists(dateDirPath))
            {
                Directory.CreateDirectory(dateDirPath);
            }

            return dateDirPath;
        }

        /// <summary>
        /// Detects the last version number from existing log files and resumes from there
        /// </summary>
        private void DetectLastVersion()
        {
            try
            {
                var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                _currentDateFolder = dateFolder;
                var dateDirPath = Path.Combine(_baseLogDirectory, dateFolder);

                if (!Directory.Exists(dateDirPath))
                {
                    _currentFileVersion = 0;
                    return;
                }

                // Get base file name without {date} placeholder
                var baseFileName = _fileNameTemplate.Replace("{date}", DateTime.Now.ToString("yyyyMMdd"));
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
                var extension = Path.GetExtension(baseFileName);

                // Find all existing log files matching the pattern
                var searchPattern = $"{nameWithoutExtension}*{extension}";
                var existingFiles = Directory.GetFiles(dateDirPath, searchPattern);

                int maxVersion = -1;
                int maxEntries = 0;

                foreach (var file in existingFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    
                    // Check if it's a versioned file (e.g., filename_v0001)
                    if (fileName.Contains("_v"))
                    {
                        var versionPart = fileName.Substring(fileName.LastIndexOf("_v") + 2);
                        if (int.TryParse(versionPart, out var version))
                        {
                            if (version > maxVersion)
                            {
                                maxVersion = version;
                                // Count lines in the latest version file
                                maxEntries = File.ReadLines(file).Count();
                            }
                        }
                    }
                    else if (fileName == nameWithoutExtension)
                    {
                        // Base file without version
                        if (maxVersion < 0)
                        {
                            maxVersion = -1;
                            maxEntries = File.ReadLines(file).Count();
                        }
                    }
                }

                if (maxVersion >= 0)
                {
                    // Resume from the last version
                    _currentFileVersion = maxVersion;
                    _currentFileEntries = maxEntries;

                    // If the last file is full, increment version for next file
                    if (_currentFileEntries >= _maxEntriesPerFile)
                    {
                        _currentFileVersion++;
                        _currentFileEntries = 0;
                    }
                }
                else
                {
                    _currentFileVersion = 0;
                    _currentFileEntries = 0;
                }
            }
            catch
            {
                // On any error, start fresh
                _currentFileVersion = 0;
                _currentFileEntries = 0;
            }
        }

        /// <summary>
        /// Cleans up log files older than the retention period
        /// </summary>
        private void CleanupOldLogs()
        {
            try
            {
                if (!Directory.Exists(_baseLogDirectory))
                    return;

                var cutoffDate = DateTime.Now.Date.AddDays(-_maxRetentionDays);

                // Get all date folders
                var dateFolders = Directory.GetDirectories(_baseLogDirectory);

                foreach (var folder in dateFolders)
                {
                    var folderName = Path.GetFileName(folder);
                    
                    // Check if folder name matches date format (yyyy-MM-dd)
                    if (DateTime.TryParseExact(folderName, "yyyy-MM-dd", 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out var folderDate))
                    {
                        if (folderDate.Date < cutoffDate)
                        {
                            // Delete the entire folder and its contents
                            Directory.Delete(folder, true);
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore cleanup errors
            }
        }

        /// <summary>
        /// Generates the next file name with version
        /// </summary>
        private string GetNextFileName()
        {
            string baseFileName;
            
            // If template contains {date} placeholder, replace it
            if (_fileNameTemplate.Contains("{date}"))
            {
                baseFileName = _fileNameTemplate.Replace("{date}", DateTime.Now.ToString("yyyyMMdd"));
            }
            else
            {
                // Use template as-is
                baseFileName = _fileNameTemplate;
            }

            // Add version number if file already exists or if we're rotating
            if (_currentFileVersion > 0)
            {
                var extension = Path.GetExtension(baseFileName);
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
                return $"{nameWithoutExtension}_v{_currentFileVersion:D4}{extension}";
            }

            return baseFileName;
        }

        /// <summary>
        /// Closes the current log file
        /// </summary>
        private void CloseCurrentFile()
        {
            try
            {
                _currentWriter?.Flush();
                _currentWriter?.Dispose();
                _currentStream?.Dispose();
                _currentWriter = null;
                _currentStream = null;
                CleanupOldLogs();
            }
            catch
            {
                // Ignore errors when closing
            }
        }

        /// <summary>
        /// Disposes the logger and flushes remaining logs
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _queue.CompleteAdding();
            _cts.Cancel();
            
            try 
            { 
                _worker.Wait(5000); 
            }
            catch 
            {
                // Ignore timeout
            }

            _cts.Dispose();
            _queue.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Log entry structure
        /// </summary>
        protected class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// Log level enumeration
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }
}
