# RdpGrpc Library

提供基本遠端協助功能的 gRPC 共用元件：

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

## 快速使用 - Server

```csharp
var config = new GrpcConfig { Port = 50051, Password = "secret" };
var logger = new GrpcLogger(config);
var host = new ServerHost(config, logger);
await host.StartAsync();
```

## 快速使用 - Client

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
