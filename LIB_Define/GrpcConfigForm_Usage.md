# GrpcConfigForm Usage Guide

## Overview

`GrpcConfigForm` provides a Windows Forms UI for configuring gRPC client and server settings. It supports:
- Host address and port configuration
- Log file path configuration
- Storage root/path configuration
- Differentiation between Server and Client modes
- Load and Save configuration to JSON files
- Reset to default values

## Usage

### Option 1: Using GrpcConfigHelper (Recommended)

The easiest way to use the configuration form is through the `GrpcConfigHelper` class:

#### For RpcClient

```csharp
using LIB_Define;

// Show client configuration dialog
string clientConfigPath = "./Config/client_config.json";
bool saved = GrpcConfigHelper.ShowClientConfigDialog(clientConfigPath);

if (saved)
{
    // User saved changes, create client with updated config
    var client = new RpcClient(clientConfigPath, index: 0);
    await client.StartConnect();
}
```

#### For RpcServer

```csharp
using LIB_Define;

// Show server configuration dialog
string serverConfigPath = "./Config/server_config.json";
bool saved = GrpcConfigHelper.ShowServerConfigDialog(serverConfigPath);

if (saved)
{
    // User saved changes, load config and start server
    var config = GrpcConfig.Load(serverConfigPath);
    var server = new RpcServer("./logs", index: 0);
    server.UpdateConfig(config.Host, config.Port);
    await server.StartAsync();
}
```

#### Get Config with Optional Dialog

```csharp
// Get config, show dialog if file doesn't exist
var config = GrpcConfigHelper.GetConfig(
    "./Config/config.json", 
    isServerMode: false,  // true for server, false for client
    showDialogIfNotExists: true
);
```

### Option 2: Using GrpcConfigForm Directly

For more control, you can instantiate the form directly:

#### Client Configuration

```csharp
using LIB_Define;
using LIB_RPC;

string configPath = "./Config/client_config.json";

using (var configForm = new GrpcConfigForm(configPath, isServerMode: false))
{
    if (configForm.ShowDialog() == DialogResult.OK)
    {
        // User clicked Save
        GrpcConfig config = configForm.Config;
        
        // Use the config
        var client = new RpcClient(configPath, 0);
        await client.StartConnect();
    }
}
```

#### Server Configuration

```csharp
using LIB_Define;
using LIB_RPC;

string configPath = "./Config/server_config.json";

using (var configForm = new GrpcConfigForm(configPath, isServerMode: true))
{
    if (configForm.ShowDialog() == DialogResult.OK)
    {
        // User clicked Save
        GrpcConfig config = configForm.Config;
        
        // Use the config
        var server = new RpcServer("./logs", 0);
        server.UpdateConfig(config.Host, config.Port);
        await server.StartAsync();
    }
}
```

## Form Features

### 1. Connection Settings
- **Host**: Server address (e.g., "localhost", "192.168.1.100")
- **Port**: Port number (1-65535, default: 50051)

### 2. File Paths
- **Log File Path**: Location for gRPC log files
  - Browse button for file selection
  - Example: `C:\MyApp\Logs\grpc.log`
  
- **Storage Root / Path**: Directory for file storage
  - For Server: Upload directory (where client files are received)
  - For Client: Download directory (where server files are saved)
  - Browse button for folder selection
  - Example: `C:\MyApp\Storage`

### 3. Mode Indicator
- Shows "Mode: Server" in dark blue for server configuration
- Shows "Mode: Client" in dark green for client configuration

### 4. Buttons
- **Save**: Validates and saves configuration to file
- **Cancel**: Closes dialog without saving
- **Load Config...**: Load configuration from a different JSON file
- **Reset Defaults**: Reset all fields to default values

## Integration Example: Adding Config Button to Your Form

### Example 1: Button in WinForms Application

```csharp
public partial class MyForm : Form
{
    private RpcClient _client;
    
    public MyForm()
    {
        InitializeComponent();
    }
    
    private void btnConfig_Click(object sender, EventArgs e)
    {
        string configPath = "./Config/client_config.json";
        
        if (GrpcConfigHelper.ShowClientConfigDialog(configPath))
        {
            // Config was saved, reload client
            if (_client != null)
            {
                // Disconnect existing client
                // (Add disconnect logic here)
            }
            
            _client = new RpcClient(configPath, 0);
            // Wire up events and connect
            // (Add your event handlers here)
            await _client.StartConnect();
            
            MessageBox.Show("Configuration updated and client reconnected.", 
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
```

### Example 2: Menu Item

```csharp
private void menuItemConfig_Click(object sender, EventArgs e)
{
    string configPath = "./Config/server_config.json";
    
    if (GrpcConfigHelper.ShowServerConfigDialog(configPath))
    {
        // Reload server configuration
        var config = GrpcConfig.Load(configPath);
        
        // Update UI or restart server
        MessageBox.Show($"Configuration saved: {config.Host}:{config.Port}", 
            "Config Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```

### Example 3: With RpcClient Integration

```csharp
using LIB_Define;
using System.Windows.Forms;

public class ClientApp
{
    private RpcClient _client;
    private string _configPath = "./Config/client_config.json";
    
    public void ShowConfigAndConnect()
    {
        // Show configuration dialog
        if (GrpcConfigHelper.ShowClientConfigDialog(_configPath))
        {
            // Create and configure client
            _client = new RpcClient(_configPath, index: 0);
            
            // Wire up events
            _client.ActionOnLog += (idx, msg) => Console.WriteLine(msg);
            _client.ActionOnServerJson += HandleServerJson;
            _client.ActionOnServerImage += HandleServerImage;
            
            // Connect
            await _client.StartConnect();
        }
    }
    
    private void HandleServerJson(int index, JsonMessage msg)
    {
        // Handle incoming JSON messages
    }
    
    private void HandleServerImage(int index, ShowPictureType type, Image img)
    {
        // Handle incoming images
    }
}
```

### Example 4: With RpcServer Integration

```csharp
using LIB_Define;
using LIB_RPC;

public class ServerApp
{
    private RpcServer _server;
    private string _configPath = "./Config/server_config.json";
    
    public async Task ShowConfigAndStart()
    {
        // Show configuration dialog
        if (GrpcConfigHelper.ShowServerConfigDialog(_configPath))
        {
            var config = GrpcConfig.Load(_configPath);
            
            // Create and configure server
            _server = new RpcServer(config.LogFilePath, index: 0);
            
            // Wire up events
            _server.ActionOnLog += (idx, msg) => Console.WriteLine(msg);
            _server.ActionOnClientConnected += (idx, clientId) => 
                Console.WriteLine($"Client connected: {clientId}");
            
            // Update config and start
            _server.UpdateConfig(config.Host, config.Port);
            await _server.StartAsync();
            
            Console.WriteLine($"Server started on {config.Host}:{config.Port}");
        }
    }
}
```

## Configuration File Format

The configuration is saved as JSON:

```json
{
  "Host": "localhost",
  "Port": 50051,
  "BaseAddress": "localhost:50051",
  "Password": "changeme",
  "MaxChunkSizeBytes": 102400,
  "StorageRoot": "C:\\MyApp\\Storage",
  "EnableConsoleLog": true,
  "LogFilePath": "C:\\MyApp\\Logs\\grpc.log",
  "MaxLogEntriesPerFile": 10000,
  "ForceAbandonLogOnException": false,
  "MaxLogRetentionDays": 60,
  "AutoDeleteReceivedFiles": false,
  "ClientDownloadPath": "C:\\MyApp\\Downloads",
  "ServerUploadPath": "C:\\MyApp\\Uploads",
  "CheckStorageRootHaveFile": false
}
```

## Validation

The form validates:
- Host cannot be empty
- Port must be between 1 and 65535
- Log file path cannot be empty
- Storage root cannot be empty

All directories are automatically created when saving.

## Tips

1. **Default Configuration**: If no config file exists, the form will load default values
2. **Separate Configs**: Use different config files for client and server:
   - `./Config/client_config.json`
   - `./Config/server_config.json`
3. **Auto-create Directories**: The form automatically creates configured directories
4. **Load Existing**: Use "Load Config..." to import settings from another file
5. **Reset**: Use "Reset Defaults" to restore default values

## Complete Example Application

```csharp
using LIB_Define;
using LIB_RPC;
using System;
using System.Windows.Forms;

namespace MyRpcApp
{
    public partial class MainForm : Form
    {
        private RpcClient _client;
        private readonly string _clientConfigPath = "./Config/client_config.json";
        
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void btnSettings_Click(object sender, EventArgs e)
        {
            // Show configuration dialog
            if (GrpcConfigHelper.ShowClientConfigDialog(_clientConfigPath))
            {
                lblStatus.Text = "Configuration updated";
            }
        }
        
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // Ensure we have a config (show dialog if not exists)
                var config = GrpcConfigHelper.GetConfig(
                    _clientConfigPath, 
                    isServerMode: false,
                    showDialogIfNotExists: true
                );
                
                // Create client
                _client = new RpcClient(_clientConfigPath, 0);
                _client.ActionOnLog += (idx, msg) => UpdateLog(msg);
                
                // Connect
                await _client.StartConnect();
                lblStatus.Text = $"Connected to {config.Host}:{config.Port}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateLog), message);
                return;
            }
            txtLog.AppendText(message + Environment.NewLine);
        }
    }
}
```

This provides a complete, production-ready configuration UI for your RPC client/server applications!
