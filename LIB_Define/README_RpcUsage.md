# RpcClient and RpcServer Usage Guide

This document describes how to use the enhanced RpcClient and RpcServer classes for building gRPC-based applications without UI dependencies.

## Overview

- **RpcClient**: Client-side API for connecting to gRPC servers (extracted from GrpcClientApp)
- **RpcServer**: Server-side API for hosting gRPC services (extracted from GrpcServerApp)
- Both support generic JSON object transfer and image transfer

## Features

### 1. Generic JSON Object Transfer (Bidirectional)

Both RpcClient and RpcServer can send/receive any JSON-serializable object, including FlowChartOBJ.

### 2. Image Transfer

RpcServer can send images to RpcClient using:
- File path (ShowPictureType + path)
- Image object (ShowPictureType + Image)

### 3. UI-Independent

Both classes can be used without WinForms UI, making them suitable for console apps, services, or web backends.

## RpcServer Usage

### Basic Server Setup

```csharp
using LIB_Define;

// Create server instance
var server = new RpcServer("/path/to/logs", index: 0);

// Configure host and port
server.UpdateConfig("localhost", 50051);

// Wire up events
server.ActionOnLog += (index, message) => Console.WriteLine($"[Server {index}] {message}");
server.ActionOnClientConnected += (index, clientId) => Console.WriteLine($"Client connected: {clientId}");
server.ActionOnClientDisconnected += (index, clientId) => Console.WriteLine($"Client disconnected: {clientId}");

// Start server
await server.StartAsync();
```

### Sending FlowChartOBJ to Clients

```csharp
// Create a FlowChartOBJ
var flowChart = new FlowChartOBJ
{
    ID = 1,
    Location_X = 100,
    Location_Y = 200,
    Type = "Process",
    Caption = "Sample Process"
};

// Add some work items
flowChart.WorkList.Add("Step 1");
flowChart.WorkList.Add("Step 2");

// Add colors (will be automatically converted to ARGB for JSON)
flowChart.WorkList_Color.Add(Color.Red);
flowChart.WorkList_Color.Add(Color.Blue);

// Broadcast to all clients with acknowledgment
var result = await server.BroadcastFlowChartAsync(flowChart, useAckMode: true);
Console.WriteLine($"Success: {result.Success}, Clients reached: {result.ClientsReached}");
```

### Sending Generic Objects

```csharp
// Send any JSON-serializable object
public class MyCustomData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> Items { get; set; }
}

var data = new MyCustomData 
{ 
    Id = 123, 
    Name = "Test", 
    Items = new List<string> { "A", "B", "C" } 
};

var result = await server.BroadcastObjectAsync("custom_data", data, useAckMode: true);
```

### Sending Images by Path

```csharp
// Send image by file path
var result = await server.BroadcastImageByPathAsync(
    ShowPictureType.Flow, 
    @"C:\images\flowchart.png", 
    useAckMode: true
);
Console.WriteLine($"Image sent: {result.Success}, Clients: {result.ClientsReached}");
```

### Sending Images by Image Object

```csharp
// Load or create an image
using var image = Image.FromFile(@"C:\images\flowchart.png");
// Or create programmatically:
// using var image = new Bitmap(800, 600);
// using var g = Graphics.FromImage(image);
// g.DrawString("Hello", font, brush, point);

var result = await server.BroadcastImageAsync(
    ShowPictureType.Map, 
    image, 
    fileName: "map.png",
    useAckMode: true
);
Console.WriteLine($"Image sent: {result.Success}");
```

### Server Statistics

```csharp
// Get statistics
var stats = server.GetStatistics();
Console.WriteLine($"Total Requests: {stats.TotalRequests}");
Console.WriteLine($"Total Bytes: {stats.TotalBytes}");
Console.WriteLine($"Runtime: {stats.Runtime}");

// Reset statistics
server.ResetStatistics();
```

### Stopping Server

```csharp
await server.StopAsync();
```

## RpcClient Usage

### Basic Client Setup

```csharp
using LIB_Define;

// Create client instance with config path
var client = new RpcClient(@"C:\config\client_config.json", index: 0);

// Wire up events
client.ActionOnLog += (index, message) => Console.WriteLine($"[Client {index}] {message}");
client.ActionOnConnectionError += (index, error) => Console.WriteLine($"Connection error: {error}");

// Handle server JSON messages
client.ActionOnServerJson += (index, jsonMsg) =>
{
    Console.WriteLine($"Received JSON: type={jsonMsg.Type}, id={jsonMsg.Id}");
    
    // Deserialize based on type
    if (jsonMsg.Type == "flowchart")
    {
        var flowChart = client.DeserializeJsonMessage<FlowChartOBJ>(jsonMsg);
        if (flowChart != null)
        {
            Console.WriteLine($"FlowChart: ID={flowChart.ID}, Type={flowChart.Type}");
            // Use the flowChart...
        }
    }
};

// Handle server images
client.ActionOnServerImage += (index, pictureType, image) =>
{
    Console.WriteLine($"Received image: Type={pictureType}, Size={image.Width}x{image.Height}");
    // Use the image...
};

client.ActionOnServerImagePath += (index, pictureType, path) =>
{
    Console.WriteLine($"Received image path: Type={pictureType}, Path={path}");
    // Load and use the image from path...
};

// Connect to server
await client.StartConnect();
```

### Sending FlowChartOBJ to Server

```csharp
var flowChart = new FlowChartOBJ
{
    ID = 2,
    Location_X = 300,
    Location_Y = 400,
    Type = "Decision",
    Caption = "Check Status"
};

// Send to server
var ack = await client.SendFlowChartAsync(flowChart);
Console.WriteLine($"Sent FlowChart: Success={ack.Success}");
```

### Sending Generic Objects

```csharp
var myData = new { Status = "OK", Timestamp = DateTime.Now };
var ack = await client.SendObjectAsJsonAsync("status_update", myData);
Console.WriteLine($"Sent status: {ack.Success}");
```

### Receiving and Deserializing Objects

```csharp
// In ActionOnServerJson event handler:
client.ActionOnServerJson += (index, jsonMsg) =>
{
    // Deserialize FlowChartOBJ
    if (jsonMsg.Type == "flowchart")
    {
        var flowChart = client.DeserializeJsonMessage<FlowChartOBJ>(jsonMsg);
        if (flowChart != null)
        {
            Console.WriteLine($"Received FlowChart ID={flowChart.ID}");
            // Colors are automatically synchronized from ARGB
            foreach (var color in flowChart.WorkList_Color)
            {
                Console.WriteLine($"  Color: {color.Name}");
            }
        }
    }
    
    // Deserialize custom type
    if (jsonMsg.Type == "custom_data")
    {
        var data = client.DeserializeJsonMessage<MyCustomData>(jsonMsg);
        if (data != null)
        {
            Console.WriteLine($"Received custom data: {data.Name}");
        }
    }
};
```

## Complete Example: Console Application

### Server Console App

```csharp
using LIB_Define;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new RpcServer("./logs", 0);
        server.ActionOnLog += (i, msg) => Console.WriteLine(msg);
        
        server.UpdateConfig("localhost", 50051);
        await server.StartAsync();
        
        Console.WriteLine("Server running. Press any key to send test data...");
        Console.ReadKey();
        
        // Send test FlowChart
        var flowChart = new FlowChartOBJ { ID = 1, Type = "Start" };
        await server.BroadcastFlowChartAsync(flowChart);
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        
        await server.StopAsync();
    }
}
```

### Client Console App

```csharp
using LIB_Define;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new RpcClient("./config/config.json", 0);
        client.ActionOnLog += (i, msg) => Console.WriteLine(msg);
        client.ActionOnServerJson += (i, jsonMsg) =>
        {
            if (jsonMsg.Type == "flowchart")
            {
                var fc = client.DeserializeJsonMessage<FlowChartOBJ>(jsonMsg);
                Console.WriteLine($"Received FlowChart: ID={fc?.ID}, Type={fc?.Type}");
            }
        };
        
        await client.StartConnect();
        
        Console.WriteLine("Client connected. Press any key to exit...");
        Console.ReadKey();
    }
}
```

## Important Notes

### Color Serialization

FlowChartOBJ uses `System.Drawing.Color` which is not JSON-serializable. The implementation provides:
- `WorkList_Color`: List<Color> - For application use (marked with [JsonIgnore])
- `WorkList_ColorArgb`: List<int> - For JSON serialization
- `SyncColorsToArgb()`: Call before sending to convert Colors to ARGB ints
- `SyncColorsFromArgb()`: Called automatically when deserializing to restore Colors

### Generic Type Support

Both RpcClient and RpcServer support generic types through:
- `SendObjectAsJsonAsync<T>()`: Send any JSON-serializable type
- `BroadcastObjectAsync<T>()`: Broadcast any JSON-serializable type
- `DeserializeJsonMessage<T>()`: Deserialize received JSON to any type

### Image Transfer Options

Images can be sent as:
1. **File Path**: Efficient for local network, requires client to have file access
2. **Base64 Data**: Works across any network, larger message size

### Thread Safety

Both RpcClient and RpcServer use internal thread synchronization. Event handlers may be called from different threads, so ensure thread-safe operations in your handlers.

## Configuration Files

RpcClient requires a JSON configuration file with structure:
```json
{
  "Host": "localhost",
  "Port": 50051,
  "LogFilePath": "./logs",
  "StorageRoot": "./storage"
}
```

RpcServer creates its configuration programmatically via `UpdateConfig()`.
