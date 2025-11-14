# Test gRPC Server Application

## Overview

TestGrpcServerApp is a .NET 8 Windows Forms test application designed to simplify testing and demonstration of gRPC server functionality. It provides a user-friendly interface to configure, start, stop, and test gRPC server operations without writing code.

## Features

### Server Control
- **Open Server Config**: Configure server host, port, storage path, and log path
- **Start Server**: Start the gRPC server with current configuration
- **Stop Server**: Gracefully stop the running server
- **Auto-start on Launch**: Enable automatic server startup when the application launches

### Test Operations
- **Send JSON**: Broadcast JSON messages to all connected clients with validation and formatting
- **Send File**: Push any file to all connected clients
- **Send Image (Path)**: Send image files specifically from a selected path

### Logging
- **Real-time Log Display**: View server events and operations in real-time
- **View Log Files**: Open the log directory in File Explorer
- **Clear Log**: Clear the current log display

## Architecture

The application is built using the decoupled architecture provided by `LIB_Define.RPC.GrpcServerHelper`:

```
TestGrpcServerApp
    ├── Uses: LIB_Define (GrpcServerHelper)
    ├── Uses: LIB_Log (LoggerServer)
    └── Provides: UI for testing gRPC server

LIB_Define.RPC.GrpcServerHelper
    ├── Wraps: IServerApi (from LIB_RPC)
    ├── Provides: Event-driven API
    └── Manages: Configuration, state, and lifecycle
```

## Usage

### Starting the Server

1. Click **Open Server Config** to configure:
   - Host: Server host address (default: localhost)
   - Port: Server port number (default: 50051)
   - Storage Path: Directory for file storage
   - Log Path: Location of log files

2. (Optional) Enable **Auto-start on Launch** checkbox to automatically start the server when the application launches

3. Click **Start Server** to begin accepting client connections

4. Monitor the log window for server events:
   - Server started/stopped
   - Client connections/disconnections
   - File uploads
   - Broadcast operations

### Testing JSON Broadcast

1. Ensure the server is running
2. Click **Send JSON**
3. In the dialog:
   - Enter a message type (e.g., "test_message")
   - Enter or paste JSON content
   - Click **Validate** to check JSON syntax
   - Click **Format** to auto-format JSON
   - Click **Send** to broadcast

### Testing File Push

1. Ensure the server is running
2. Click **Send File**
3. Select any file from the file picker
4. The file will be pushed to all connected clients
5. Check the log for confirmation

### Testing Image Push

1. Ensure the server is running
2. Click **Send Image (Path)**
3. Select an image file (PNG, JPG, JPEG, BMP, GIF)
4. The image will be pushed to all connected clients
5. Check the log for confirmation

## Configuration

Server configuration is stored in `Config/ServerConfig.json` relative to the application directory:

```json
{
  "Host": "localhost",
  "Port": 50051,
  "StorageRoot": "C:\\Path\\To\\Storage",
  "LogFilePath": "C:\\Path\\To\\Log\\grpc.log"
}
```

Application settings (including auto-start preference) are stored in `Config/AppSettings.json`:

```json
{
  "AutoStartServer": true
}
```

## Dependencies

- **LIB_Define**: Provides GrpcServerHelper for decoupled server operations
- **LIB_Log**: Provides LoggerServer for file-based logging
- **LIB_RPC**: Core gRPC functionality (indirect dependency via LIB_Define)

## Event Handling

The application responds to the following server events:

- `OnLog`: Log messages from server operations
- `OnServerStarted`: Server successfully started
- `OnServerStopped`: Server stopped
- `OnServerStartFailed`: Server failed to start
- `OnClientConnected`: Client connected to server
- `OnClientDisconnected`: Client disconnected from server
- `OnFileAdded`: File added to server storage
- `OnFileUploadCompleted`: File upload from client completed

## Troubleshooting

### Server fails to start
- Check if the port is already in use
- Verify host configuration is correct
- Check log files for detailed error messages

### Cannot send messages
- Ensure server is running (Start Server button should be disabled)
- Verify at least one client is connected
- Check network connectivity

### Log directory not opening
- The application attempts to open the log directory using Windows Explorer
- Ensure the log path exists and is accessible
- Check Windows file permissions

## Development

To extend this application:

1. Reference the `GrpcServerHelper` class for server operations
2. Subscribe to events for real-time updates
3. Call async methods for operations (StartServerAsync, BroadcastJsonAsync, etc.)
4. Handle errors through try-catch and event callbacks

Example:
```csharp
var serverHelper = new GrpcServerHelper();
serverHelper.OnLog += (msg) => Console.WriteLine(msg);
await serverHelper.StartServerAsync();
await serverHelper.BroadcastJsonAsync("test", "{\"data\":\"value\"}");
await serverHelper.StopServerAsync();
```

## License

This is part of the Rdp project. See the main repository for license information.
