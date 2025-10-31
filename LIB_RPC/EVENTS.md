# 新增事件完整說明 (New Events Documentation)

## 概述

根據需求，已在 LIB_RPC 中新增豐富的事件系統，讓 Server 和 Client 能夠追蹤所有關鍵操作的狀態。

## Client 端新增事件

### 連線相關事件
- **OnConnected**: 成功連接到伺服器時觸發
- **OnDisconnected**: 從伺服器斷開連接時觸發
- **OnConnectionError**: 連接嘗試失敗時觸發，參數：錯誤訊息

### 檔案上傳事件
- **OnUploadStarted**: 檔案上傳開始時觸發，參數：檔案路徑
- **OnUploadCompleted**: 檔案上傳成功完成時觸發，參數：檔案路徑
- **OnUploadFailed**: 檔案上傳失敗時觸發，參數：檔案路徑、錯誤訊息
- **OnUploadProgress**: 檔案上傳進度更新時觸發（已存在），參數：檔案路徑、進度百分比

### 檔案下載事件
- **OnDownloadStarted**: 檔案下載開始時觸發，參數：遠端檔案路徑
- **OnDownloadCompleted**: 檔案下載成功完成時觸發，參數：遠端檔案路徑
- **OnDownloadFailed**: 檔案下載失敗時觸發，參數：遠端檔案路徑、錯誤訊息
- **OnDownloadProgress**: 檔案下載進度更新時觸發（已存在），參數：檔案路徑、進度百分比

### 螢幕截圖事件
- **OnScreenshotStarted**: 螢幕截圖開始時觸發
- **OnScreenshotCompleted**: 螢幕截圖成功完成時觸發，參數：圖片大小（bytes）
- **OnScreenshotFailed**: 螢幕截圖失敗時觸發，參數：錯誤訊息
- **OnScreenshotProgress**: 螢幕截圖進度更新時觸發（已存在），參數：進度百分比

### 伺服器推送事件
- **OnServerFileStarted**: 伺服器開始推送檔案時觸發，參數：檔案路徑
- **OnServerFileCompleted**: 伺服器檔案推送完成時觸發（已存在），參數：檔案路徑
- **OnServerFileError**: 伺服器檔案推送錯誤時觸發（已存在），參數：檔案路徑、錯誤訊息

## Server 端新增事件

### 伺服器生命週期事件
- **OnServerStarted**: 伺服器成功啟動時觸發
- **OnServerStopped**: 伺服器停止時觸發
- **OnServerStartFailed**: 伺服器啟動失敗時觸發，參數：錯誤訊息

### 客戶端連線事件
- **OnClientConnected**: 客戶端連接到伺服器時觸發，參數：客戶端 ID
- **OnClientDisconnected**: 客戶端斷開連接時觸發，參數：客戶端 ID

### 檔案上傳事件（從客戶端）
- **OnFileUploadStarted**: 客戶端開始上傳檔案時觸發，參數：檔案路徑
- **OnFileUploadCompleted**: 客戶端檔案上傳成功時觸發，參數：檔案路徑
- **OnFileUploadFailed**: 客戶端檔案上傳失敗時觸發，參數：檔案路徑、錯誤訊息
- **OnFileAdded**: 檔案新增到伺服器儲存時觸發（已存在），參數：檔案路徑

### 檔案推送事件（從伺服器到客戶端）
- **OnFilePushStarted**: 伺服器開始推送檔案到客戶端時觸發，參數：檔案路徑
- **OnFilePushCompleted**: 伺服器檔案推送完成時觸發，參數：檔案路徑
- **OnFilePushFailed**: 伺服器檔案推送失敗時觸發，參數：檔案路徑、錯誤訊息

### 廣播事件
- **OnBroadcastSent**: 廣播訊息成功發送時觸發，參數：訊息類型、接收客戶端數量
- **OnBroadcastFailed**: 廣播訊息發送失敗時觸發，參數：訊息類型、錯誤訊息

## 使用範例

### Client 端使用

```csharp
var config = new GrpcConfigBuilder()
    .WithHost("localhost")
    .WithPort(50051)
    .Build();

IClientApi client = GrpcApiFactory.CreateClient(config);

// 訂閱連線事件
client.OnConnected += () => Console.WriteLine("✓ 已連接到伺服器");
client.OnDisconnected += () => Console.WriteLine("✗ 已斷開連接");
client.OnConnectionError += err => Console.WriteLine($"✗ 連接失敗: {err}");

// 訂閱檔案上傳事件
client.OnUploadStarted += path => Console.WriteLine($"⬆ 開始上傳: {path}");
client.OnUploadProgress += (path, pct) => Console.WriteLine($"⬆ 上傳進度: {path} - {pct:F1}%");
client.OnUploadCompleted += path => Console.WriteLine($"✓ 上傳完成: {path}");
client.OnUploadFailed += (path, err) => Console.WriteLine($"✗ 上傳失敗: {path} - {err}");

// 訂閱檔案下載事件
client.OnDownloadStarted += path => Console.WriteLine($"⬇ 開始下載: {path}");
client.OnDownloadProgress += (path, pct) => Console.WriteLine($"⬇ 下載進度: {path} - {pct:F1}%");
client.OnDownloadCompleted += path => Console.WriteLine($"✓ 下載完成: {path}");
client.OnDownloadFailed += (path, err) => Console.WriteLine($"✗ 下載失敗: {path} - {err}");

// 訂閱螢幕截圖事件
client.OnScreenshotStarted += () => Console.WriteLine("📷 開始截圖");
client.OnScreenshotProgress += pct => Console.WriteLine($"📷 截圖進度: {pct:F1}%");
client.OnScreenshotCompleted += size => Console.WriteLine($"✓ 截圖完成: {size} bytes");
client.OnScreenshotFailed += err => Console.WriteLine($"✗ 截圖失敗: {err}");

// 訂閱伺服器推送事件
client.OnServerFileStarted += path => Console.WriteLine($"⬇ 伺服器推送檔案: {path}");
client.OnServerFileCompleted += path => Console.WriteLine($"✓ 伺服器檔案接收完成: {path}");
client.OnServerFileError += (path, err) => Console.WriteLine($"✗ 伺服器檔案推送錯誤: {path} - {err}");

await client.ConnectAsync();
```

### Server 端使用

```csharp
IServerApi server = GrpcApiFactory.CreateServer();

// 訂閱伺服器生命週期事件
server.OnServerStarted += () => Console.WriteLine("✓ 伺服器已啟動");
server.OnServerStopped += () => Console.WriteLine("✗ 伺服器已停止");
server.OnServerStartFailed += err => Console.WriteLine($"✗ 伺服器啟動失敗: {err}");

// 訂閱客戶端連線事件
server.OnClientConnected += clientId => Console.WriteLine($"➕ 客戶端已連接: {clientId}");
server.OnClientDisconnected += clientId => Console.WriteLine($"➖ 客戶端已斷開: {clientId}");

// 訂閱檔案上傳事件（從客戶端）
server.OnFileUploadStarted += path => Console.WriteLine($"⬆ 客戶端開始上傳: {path}");
server.OnFileUploadCompleted += path => Console.WriteLine($"✓ 客戶端上傳完成: {path}");
server.OnFileUploadFailed += (path, err) => Console.WriteLine($"✗ 客戶端上傳失敗: {path} - {err}");
server.OnFileAdded += path => Console.WriteLine($"📁 檔案已新增到儲存: {path}");

// 訂閱檔案推送事件（從伺服器到客戶端）
server.OnFilePushStarted += path => Console.WriteLine($"⬇ 開始推送檔案到客戶端: {path}");
server.OnFilePushCompleted += path => Console.WriteLine($"✓ 檔案推送完成: {path}");
server.OnFilePushFailed += (path, err) => Console.WriteLine($"✗ 檔案推送失敗: {path} - {err}");

// 訂閱廣播事件
server.OnBroadcastSent += (type, count) => Console.WriteLine($"📢 廣播已發送: type={type}, 接收客戶端={count}");
server.OnBroadcastFailed += (type, err) => Console.WriteLine($"✗ 廣播失敗: type={type}, error={err}");

server.UpdateConfig("0.0.0.0", 50051);
await server.StartAsync();
```

## 事件流程圖

### 檔案上傳流程（Client → Server）
```
Client:  OnUploadStarted → OnUploadProgress (多次) → OnUploadCompleted
                                                    ↓
Server:                     OnFileUploadStarted → OnFileUploadCompleted → OnFileAdded
```

### 檔案推送流程（Server → Client）
```
Server:  OnFilePushStarted → (推送中) → OnFilePushCompleted
                                        ↓
Client:                     OnServerFileStarted → OnServerFileCompleted
```

### 連線流程
```
Client:  ConnectAsync() → OnConnected
                        ↓
Server:                 OnClientConnected

Client:  DisconnectAsync() → OnDisconnected
                            ↓
Server:                     OnClientDisconnected
```

## 錯誤處理
所有操作都有對應的失敗事件：
- 上傳失敗：OnUploadFailed (Client) + OnFileUploadFailed (Server)
- 下載失敗：OnDownloadFailed (Client)
- 推送失敗：OnFilePushFailed (Server) + OnServerFileError (Client)
- 截圖失敗：OnScreenshotFailed (Client)
- 連線失敗：OnConnectionError (Client) + OnServerStartFailed (Server)
- 廣播失敗：OnBroadcastFailed (Server)

## 注意事項
1. 所有事件都是可選的（nullable），您可以只訂閱需要的事件
2. 事件處理器中的例外會被記錄到日誌，但不會影響主要流程
3. 事件觸發是同步的，避免在事件處理器中執行耗時操作
4. 建議在 UI 應用程式中使用 BeginInvoke 將事件處理器封裝到 UI 執行緒

## 升級指南
現有代碼無需修改，新事件是選擇性訂閱的。如果需要更詳細的狀態追蹤，可以訂閱新增的事件。
