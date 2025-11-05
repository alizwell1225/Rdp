using System.Collections.Concurrent;
using System.Text;

namespace LogViewer
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
        private readonly string _logDirectory;
        private readonly string _fileNameTemplate;
        private readonly int _maxEntriesPerFile;
        private int _currentFileEntries;
        private int _currentFileVersion;
        private StreamWriter? _currentWriter;
        private FileStream? _currentStream;
        private readonly object _fileLock = new();
        private bool _disposed;

        /// <summary>
        /// Event raised when a log line is written
        /// </summary>
        public event Action<string>? OnLine;

        /// <summary>
        /// Gets or sets whether to write logs to console
        /// </summary>
        public bool EnableConsoleLog { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to force abandon writing on exception
        /// </summary>
        public bool ForceAbandonOnException { get; set; } = false;

        protected LoggerBase(string logDirectory, string fileNameTemplate, int maxEntriesPerFile = 20000)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
                throw new ArgumentException("Log directory cannot be null or empty", nameof(logDirectory));
            
            if (string.IsNullOrWhiteSpace(fileNameTemplate))
                throw new ArgumentException("File name template cannot be null or empty", nameof(fileNameTemplate));

            if (maxEntriesPerFile <= 0)
                throw new ArgumentException("Max entries per file must be positive", nameof(maxEntriesPerFile));

            _logDirectory = logDirectory;
            _fileNameTemplate = fileNameTemplate;
            _maxEntriesPerFile = maxEntriesPerFile;
            _currentFileEntries = 0;
            _currentFileVersion = 0;

            // Ensure log directory exists
            if (Directory.Exists(_logDirectory)==false)
                Directory.CreateDirectory(_logDirectory);

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
            try
            {
                var entry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message
                };

                _queue.Add(entry);

                var line = FormatLogEntry(entry);
                if (EnableConsoleLog) 
                    Console.WriteLine(line);
                
                OnLine?.Invoke(line);
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
            var filePath = Path.Combine(_logDirectory, fileName);

            _currentStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _currentWriter = new StreamWriter(_currentStream, Encoding.UTF8);
            Interlocked.Exchange(ref _currentFileEntries, 0);
            _currentFileVersion++;
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
