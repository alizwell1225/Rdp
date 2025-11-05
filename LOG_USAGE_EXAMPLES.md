# Logging Architecture - Usage Examples

## Basic Usage

### Simple Logging

```csharp
using LIB_RPC;

// Create configuration
var config = new GrpcConfig
{
    LogFilePath = @"C:\logs\myapp.log",
    EnableConsoleLog = true
};

// Create and use logger
using var logger = new GrpcLogger(config);
logger.Info("Application started");
logger.Warn("This is a warning");
logger.Error("An error occurred");
logger.Debug("Debug information");
```

### Configurable File Path and Name

```csharp
// Use date in filename
var config = new GrpcConfig
{
    LogFilePath = @"C:\logs\app_{date}.log", // Creates app_20251105.log
    MaxLogEntriesPerFile = 20000,
    EnableConsoleLog = true
};

using var logger = new GrpcLogger(config);
logger.Info("Logs will be saved to a date-based file");
```

### File Rotation Example

```csharp
var config = new GrpcConfig
{
    LogFilePath = @"C:\logs\myapp.log",
    MaxLogEntriesPerFile = 10000, // Rotate after 10,000 entries
    EnableConsoleLog = false // Disable console for performance
};

using var logger = new GrpcLogger(config);

// This will create multiple files as entries exceed MaxLogEntriesPerFile:
// - myapp.log (first 10,000 entries)
// - myapp_v0001.log (next 10,000 entries)
// - myapp_v0002.log (next 10,000 entries)
// ... and so on

for (int i = 0; i < 25000; i++)
{
    logger.Info($"Log entry {i}");
}
```

### Exception-Safe Logging

```csharp
// Normal mode - logs exceptions to console
var config1 = new GrpcConfig
{
    LogFilePath = @"C:\logs\app.log",
    ForceAbandonLogOnException = false // Default
};

// High-performance mode - silently abandons logs on exception
var config2 = new GrpcConfig
{
    LogFilePath = @"C:\logs\app.log",
    ForceAbandonLogOnException = true // For critical performance scenarios
};
```

## Advanced Usage

### Custom Log Directory

```csharp
// Separate directory for logs
var logDirectory = Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.ApplicationData), "MyApp", "Logs");

Directory.CreateDirectory(logDirectory);

var config = new GrpcConfig
{
    LogFilePath = Path.Combine(logDirectory, "app_{date}.log"),
    MaxLogEntriesPerFile = 20000
};
```

### Event-Based Logging

```csharp
var logger = new GrpcLogger(config);

// Subscribe to log events
logger.OnLine += (line) =>
{
    // Send to remote logging service, database, etc.
    Console.WriteLine($"[EVENT] {line}");
};

logger.Info("This triggers the OnLine event");
```

### High-Performance Logging

```csharp
var config = new GrpcConfig
{
    LogFilePath = @"C:\logs\high_perf.log",
    MaxLogEntriesPerFile = 50000, // Larger files
    EnableConsoleLog = false, // Disable console output
    ForceAbandonLogOnException = true // Fail silently
};

using var logger = new GrpcLogger(config);

// Log intensively without blocking
for (int i = 0; i < 1000000; i++)
{
    logger.Info($"High-performance log entry {i}");
}
// All writes are asynchronous and non-blocking
```

## Log Viewer Usage

### Running the Log Viewer

1. **From Visual Studio/Rider:**
   - Open RDP.sln
   - Set LogViewer as startup project
   - Press F5 to run

2. **From Command Line:**
   ```bash
   cd LogViewer
   dotnet run
   ```

3. **From Compiled Executable:**
   ```bash
   dotnet build LogViewer/LogViewer.csproj -c Release
   ./LogViewer/bin/Release/net8.0-windows/LogViewer.exe
   ```

### Using the Log Viewer

1. **Load Logs:**
   - Click "Browse..." to select log directory
   - Click "Load Logs" to read all .log files

2. **Filter by Date:**
   - Check "Enable Date Filter"
   - Select start and end dates
   - Click "Apply Filter"

3. **Filter by Keyword:**
   - Enter keyword in "Keyword" field
   - Click "Apply Filter"
   - Search is case-insensitive

4. **Filter by Log Level:**
   - Select level from dropdown (All, Debug, Info, Warn, Error)
   - Click "Apply Filter"

5. **Combine Filters:**
   - All filters can be used together
   - Filters are AND-combined (all must match)

6. **Clear Filters:**
   - Click "Clear Filter" to reset all filters

### Log Viewer Screenshots

The viewer displays:
- **Timestamp**: When the log was created
- **Level**: Log level (Debug, Info, Warn, Error)
- **Message**: The log message
- **File Name**: Source log file

## Testing Your Logging Setup

Use the included test program:

```bash
cd LoggingTest
dotnet run
```

This will:
1. Create a test_logs directory
2. Generate 250+ log entries
3. Demonstrate file rotation (max 100 entries per file)
4. Show different log levels
5. Display created files and their sizes

Expected output:
```
=== Log Architecture Test ===

Log directory: /path/to/test_logs
Max entries per file: 100
Testing file rotation...

Generating logs to test file rotation...

Waiting for logs to flush...

=== Log Files Created ===
Total files created: 3
  test_20251105.log: 100 lines, 8456 bytes
  test_20251105_v0001.log: 100 lines, 8589 bytes
  test_20251105_v0002.log: 54 lines, 4612 bytes

=== Test Complete ===
```

## Troubleshooting

### Logs Not Being Written

1. Check directory permissions
2. Ensure LogFilePath directory exists or is creatable
3. Check EnableConsoleLog to see if logs are being generated
4. Call Dispose() or use 'using' statement to flush logs

### File Rotation Not Working

1. Verify MaxLogEntriesPerFile is set correctly
2. Ensure enough entries are being logged
3. Wait for async writes to complete (use Dispose or 'using')

### Performance Issues

1. Disable console output: `EnableConsoleLog = false`
2. Increase MaxLogEntriesPerFile to reduce rotation frequency
3. Use ForceAbandonLogOnException for critical scenarios
4. Ensure log directory is on fast storage (SSD)

## Configuration Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| LogFilePath | string | "rdp-grpc.log" | Full path to log file. Supports {date} placeholder. |
| MaxLogEntriesPerFile | int | 20000 | Maximum entries before file rotation. |
| EnableConsoleLog | bool | true | Write logs to console output. |
| ForceAbandonLogOnException | bool | false | Silently abandon log writes on exception. |

## Best Practices

1. **Always use `using` statement** for proper disposal
2. **Choose appropriate MaxLogEntriesPerFile** based on log frequency
3. **Disable console logging in production** for better performance
4. **Use date placeholders** for easier log organization
5. **Monitor log directory size** to prevent disk space issues
6. **Use appropriate log levels** for different message types
7. **Test log rotation** with your expected load
