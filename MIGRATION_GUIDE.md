# 遷移指南 (Migration Guide)

## 從舊 API 遷移到新 API

本指南協助您將現有代碼從舊的緊耦合架構遷移到新的鬆耦合架構。

## 遷移策略

建議採用**漸進式遷移**策略：
1. 新功能使用新 API
2. 修改現有功能時順便遷移
3. 不急於一次性全部遷移

## 客戶端遷移

### 基本設定

**舊代碼**:
```csharp
using RdpGrpc;
using LIB_RPC;

var config = new GrpcConfig { 
    Host = "localhost", 
    Port = 50051, 
    Password = "secret" 
};
var logger = new GrpcLogger(config);
var api = new GrpcClientApi(config);
```

**新代碼**:
```csharp
using LIB_RPC;
using LIB_RPC.Abstractions;

// 使用建造者模式
var config = new GrpcConfigBuilder()
    .WithHost("localhost")
    .WithPort(50051)
    .WithPassword("secret")
    .Build();

// 使用工廠模式，返回介面
IClientApi api = GrpcApiFactory.CreateClient(config);
```

### 事件訂閱

**舊代碼**:
```csharp
api.OnServerJson += env => 
{
    // env 是 RdpGrpc.Proto.JsonEnvelope 類型
    Console.WriteLine($"Type: {env.Type}, ID: {env.Id}");
    var json = env.Json; // Protocol Buffer 類型
};
```

**新代碼**:
```csharp
api.OnServerJson += msg => 
{
    // msg 是 JsonMessage DTO 類型
    Console.WriteLine($"Type: {msg.Type}, ID: {msg.Id}");
    var json = msg.Json; // 簡單的字串屬性
};
```

### 發送 JSON

**舊代碼**:
```csharp
var ack = await api.SendJsonAsync("ping", "{\"test\": true}");
// ack 是 RdpGrpc.Proto.JsonAck
bool success = ack.Success;
string error = ack.Error;
```

**新代碼**:
```csharp
var ack = await api.SendJsonAsync("ping", "{\"test\": true}");
// ack 是 JsonAcknowledgment DTO
bool success = ack.Success;
string error = ack.Error;
```

### 檔案上傳

**舊代碼**:
```csharp
var status = await api.UploadFileAsync("file.txt");
// status 是 RdpGrpc.Proto.FileTransferStatus
if (status.Success)
    Console.WriteLine("Upload succeeded");
```

**新代碼**:
```csharp
var result = await api.UploadFileAsync("file.txt");
// result 是 FileTransferResult DTO
if (result.Success)
    Console.WriteLine("Upload succeeded");
```

### 完整範例

**舊代碼**:
```csharp
using RdpGrpc;
using System;
using System.Windows.Forms;
using LIB_RPC;
using LIB_RPC.API;

public partial class ClientForm : Form
{
    private GrpcClientApi? _api;
    private GrpcConfig? _config;

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        _config = new GrpcConfig { Host = "localhost", Port = 50051 };
        _api = new GrpcClientApi(_config);
        
        _api.OnServerJson += env => 
        {
            // 使用 Proto 類型
            Console.WriteLine(env.Type);
        };
        
        await _api.ConnectAsync();
    }
}
```

**新代碼**:
```csharp
using System;
using System.Windows.Forms;
using LIB_RPC;
using LIB_RPC.Abstractions;

public partial class ClientForm : Form
{
    private IClientApi? _api;

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        var config = new GrpcConfigBuilder()
            .WithHost("localhost")
            .WithPort(50051)
            .Build();
            
        _api = GrpcApiFactory.CreateClient(config);
        
        _api.OnServerJson += msg => 
        {
            // 使用 DTO 類型
            Console.WriteLine(msg.Type);
        };
        
        await _api.ConnectAsync();
    }
}
```

## 伺服器端遷移

### 基本設定

**舊代碼**:
```csharp
using LIB_RPC;

var controller = new GrpcServerController();
controller.OnLog += line => Console.WriteLine(line);
controller.UpdateConfig("0.0.0.0", 50051);
await controller.StartAsync();
```

或

```csharp
using LIB_RPC.API;

var controller = new GrpcServerApi();
controller.OnLog += line => Console.WriteLine(line);
controller.UpdateConfig("0.0.0.0", 50051);
await controller.StartAsync();
```

**新代碼**:
```csharp
using LIB_RPC;
using LIB_RPC.Abstractions;

IServerApi server = GrpcApiFactory.CreateServer();
server.OnLog += line => Console.WriteLine(line);
server.UpdateConfig("0.0.0.0", 50051);
await server.StartAsync();
```

### 廣播訊息

**舊代碼與新代碼相同** (方法簽名未變):
```csharp
await controller.BroadcastJsonAsync("notification", "{\"msg\": \"hello\"}");
```

### 推送檔案

**舊代碼與新代碼相同** (方法簽名未變):
```csharp
await controller.PushFileAsync(@"C:\file.pdf");
```

### 完整範例

**舊代碼**:
```csharp
using System;
using System.Windows.Forms;
using LIB_RPC;
using LIB_RPC.API;

public partial class ServerForm : Form
{
    private GrpcServerApi _controller = new GrpcServerApi();
    
    public ServerForm()
    {
        InitializeComponent();
        _controller.OnLog += line => Console.WriteLine(line);
    }
    
    private async void StartButton_Click(object sender, EventArgs e)
    {
        _controller.UpdateConfig("0.0.0.0", 50051);
        await _controller.StartAsync();
    }
}
```

**新代碼**:
```csharp
using System;
using System.Windows.Forms;
using LIB_RPC;
using LIB_RPC.Abstractions;

public partial class ServerForm : Form
{
    private readonly IServerApi _controller;
    
    public ServerForm()
    {
        InitializeComponent();
        _controller = GrpcApiFactory.CreateServer();
        _controller.OnLog += line => Console.WriteLine(line);
    }
    
    private async void StartButton_Click(object sender, EventArgs e)
    {
        _controller.UpdateConfig("0.0.0.0", 50051);
        await _controller.StartAsync();
    }
}
```

## 單元測試遷移

### 舊方式 (難以測試)

```csharp
[Test]
public async Task TestClientConnection()
{
    // 難以 mock，必須真實連線
    var config = new GrpcConfig { Host = "localhost", Port = 50051 };
    var api = new GrpcClientApi(config);
    
    await api.ConnectAsync();
    // 需要真實的伺服器運行
}
```

### 新方式 (易於測試)

```csharp
using Moq;

[Test]
public async Task TestClientConnection()
{
    // 使用 Mock
    var mockClient = new Mock<IClientApi>();
    mockClient.Setup(c => c.ConnectAsync(It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);
    
    // 注入 mock
    var form = new ClientForm(mockClient.Object);
    
    // 測試邏輯，無需真實伺服器
}

[Test]
public async Task TestSendJsonReturnsSuccess()
{
    var mockClient = new Mock<IClientApi>();
    mockClient.Setup(c => c.SendJsonAsync("test", "{}", default))
              .ReturnsAsync(new JsonAcknowledgment 
              { 
                  Id = "123", 
                  Success = true, 
                  Error = "" 
              });
    
    var ack = await mockClient.Object.SendJsonAsync("test", "{}");
    
    Assert.IsTrue(ack.Success);
}
```

## 自訂 ScreenCapture 實作

如果您需要在非 Windows 平台或有特殊截圖需求：

```csharp
using LIB_RPC.Abstractions;

public class CustomScreenCapture : IScreenCapture
{
    public byte[] CapturePrimaryPng()
    {
        // 您的自訂截圖邏輯
        // 例如使用 ImageSharp, SkiaSharp 等跨平台庫
        return capturedBytes;
    }
}

// 使用自訂實作
var config = new GrpcConfig();
var logger = new GrpcLogger(config);
var customCapture = new CustomScreenCapture();

var server = new ServerHost(config, logger, customCapture);
await server.StartAsync();
```

## 配置檔遷移

### 舊方式
```csharp
var config = GrpcConfig.Load("config.json");
```

### 新方式（更好的驗證）
```csharp
var config = new GrpcConfigBuilder()
    .FromFile("config.json")
    .WithPassword("override-password") // 可選：覆寫特定設定
    .Build();
```

## 常見問題

### Q: 我需要立即遷移所有代碼嗎？
**A**: 不需要。舊 API 仍然可用，您可以漸進式遷移。建議新功能使用新 API。

### Q: 介面會影響性能嗎？
**A**: 不會。現代 JIT 編譯器會內聯介面調用，性能影響可忽略不計。

### Q: DTO 轉換有開銷嗎？
**A**: 開銷極小（簡單的屬性複製），對整體性能影響微乎其微。

### Q: 如何在建構函數中注入依賴？
**A**: 
```csharp
public class ClientForm : Form
{
    private readonly IClientApi _api;
    
    // 使用依賴注入容器或手動傳入
    public ClientForm(IClientApi api)
    {
        _api = api;
        InitializeComponent();
    }
    
    // 或提供無參數建構函數使用預設實作
    public ClientForm() : this(GrpcApiFactory.CreateClient())
    {
    }
}
```

### Q: 我可以混用舊 API 和新 API 嗎？
**A**: 可以，但不建議在同一個模組內混用。建議以模組為單位進行遷移。

## 遷移檢查清單

- [ ] 更新 using 語句，移除 `RdpGrpc` 命名空間
- [ ] 將 `GrpcClientApi` 替換為 `IClientApi`
- [ ] 將 `GrpcServerApi` 替換為 `IServerApi`
- [ ] 使用 `GrpcApiFactory` 創建實例
- [ ] 更新事件處理器使用 DTO 類型
- [ ] 更新配置創建使用 `GrpcConfigBuilder`
- [ ] 更新單元測試使用 Mock
- [ ] 更新文檔和註解

## 遷移效益

遷移後您將獲得：

✅ **更低的耦合度**: 代碼更易維護  
✅ **更高的可測試性**: 單元測試更容易撰寫  
✅ **更好的擴展性**: 易於添加新功能  
✅ **更清晰的架構**: 職責分離更明確  
✅ **更好的 IDE 支援**: 介面提供更好的智能提示  

## 需要協助？

如果遷移過程中遇到問題，請參考：
- [架構文檔 (中文)](./LIB_RPC/ARCHITECTURE.md)
- [Architecture Documentation (English)](./LIB_RPC/ARCHITECTURE_EN.md)
- [優化總結](./OPTIMIZATION_SUMMARY.md)
