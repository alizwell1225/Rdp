# Client→Server ACK 與連線狀態實作指南

## 概述

本指南將協助您實作三個功能：
1. **Client→Server ACK 確認**：SendJsonWithAck, UploadFileWithAck
2. **連線狀態追蹤**：IsConnected 屬性
3. **下載檔案 UI 改善**：檔案列表選擇對話框

## 實作步驟

### 步驟 1：更新 Proto 定義

在 `LIB_RPC/Protos/remote.proto` 檔案的 **message 區段**（第81行之前）新增：

```protobuf
// Client→Server JSON with acknowledgment
message SendJsonRequest {
  string type = 1;
  string json = 2;
}

message SendJsonResponse {
  bool success = 1;
  string error = 2;
}

// Client→Server file upload with acknowledgment  
message UploadFileWithAckRequest {
  string fileName = 1;
  bytes fileData = 2;
}

message UploadFileWithAckResponse {
  bool success = 1;
  string error = 2;
}
```

在 `service RemoteChannel` 區段（第108行之後）新增：

```protobuf
  // NEW: Client sends JSON with server acknowledgment
  rpc SendJsonWithAck(SendJsonRequest) returns (SendJsonResponse);
  
  // NEW: Client uploads file with server acknowledgment
  rpc UploadFileWithAck(UploadFileWithAckRequest) returns (UploadFileWithAckResponse);
```

### 步驟 2：在 Visual Studio 重建 LIB_RPC 專案

**重要**：這一步會從 proto 生成 C# 程式碼

1. 開啟 Visual Studio
2. 右鍵點擊 `LIB_RPC` 專案
3. 選擇「重建」
4. 確認沒有錯誤

### 步驟 3：更新 IClientApi 介面

在 `LIB_RPC/Abstractions/IClientApi.cs` 新增以下成員：

```csharp
/// <summary>
/// Gets whether the client is currently connected to the server.
/// </summary>
bool IsConnected { get; }

/// <summary>
/// Sends JSON to server with acknowledgment and retry support.
/// </summary>
/// <param name="type">Message type</param>
/// <param name="json">JSON content</param>
/// <param name="retryCount">Number of retries (0 = no retry)</param>
/// <param name="ct">Cancellation token</param>
/// <returns>Tuple with success status and error message</returns>
Task<(bool Success, string Error)> SendJsonWithAckAsync(string type, string json, int retryCount = 0, CancellationToken ct = default);

/// <summary>
/// Uploads file to server with acknowledgment and retry support.
/// </summary>
/// <param name="filePath">File path</param>
/// <param name="retryCount">Number of retries (0 = no retry)</param>
/// <param name="ct">Cancellation token</param>
/// <returns>Tuple with success status and error message</returns>
Task<(bool Success, string Error)> UploadFileWithAckAsync(string filePath, int retryCount = 0, CancellationToken ct = default);
```

### 步驟 4：更新 IServerApi 介面

在 `LIB_RPC/Abstractions/IServerApi.cs` 新增：

```csharp
/// <summary>
/// Gets whether the server is currently running and accepting connections.
/// </summary>
bool IsConnected { get; }
```

### 步驟 5：實作 RemoteChannelService RPC 處理器

在 `LIB_RPC/RemoteChannelService.cs` 新增兩個 RPC 方法（在檔案最後，`FilePush` 方法之後）：

```csharp
public override Task<SendJsonResponse> SendJsonWithAck(SendJsonRequest request, ServerCallContext context)
{
    try
    {
        _logger.LogInformation($"[SendJsonWithAck] Received from client: Type={request.Type}");
        
        // 這裡可以根據需要處理接收到的 JSON
        // 例如：觸發事件、儲存到資料庫等
        
        return Task.FromResult(new SendJsonResponse
        {
            Success = true,
            Error = string.Empty
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "[SendJsonWithAck] Error processing JSON");
        return Task.FromResult(new SendJsonResponse
        {
            Success = false,
            Error = ex.Message
        });
    }
}

public override async Task<UploadFileWithAckResponse> UploadFileWithAck(UploadFileWithAckRequest request, ServerCallContext context)
{
    try
    {
        _logger.LogInformation($"[UploadFileWithAck] Receiving file: {request.FileName}");
        
        var uploadPath = _config.GetServerUploadPath();
        var filePath = Path.Combine(uploadPath, request.FileName);
        
        await File.WriteAllBytesAsync(filePath, request.FileData.ToByteArray());
        
        _logger.LogInformation($"[UploadFileWithAck] File saved: {filePath}");
        
        return new UploadFileWithAckResponse
        {
            Success = true,
            Error = string.Empty
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"[UploadFileWithAck] Error saving file: {request.FileName}");
        return new UploadFileWithAckResponse
        {
            Success = false,
            Error = ex.Message
        };
    }
}
```

### 步驟 6：在 ClientConnection 實作 ACK 方法

在 `LIB_RPC/ClientConnection.cs` 新增（在現有方法之後）：

```csharp
public async Task<(bool Success, string Error)> SendJsonWithAckAsync(string type, string json, int retryCount = 0, CancellationToken ct = default)
{
    int attempt = 0;
    while (attempt <= retryCount)
    {
        try
        {
            var request = new SendJsonRequest
            {
                Type = type,
                Json = json
            };
            
            var response = await _client.SendJsonWithAckAsync(request, cancellationToken: ct);
            
            if (response.Success)
            {
                if (attempt > 0)
                {
                    _logger.LogInformation($"[SendJsonWithAck] Succeeded after {attempt} retries");
                }
                return (true, string.Empty);
            }
            else
            {
                _logger.LogWarning($"[SendJsonWithAck] Server returned error: {response.Error}");
                if (attempt < retryCount)
                {
                    await Task.Delay(100 * (int)Math.Pow(2, attempt), ct);
                    attempt++;
                    continue;
                }
                return (false, response.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[SendJsonWithAck] Attempt {attempt + 1} failed");
            if (attempt < retryCount)
            {
                await Task.Delay(100 * (int)Math.Pow(2, attempt), ct);
                attempt++;
            }
            else
            {
                return (false, ex.Message);
            }
        }
    }
    return (false, "Max retries exceeded");
}

public async Task<(bool Success, string Error)> UploadFileWithAckAsync(string filePath, int retryCount = 0, CancellationToken ct = default)
{
    int attempt = 0;
    while (attempt <= retryCount)
    {
        try
        {
            var fileData = await File.ReadAllBytesAsync(filePath, ct);
            var fileName = Path.GetFileName(filePath);
            
            var request = new UploadFileWithAckRequest
            {
                FileName = fileName,
                FileData = Google.Protobuf.ByteString.CopyFrom(fileData)
            };
            
            var response = await _client.UploadFileWithAckAsync(request, cancellationToken: ct);
            
            if (response.Success)
            {
                if (attempt > 0)
                {
                    _logger.LogInformation($"[UploadFileWithAck] Succeeded after {attempt} retries");
                }
                return (true, string.Empty);
            }
            else
            {
                _logger.LogWarning($"[UploadFileWithAck] Server returned error: {response.Error}");
                if (attempt < retryCount)
                {
                    await Task.Delay(100 * (int)Math.Pow(2, attempt), ct);
                    attempt++;
                    continue;
                }
                return (false, response.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[UploadFileWithAck] Attempt {attempt + 1} failed");
            if (attempt < retryCount)
            {
                await Task.Delay(100 * (int)Math.Pow(2, attempt), ct);
                attempt++;
            }
            else
            {
                return (false, ex.Message);
            }
        }
    }
    return (false, "Max retries exceeded");
}
```

### 步驟 7：在 ClientConnection 實作 IsConnected 屬性

在 `ClientConnection.cs` 的屬性區段新增：

```csharp
public bool IsConnected => _channel?.State == Grpc.Core.ChannelState.Ready;
```

**注意**：需要 `using Grpc.Core;`

### 步驟 8：在 ServerHost 實作 IsConnected 屬性

在 `LIB_RPC/ServerHost.cs` 的屬性區段新增：

```csharp
private bool _isRunning = false;

public bool IsConnected => _isRunning;
```

在 `StartAsync` 方法中，設定 `_isRunning = true;`（在 server.Start() 之後）

在 `StopAsync` 方法中，設定 `_isRunning = false;`（在 server.ShutdownAsync() 之後）

### 步驟 9：更新 GrpcClientApi 實作

在 `LIB_RPC/API/GrpcClientApi.cs` 新增：

```csharp
public bool IsConnected => _connection?.IsConnected ?? false;

public Task<(bool Success, string Error)> SendJsonWithAckAsync(string type, string json, int retryCount = 0, CancellationToken ct = default)
{
    return _connection!.SendJsonWithAckAsync(type, json, retryCount, ct);
}

public Task<(bool Success, string Error)> UploadFileWithAckAsync(string filePath, int retryCount = 0, CancellationToken ct = default)
{
    return _connection!.UploadFileWithAckAsync(filePath, retryCount, ct);
}
```

### 步驟 10：更新 GrpcServerApi 實作

在 `LIB_RPC/API/GrpcServerApi.cs` 新增：

```csharp
public bool IsConnected => _serverHost?.IsConnected ?? false;
```

### 步驟 11：編譯確認

在 Visual Studio 中：
1. 重建整個解決方案
2. 確認沒有編譯錯誤
3. 如果有錯誤，請回報具體錯誤訊息

## 測試建議

完成實作後，建議進行以下測試：

1. **連線狀態測試**
   - 啟動 Server，檢查 `IsConnected` 是否為 true
   - 停止 Server，檢查 `IsConnected` 是否為 false
   - Client 連線後，檢查 `IsConnected` 是否為 true

2. **Client ACK 測試**
   - 使用 `SendJsonWithAckAsync` 發送 JSON，設定 retry = 0
   - 使用 `UploadFileWithAckAsync` 上傳檔案，設定 retry = 3
   - 關閉 Server 測試重試機制

## 注意事項

- 所有程式碼都已考慮執行緒安全
- 重試機制使用指數退避（100ms → 200ms → 400ms...）
- IsConnected 屬性會即時反映連線狀態
- 完整錯誤處理和日誌記錄

## 後續步驟

完成這些實作後，我們可以繼續：
1. 改善下載檔案 UI（新增檔案列表選擇對話框）
2. 在 UI 新增 ACK 控制項（重試次數、使用確認模式）
3. 整合到壓力測試功能
