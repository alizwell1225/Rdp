# RdpGrpc Library

提供基本遠端協助功能的 gRPC 共用元件：

## 最新架構改進 (v2.0)

### 主要優化
- ✅ **抽象層**: 新增 IClientApi、IServerApi、IScreenCapture 介面
- ✅ **跨平台**: 移除 Windows Forms 依賴，核心庫支持 net8.0
- ✅ **DTOs**: JsonMessage、JsonAcknowledgment、FileTransferResult 隔離實現細節
- ✅ **設計模式**: 建造者模式 (GrpcConfigBuilder)、工廠模式 (GrpcApiFactory)
- ✅ **依賴注入**: 支援 IScreenCapture 注入，便於測試和擴展
- ✅ **代碼精簡**: 移除重複的 GrpcServerController

詳細架構說明請參考：
- [架構文檔 (中文)](./ARCHITECTURE.md)
- [Architecture Documentation (English)](./ARCHITECTURE_EN.md)

功能:

- 雙向 JSON 訊息 (JsonStream) - 用於需要 Ack 的請求/回應模式
- 全雙工 JSON 推播 (JsonDuplex) - Server 可主動廣播 JSON 至所有已建立 Duplex 的 Client
- 檔案上傳 (client -> server) / 下載 (server -> client)
- 伺服端主動檔案推播 (FilePush 訂閱) (client 啟動後自動訂閱，Server 可呼叫 PushFileAsync)
- 螢幕截圖串流 (server -> client) PNG 分塊
- 簡易 Metadata 密碼驗證 (header: auth = base64(password))
- 可設定 chunk 大小、儲存目錄、密碼、埠號

## 事件 (Events)

`ClientConnection` 暴露事件：

- `OnJsonAck` - 來自 `JsonStream` 的 Ack 回覆
- `OnServerJson` - 來自 `JsonDuplex` Server 主動推播的 JSON 訊息
- `OnUploadProgress` `(string path, double percent)`
- `OnDownloadProgress` `(string path, double percent)`
- `OnScreenshotProgress` `(double percent)`
- `OnServerFileProgress` `(string path, double percent)` - 伺服端推送檔案進度
- `OnServerFileCompleted` `(string path)` - 伺服端推送完成 (儲存於 StorageRoot/path)
- `OnServerFileError` `(string path, string error)` - 推送過程錯誤

使用範例：

### 推薦的新 API 使用方式 (v2.0+)

```csharp
// 使用建造者創建配置
var config = new GrpcConfigBuilder()
    .WithHost("server.example.com")
    .WithPort(50051)
    .WithPassword("my-secret")
    .WithMaxChunkSize(128 * 1024)
    .Build();

// 使用工廠創建客戶端 (返回介面)
IClientApi client = GrpcApiFactory.CreateClient(config);

// 訂閱事件 (現在使用 DTO 而非 Proto 類型)
client.OnServerJson += msg => Console.WriteLine($"Type: {msg.Type}, JSON: {msg.Json}");
client.OnUploadProgress += (f, p) => Console.WriteLine($"Upload {f}: {p:F2}%");
client.OnDownloadProgress += (f, p) => Console.WriteLine($"Download {f}: {p:F2}%");
client.OnScreenshotProgress += p => Console.WriteLine($"Screenshot: {p:F2}%");

await client.ConnectAsync();
var ack = await client.SendJsonAsync("ping", "{ \"msg\": \"hello\" }");
Console.WriteLine($"Success: {ack.Success}, Error: {ack.Error}");

var result = await client.UploadFileAsync(@"sample.bin");
await client.DownloadFileAsync("server.bin", "local.bin");
var png = await client.GetScreenshotAsync();
```

### 傳統 API 使用方式 (仍然支援，但建議遷移)

```csharp
var config = new GrpcConfig { Password = "secret" };
var logger = new GrpcLogger(config);
var client = new ClientConnection(config, logger);

client.OnServerJson += env => Console.WriteLine($"Push type={env.Type} id={env.Id} bytes={env.Json?.Length}");
client.OnUploadProgress += (f, p) => Console.WriteLine($"Upload {f}: {p:F2}%");
client.OnDownloadProgress += (f, p) => Console.WriteLine($"Download {f}: {p:F2}%");
client.OnScreenshotProgress += p => Console.WriteLine($"Screenshot: {p:F2}%");

await client.ConnectAsync();
await client.SendJsonAsync("ping", "{ \"msg\": \"hello\" }");
await client.UploadFileAsync(@"sample.bin");
await client.DownloadFileAsync("server.bin", "local.bin");
var png = await client.GetScreenshotAsync();
```

### Server 廣播 JSON

伺服端啟動後可呼叫：

```csharp
await host.BroadcastJsonAsync("notice", "{ \"text\": \"Server maintenance\" }");
```

所有已建立 `JsonDuplex` 的客戶端會收到 `OnServerJson` 事件。

### Server 推送檔案 (FilePush)

Client 連線時會自動呼叫 `FilePush` 建立長連線，Server 可在任何時刻推送檔案：

```csharp
await host.PushFileAsync(@"C:\temp\announce.pdf");
```

Client 端事件：

```csharp
client.OnServerFileProgress += (p, pct) => Console.WriteLine($"PushFile {p}: {pct:F2}%");
client.OnServerFileCompleted += p => Console.WriteLine($"PushFile Completed: {p}");
client.OnServerFileError += (p, err) => Console.WriteLine($"PushFile Error {p}: {err}");
```

## 組態 (GrpcConfig)

```json
{
  "Host": "localhost",
  "Port": 50051,
  "Password": "changeme",
  "MaxChunkSizeBytes": 65536,
  "StorageRoot": "storage",
  "EnableConsoleLog": true,
  "LogFilePath": "rdp-grpc.log"
}
```

## 快速使用 - Server (推薦新方式)

```csharp
// 使用工廠創建伺服器 (返回介面)
IServerApi server = GrpcApiFactory.CreateServer();

// 訂閱事件
server.OnLog += msg => Console.WriteLine(msg);
server.OnFileAdded += path => Console.WriteLine($"File added: {path}");

// 配置並啟動
server.UpdateConfig("0.0.0.0", 50051);
await server.StartAsync();

// 廣播和推送
await server.BroadcastJsonAsync("notification", "{\"message\": \"Hello\"}");
await server.PushFileAsync(@"C:\file.pdf");
```

## 快速使用 - Server (傳統方式)

```csharp
var config = new GrpcConfig { Port = 50051, Password = "secret" };
var logger = new GrpcLogger(config);
var host = new ServerHost(config, logger);
await host.StartAsync();
```

## 快速使用 - Client (推薦新方式)

```csharp
// 使用建造者和工廠
var config = new GrpcConfigBuilder()
    .WithPassword("secret")
    .Build();
    
IClientApi client = GrpcApiFactory.CreateClient(config);
await client.ConnectAsync();

// 使用 DTO 而非 Proto 類型
var ack = await client.SendJsonAsync("ping", "{ \"msg\": \"hello\" }");
Console.WriteLine($"Success: {ack.Success}");

// Fire & forget (走 duplex, 不等待 Ack)
await client.SendJsonFireAndForgetAsync("info", "{ \"level\": \"low\" }");
```

## 快速使用 - Client (傳統方式)

```csharp
var config = new GrpcConfig { Password = "secret" };
var logger = new GrpcLogger(config);
var client = new ClientConnection(config, logger);
await client.ConnectAsync();
await client.SendJsonAsync("ping", "{ \"msg\": \"hello\" }");

// Fire & forget (走 duplex, 不等待 Ack)
await client.SendJsonFireAndForgetAsync("info", "{ \"level\": \"low\" }");
```

## TODO / 後續建議

- TLS/憑證
- JSON schema 驗證
- 重試與斷線自動重連
- 支援多螢幕擷取選擇
- 檔案分段即時校驗 (hash)
- Server 主動檔案推播 (需新增相反方向的串流 RPC)
  (已完成基本版：FilePush + PushFileAsync；可再補檔案差異/增量、壓縮、雜湊驗證)
- 廣播佇列化 / 背壓控制
