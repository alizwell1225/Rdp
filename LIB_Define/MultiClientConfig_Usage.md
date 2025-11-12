# Multi-Client Configuration Guide

## Overview

The Multi-Client Configuration Manager allows you to configure and manage multiple RpcClient instances (up to 50) on a single machine. This is perfect for scenarios where you need to run multiple clients simultaneously, such as testing, monitoring, or distributed processing.

## Features

✅ **Configure Multiple Clients**: Set up 1-50 clients on one machine
✅ **Individual Settings**: Each client has its own Host, Port, LogPath, and StoragePath
✅ **Enable/Disable**: Selectively enable or disable specific clients
✅ **Template Application**: Apply common settings to all enabled clients at once
✅ **Individual Editing**: Edit any client using the standard GrpcConfigForm
✅ **Batch Operations**: Enable/disable all clients with one click
✅ **Separate Configs**: Each client saves its own JSON config file
✅ **Auto Port Increment**: Automatically increment ports for each client

## UI Components

### Main Window

The Multi-Client Configuration Manager provides a comprehensive interface:

```
┌─────────────────────────────────────────────────────────────────────┐
│ Multi-Client Configuration Manager                            [_][□][X]│
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│ ┌─ Client Count ───────┐                                           │
│ │ Number of Clients: [12▼] │                                       │
│ └──────────────────────┘                                           │
│                                                                     │
│ ┌─ Client Configuration Grid ────────────────────────────────────┐ │
│ │ # │☑│ Name      │ Host      │ Port  │ Log Path │ Storage │...│ │
│ │ 0 │☑│ Client 0  │ localhost │ 50051 │ ./Logs/..│ ./Stor..│...│ │
│ │ 1 │☑│ Client 1  │ localhost │ 50052 │ ./Logs/..│ ./Stor..│...│ │
│ │ 2 │☑│ Client 2  │ localhost │ 50053 │ ./Logs/..│ ./Stor..│...│ │
│ │...│ │           │           │       │          │         │   │ │
│ │ 11│☑│ Client 11 │ localhost │ 50062 │ ./Logs/..│ ./Stor..│...│ │
│ └─────────────────────────────────────────────────────────────────┘ │
│                                                                     │
│ [Load Config] [Apply Template] [Edit Selected] [Enable All] [...]  │
│                                              [Save] [Cancel]        │
└─────────────────────────────────────────────────────────────────────┘
```

### Grid Columns

1. **#**: Client index (0-based)
2. **Enabled**: Checkbox to enable/disable client
3. **Display Name**: Friendly name for the client
4. **Host**: Server address (e.g., "localhost", "192.168.1.100")
5. **Port**: Server port number
6. **Log Path**: Path to log file for this client
7. **Storage Path**: Storage directory for downloads
8. **Config Path**: Path to individual JSON config file

### Buttons

- **Load Config...**: Load a previously saved multi-client configuration
- **Apply Template...**: Apply common settings to all enabled clients
- **Edit Selected...**: Open individual config dialog for selected client
- **Enable All**: Enable all clients
- **Disable All**: Disable all clients
- **Save**: Save all configurations
- **Cancel**: Close without saving

## Usage

### Option 1: Using MultiClientHelper (Recommended)

#### Show Configuration Dialog

```csharp
using LIB_Define;

// Show multi-client config dialog
bool saved = MultiClientHelper.ShowMultiClientConfigDialog("./Config/multi_client_config.json");

if (saved)
{
    Console.WriteLine("Multi-client configuration saved!");
}
```

#### Create and Start All Clients

```csharp
using LIB_Define;
using System.Collections.Generic;

// Create and start all enabled clients
List<RpcClient> clients = await MultiClientHelper.CreateAndStartClientsAsync("./Config/multi_client_config.json");

Console.WriteLine($"Started {clients.Count} clients");

// Wire up events for all clients
foreach (var client in clients)
{
    client.ActionOnLog += (index, message) => 
        Console.WriteLine($"[Client {index}] {message}");
    
    client.ActionOnServerJson += (index, jsonMsg) =>
    {
        Console.WriteLine($"[Client {index}] Received: {jsonMsg.Type}");
    };
}
```

#### Stop All Clients

```csharp
// Stop all clients when done
await MultiClientHelper.StopAllClientsAsync(clients);
```

### Option 2: Manual Configuration

#### Load Configuration

```csharp
using LIB_Define;

// Load configuration
var config = MultiClientHelper.LoadMultiClientConfig("./Config/multi_client_config.json");

Console.WriteLine($"Loaded configuration for {config.ClientCount} clients");
Console.WriteLine($"Enabled clients: {config.Clients.Count(c => c.Enabled)}");
```

#### Create Clients Manually

```csharp
// Create only enabled clients
var clients = new List<RpcClient>();

foreach (var clientConfig in config.Clients)
{
    if (clientConfig.Enabled)
    {
        // Ensure config file exists
        clientConfig.SaveIndividualConfig();
        
        // Create client
        var client = new RpcClient(clientConfig.ConfigPath, clientConfig.Index);
        clients.Add(client);
    }
}

// Start all clients
foreach (var client in clients)
{
    await client.StartConnect();
}
```

### Option 3: Direct Form Usage

```csharp
using LIB_Define;
using System.Windows.Forms;

string configPath = "./Config/multi_client_config.json";

using (var form = new MultiClientConfigForm(configPath))
{
    if (form.ShowDialog() == DialogResult.OK)
    {
        var config = form.Config;
        Console.WriteLine($"Configured {config.Clients.Count(c => c.Enabled)} clients");
    }
}
```

## Apply Template Feature

The "Apply Template" button allows you to quickly configure all enabled clients with common settings:

### Template Dialog

```
┌─────────────────────────────────────────┐
│ Apply Template Settings          [X]    │
├─────────────────────────────────────────┤
│                                         │
│ Host:         [localhost__________]     │
│                                         │
│ Base Port:    [50051]                   │
│ ☑ Increment port for each client (+1)  │
│                                         │
│ Log Path:     [./Logs_____________]     │
│                                         │
│ Storage Path: [./Storage__________]     │
│                                         │
│                    [Apply] [Cancel]     │
└─────────────────────────────────────────┘
```

### Template Behavior

When you apply a template:
- **Host**: Same for all clients
- **Port**: 
  - If "Increment port" is checked: Base port + client index
  - If unchecked: Same port for all
- **Log Path**: Base path + `/client_{index}_grpc.log`
- **Storage Path**: Base path + `/client_{index}`

### Example

With template settings:
- Host: `192.168.1.100`
- Base Port: `50051`
- Increment: ✓ Checked
- Log Path: `C:\Logs`
- Storage Path: `C:\Storage`

Results in:
```
Client 0: 192.168.1.100:50051, C:\Logs\client_0_grpc.log, C:\Storage\client_0
Client 1: 192.168.1.100:50052, C:\Logs\client_1_grpc.log, C:\Storage\client_1
Client 2: 192.168.1.100:50053, C:\Logs\client_2_grpc.log, C:\Storage\client_2
...
```

## Configuration Files

### Multi-Client Config File

The main configuration file stores all client settings:

**Location**: `./Config/multi_client_config.json`

```json
{
  "ClientCount": 12,
  "Clients": [
    {
      "Index": 0,
      "Enabled": true,
      "ConfigPath": "./Config/client_0_config.json",
      "Host": "localhost",
      "Port": 50051,
      "LogFilePath": "./Logs/client_0_grpc.log",
      "StorageRoot": "./Storage/client_0",
      "DisplayName": "Client 0"
    },
    {
      "Index": 1,
      "Enabled": true,
      "ConfigPath": "./Config/client_1_config.json",
      "Host": "localhost",
      "Port": 50052,
      "LogFilePath": "./Logs/client_1_grpc.log",
      "StorageRoot": "./Storage/client_1",
      "DisplayName": "Client 1"
    }
    // ... more clients
  ]
}
```

### Individual Client Config Files

Each enabled client also has its own GrpcConfig file:

**Location**: `./Config/client_{index}_config.json`

```json
{
  "Host": "localhost",
  "Port": 50051,
  "LogFilePath": "./Logs/client_0_grpc.log",
  "StorageRoot": "./Storage/client_0",
  "ClientDownloadPath": "./Storage/client_0",
  // ... other GrpcConfig properties
}
```

## Complete Example: Multi-Client Application

```csharp
using LIB_Define;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiClientApp
{
    public class Program
    {
        private static List<RpcClient> _clients;

        [STAThread]
        static async Task Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show configuration dialog
            if (!MultiClientHelper.ShowMultiClientConfigDialog("./Config/multi_client_config.json"))
            {
                Console.WriteLine("Configuration cancelled");
                return;
            }

            // Load and create clients
            var config = MultiClientHelper.LoadMultiClientConfig("./Config/multi_client_config.json");
            _clients = MultiClientHelper.CreateClients(config);

            Console.WriteLine($"Created {_clients.Count} clients");

            // Wire up events for all clients
            foreach (var client in _clients)
            {
                int clientIndex = client.Index;
                
                client.ActionOnLog += (idx, msg) => 
                    Console.WriteLine($"[Client {idx}] {msg}");
                
                client.ActionOnConnectionError += (idx, error) =>
                    Console.WriteLine($"[Client {idx}] Connection error: {error}");
                
                client.ActionOnServerJson += (idx, jsonMsg) =>
                {
                    Console.WriteLine($"[Client {idx}] Received JSON: type={jsonMsg.Type}");
                    
                    // Handle FlowChartOBJ
                    if (jsonMsg.Type == "flowchart")
                    {
                        var flowChart = client.DeserializeJsonMessage<FlowChartOBJ>(jsonMsg);
                        if (flowChart != null)
                        {
                            Console.WriteLine($"[Client {idx}] FlowChart ID={flowChart.ID}");
                        }
                    }
                };
                
                client.ActionOnServerImage += (idx, pictureType, image) =>
                {
                    Console.WriteLine($"[Client {idx}] Received image: {pictureType}");
                    // Save or display image
                };
            }

            // Start all clients
            Console.WriteLine("Starting all clients...");
            var startTasks = new List<Task>();
            foreach (var client in _clients)
            {
                startTasks.Add(client.StartConnect());
            }
            await Task.WhenAll(startTasks);

            Console.WriteLine("All clients started!");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Stop all clients
            await MultiClientHelper.StopAllClientsAsync(_clients);
            Console.WriteLine("All clients stopped");
        }
    }
}
```

## Advanced Scenarios

### Scenario 1: Different Servers for Each Client

Configure clients to connect to different servers:

```csharp
// Client 0-3: Server A (192.168.1.100)
// Client 4-7: Server B (192.168.1.101)
// Client 8-11: Server C (192.168.1.102)

for (int i = 0; i < config.Clients.Count; i++)
{
    var client = config.Clients[i];
    
    if (i < 4)
        client.Host = "192.168.1.100";
    else if (i < 8)
        client.Host = "192.168.1.101";
    else
        client.Host = "192.168.1.102";
    
    client.Port = 50051; // Same port for all
}
```

### Scenario 2: Enable Only Specific Clients

```csharp
// Enable only clients 0, 2, 4, 6, 8, 10
for (int i = 0; i < config.Clients.Count; i++)
{
    config.Clients[i].Enabled = (i % 2 == 0);
}
```

### Scenario 3: Load Balancing Across Ports

```csharp
// Distribute clients across 3 ports
int[] ports = { 50051, 50052, 50053 };

for (int i = 0; i < config.Clients.Count; i++)
{
    config.Clients[i].Port = ports[i % ports.Length];
}
```

## Tips and Best Practices

1. **Separate Log Files**: Each client should have its own log file to avoid conflicts
2. **Separate Storage**: Use different storage directories for each client
3. **Port Management**: If connecting to the same server, use the same port. For different servers, adjust accordingly
4. **Enable/Disable**: Disable clients you don't need instead of deleting them
5. **Template First**: Use "Apply Template" to set common settings, then edit individual clients
6. **Save Often**: The form saves both the multi-client config and individual configs
7. **Naming**: Use descriptive display names to identify clients easily

## Troubleshooting

### Problem: Clients won't start
- Check that all config files exist
- Verify Host and Port are correct
- Ensure log and storage directories can be created

### Problem: Port conflicts
- Make sure each client connecting to different servers uses unique ports if needed
- Check for port conflicts with other applications

### Problem: Changes not saved
- Click "Save" button in the form
- Check that config directory exists and is writable

## Summary

The Multi-Client Configuration Manager provides a powerful yet easy-to-use interface for managing multiple RpcClient instances. Whether you need to run 2 clients or 50, this tool streamlines the configuration process and makes it easy to manage complex multi-client setups.

**Key Benefits:**
- ✅ One UI to configure all clients
- ✅ Quick template application
- ✅ Individual client editing
- ✅ Easy enable/disable
- ✅ Separate config files for each client
- ✅ Production-ready with proper error handling

Start using it today to simplify your multi-client scenarios!
