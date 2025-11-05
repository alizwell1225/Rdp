# Log Architecture Upgrade

## Overview

The logging system has been upgraded with a decoupled base architecture, automatic file rotation, and a visual log viewer application.

## New Features

### 1. LoggerBase - Decoupled Base Class

Located in `LIB_RPC/Logging/LoggerBase.cs`, this abstract base class provides:

- **Asynchronous, high-performance logging** using BlockingCollection for thread-safe, non-blocking writes
- **Automatic file rotation** when log entries exceed a configurable threshold (default: 20,000 entries)
- **Version numbering** for rotated log files (e.g., `log.log`, `log_v0001.log`, `log_v0002.log`)
- **Configurable file naming** with date placeholder support (e.g., `{date}` becomes `20251105`)
- **Exception-safe logging** with configurable forced abandon capability
- **Multiple log levels**: Debug, Info, Warn, Error

### 2. Enhanced GrpcLogger

`GrpcLogger` now inherits from `LoggerBase` and automatically gets all the enhanced features:

```csharp
// Using the logger
var config = new GrpcConfig 
{ 
    LogFilePath = @"C:\logs\myapp_{date}.log",
    MaxLogEntriesPerFile = 20000,
    EnableConsoleLog = true,
    ForceAbandonLogOnException = false
};

using var logger = new GrpcLogger(config);
logger.Info("Application started");
logger.Warn("Warning message");
logger.Error("Error occurred");
```

### 3. Configuration Options (GrpcConfig)

New configuration properties:

- **`LogFilePath`**: Full path to log file (directory and filename)
- **`MaxLogEntriesPerFile`**: Maximum entries before rotating (default: 20,000)
- **`EnableConsoleLog`**: Enable/disable console output (default: true)
- **`ForceAbandonLogOnException`**: Force abandon log writing on exception (default: false)

### 4. Log Viewer Application

A WinForms application (`LogViewer`) provides visual log analysis:

#### Features:
- **Browse and load** multiple log files from a directory
- **Filter logs** by:
  - Date range (with enable/disable toggle)
  - Keywords (case-insensitive search)
  - Log level (Debug, Info, Warn, Error)
- **View details** including timestamp, level, message, and source file
- **Real-time filtering** without reloading files
- **Clear filters** to reset view

#### Running the Log Viewer:

```bash
cd LogViewer
dotnet run
```

Or build and run the executable:

```bash
dotnet build LogViewer/LogViewer.csproj -c Release
./LogViewer/bin/Release/net8.0-windows/LogViewer.exe
```

## Architecture

### File Rotation Logic

1. Logger starts writing to the base file (e.g., `app.log`)
2. When entry count reaches `MaxLogEntriesPerFile`, the current file is closed
3. A new file is created with version suffix (e.g., `app_v0001.log`)
4. Process continues with incremented version numbers

### Thread Safety

- Uses `BlockingCollection<T>` for lock-free queuing
- Single background worker thread for file I/O
- `Interlocked` operations for counter updates
- File lock protection during rotation

### Exception Handling

- Exceptions during logging are caught and optionally logged to console
- `ForceAbandonOnException` allows silent failure for critical performance scenarios
- File handles are properly disposed even on exceptions
- Graceful shutdown with timeout on disposal

## Log File Format

Standard format:
```
2025-11-05 03:13:45.123 [INFO] Application started
2025-11-05 03:13:46.456 [WARN] Connection timeout, retrying...
2025-11-05 03:13:47.789 [ERROR] Failed to connect: timeout
```

Format: `{timestamp:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}`

## Migration from Old GrpcLogger

The API remains compatible. Existing code works without changes:

```csharp
// Old and new code both work
var logger = new GrpcLogger(config);
logger.Info("message");
logger.Error("error");
logger.Warn("warning");
logger.Dispose(); // Or use 'using' statement
```

## Best Practices

1. **Use `using` statements** to ensure proper disposal:
   ```csharp
   using var logger = new GrpcLogger(config);
   // Logs are automatically flushed on disposal
   ```

2. **Configure rotation threshold** based on your needs:
   - High-frequency logging: Lower threshold (10,000-15,000)
   - Normal logging: Default threshold (20,000)
   - Low-frequency logging: Higher threshold (50,000+)

3. **Use appropriate log levels**:
   - `Debug`: Detailed diagnostic information
   - `Info`: General informational messages
   - `Warn`: Warning messages for potential issues
   - `Error`: Error messages for failures

4. **Performance considerations**:
   - Logging is asynchronous and non-blocking
   - File I/O happens on background thread
   - Console output (if enabled) is synchronous - disable for maximum performance

## Testing

To test the logging system:

```csharp
// Generate test logs
for (int i = 0; i < 25000; i++)
{
    logger.Info($"Test log entry {i}");
    if (i % 5000 == 0)
        logger.Warn($"Checkpoint at {i} entries");
}
// Should create multiple versioned log files
```

## Future Enhancements

Potential improvements for future versions:
- Log compression for archived files
- Automatic log cleanup based on age
- Remote log shipping
- Structured logging (JSON format)
- Log level filtering at write time
