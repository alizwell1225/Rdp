# 架構優化文件 (Architecture Optimization)

## 概述

本文件說明針對 GrpcClientApp、GrpcServerApp 和 LIB_RPC 之間的耦合度優化。

## 主要改進

### 1. 抽象層 (Abstraction Layer)

#### 介面定義 (Interface Definitions)
- **IClientApi**: 客戶端 API 的抽象介面，隱藏實作細節
- **IServerApi**: 伺服器端 API 的抽象介面，提供清晰的契約
- **IScreenCapture**: 螢幕截圖功能的抽象，允許不同平台實作

#### 資料傳輸物件 (DTOs)
- **JsonMessage**: 隔離 UI 層與 Protocol Buffer 類型
- **JsonAcknowledgment**: JSON 確認回應的 DTO
- **FileTransferResult**: 檔案傳輸結果的 DTO

**優點**:
- UI 層不再直接依賴 `RdpGrpc.Proto` 命名空間
- 更容易進行單元測試（可使用 mock）
- 更好的向後相容性

### 2. 移除平台依賴

**之前**: LIB_RPC 目標框架為 `net8.0-windows`，使用 `UseWindowsForms`

**現在**: 
- LIB_RPC 目標框架為 `net8.0`（跨平台）
- ScreenCapture 透過條件編譯處理 Windows 特定功能
- 使用依賴注入傳入 IScreenCapture 實作

**優點**:
- 函式庫可在 Linux/macOS 上使用（除了截圖功能）
- 更好的跨平台支援
- 降低對特定 UI 框架的依賴

### 3. 設計模式應用

#### 建造者模式 (Builder Pattern)
```csharp
var config = new GrpcConfigBuilder()
    .WithHost("192.168.1.100")
    .WithPort(50051)
    .WithPassword("secure-password")
    .WithMaxChunkSize(128 * 1024)
    .Build();
```

#### 工廠模式 (Factory Pattern)
```csharp
var client = GrpcApiFactory.CreateClient(config);
var server = GrpcApiFactory.CreateServer();
```

**優點**:
- 更直觀的 API
- 編譯時期驗證
- 集中的實例創建邏輯

### 4. 依賴注入 (Dependency Injection)

**ServerHost** 和 **RemoteChannelService** 現在接受 `IScreenCapture` 參數:

```csharp
public ServerHost(GrpcConfig config, GrpcLogger logger, IScreenCapture? screenCapture = null)
{
    _screenCapture = screenCapture ?? new ScreenCapture();
}
```

**優點**:
- 更容易進行單元測試
- 可替換不同的實作
- 遵循 SOLID 原則中的依賴反轉原則

### 5. 移除重複代碼

**移除**: `GrpcServerController.cs`（與 `GrpcServerApi.cs` 功能重複）

**優點**:
- 減少維護負擔
- 避免邏輯分歧
- 更清晰的代碼結構

## 架構圖

### 之前的架構
```
┌─────────────────┐         ┌─────────────────┐
│ GrpcClientApp   │         │ GrpcServerApp   │
│  (UI Layer)     │         │  (UI Layer)     │
│                 │         │                 │
│  ┌──────────┐   │         │  ┌──────────┐   │
│  │ClientForm├───┼─────┐   │  │ServerForm├───┼─────┐
│  └──────────┘   │     │   │  └──────────┘   │     │
└─────────────────┘     │   └─────────────────┘     │
                        │                           │
                        ↓                           ↓
              直接依賴 RdpGrpc.Proto          直接依賴 LIB_RPC
              直接使用 GrpcClientApi         使用 GrpcServerController
                        │                    或 GrpcServerApi (重複)
                        │                           │
                        └───────────┬───────────────┘
                                    ↓
                        ┌─────────────────────┐
                        │      LIB_RPC        │
                        │ (net8.0-windows)    │
                        │ 依賴 Windows Forms  │
                        └─────────────────────┘
```

### 優化後的架構
```
┌─────────────────┐         ┌─────────────────┐
│ GrpcClientApp   │         │ GrpcServerApp   │
│  (UI Layer)     │         │  (UI Layer)     │
│                 │         │                 │
│  ┌──────────┐   │         │  ┌──────────┐   │
│  │ClientForm│   │         │  │ServerForm│   │
│  └────┬─────┘   │         │  └────┬─────┘   │
└───────┼─────────┘         └───────┼─────────┘
        │                           │
        │ 使用介面                   │ 使用介面
        ↓                           ↓
   IClientApi                  IServerApi
        ↑                           ↑
        │                           │
┌───────┴─────────────────────────┴─────────────┐
│            LIB_RPC.Abstractions               │
│  ┌──────────┐  ┌──────────┐  ┌────────────┐  │
│  │IClientApi│  │IServerApi│  │IScreenCapture│ │
│  └──────────┘  └──────────┘  └────────────┘  │
│                                               │
│  ┌─────────────────────────────────────┐     │
│  │  DTOs (JsonMessage, etc.)           │     │
│  └─────────────────────────────────────┘     │
└───────────────────────────────────────────────┘
                    ↑
                    │ 實作
┌───────────────────┴───────────────────────────┐
│            LIB_RPC (net8.0)                   │
│                                               │
│  ┌──────────────┐      ┌──────────────┐      │
│  │GrpcClientApi │      │GrpcServerApi │      │
│  │implements    │      │implements    │      │
│  │IClientApi    │      │IServerApi    │      │
│  └──────────────┘      └──────────────┘      │
│                                               │
│  ┌──────────────┐      ┌──────────────┐      │
│  │ScreenCapture │      │  ServerHost  │      │
│  │implements    │      │  (DI ready)  │      │
│  │IScreenCapture│      └──────────────┘      │
│  └──────────────┘                            │
│                                               │
│  ┌─────────────────────────────────────┐     │
│  │  GrpcConfigBuilder, GrpcApiFactory  │     │
│  └─────────────────────────────────────┘     │
└───────────────────────────────────────────────┘
```

## 耦合度改進

### 之前的耦合問題
1. **緊耦合**: UI 直接使用 Protocol Buffer 類型
2. **平台耦合**: 函式庫綁定 Windows Forms
3. **重複代碼**: GrpcServerController 和 GrpcServerApi
4. **缺乏抽象**: 難以替換實作或進行測試
5. **硬編碼依賴**: 直接實例化具體類別

### 優化後的效果
1. **鬆耦合**: 透過介面和 DTO 隔離實作
2. **跨平台**: 核心函式庫不依賴特定 UI 框架
3. **無重複**: 統一使用 GrpcServerApi
4. **可測試性**: 所有依賴都可注入和模擬
5. **靈活性**: 使用工廠和建造者模式

## 測試改進

### 之前
```csharp
// 難以測試 - 直接依賴具體類別
var form = new ClientForm();
// 無法注入 mock 依賴
```

### 現在
```csharp
// 容易測試 - 使用介面
IClientApi mockClient = new MockClientApi();
var form = new ClientForm(mockClient); // 假設接受 IClientApi

// 或使用工廠進行整合測試
var client = GrpcApiFactory.CreateClient(testConfig);
```

## 使用範例

### 客戶端使用新 API
```csharp
// 使用建造者創建配置
var config = new GrpcConfigBuilder()
    .WithHost("server.example.com")
    .WithPort(50051)
    .WithPassword("my-secret")
    .Build();

// 使用工廠創建客戶端
IClientApi client = GrpcApiFactory.CreateClient(config);

// 訂閱事件（現在使用 DTO）
client.OnServerJson += msg => Console.WriteLine($"Type: {msg.Type}, JSON: {msg.Json}");

// 連接並使用
await client.ConnectAsync();
var result = await client.SendJsonAsync("ping", "{}");
Console.WriteLine($"Success: {result.Success}");
```

### 伺服器端使用新 API
```csharp
// 使用工廠創建伺服器
IServerApi server = GrpcApiFactory.CreateServer();

// 訂閱事件
server.OnLog += msg => Console.WriteLine(msg);
server.OnFileAdded += path => Console.WriteLine($"New file: {path}");

// 配置並啟動
server.UpdateConfig("0.0.0.0", 50051);
await server.StartAsync();

// 廣播訊息
await server.BroadcastJsonAsync("notification", "{\"message\": \"Hello\"}");
```

## 向後相容性

現有代碼仍可繼續使用 `GrpcClientApi` 和 `GrpcServerApi`，但建議遷移到介面:

```csharp
// 舊代碼（仍然可用）
var client = new GrpcClientApi(config);

// 新代碼（推薦）
IClientApi client = GrpcApiFactory.CreateClient(config);
```

## 未來擴展建議

1. **服務定位器模式**: 考慮添加服務容器用於更複雜的 DI 場景
2. **配置驗證**: GrpcConfigBuilder 增加更多驗證邏輯
3. **日誌抽象**: 將 GrpcLogger 也抽象化為 ILogger
4. **重試策略**: 在 IClientApi 中添加重試和斷線重連機制
5. **健康檢查**: 添加伺服器健康檢查端點

## 效能影響

這些改進對效能的影響微乎其微:
- DTO 轉換開銷很小（簡單的屬性複製）
- 介面調用與直接調用性能相同（JIT 優化後）
- 建造者模式只在初始化時使用一次

## 總結

這次優化大幅降低了各層之間的耦合度：

✅ **介面隔離**: UI 層只依賴抽象介面  
✅ **平台獨立**: 核心函式庫跨平台  
✅ **可測試性**: 所有依賴可注入  
✅ **可維護性**: 移除重複代碼  
✅ **擴展性**: 使用設計模式便於擴展  
✅ **文檔化**: 清晰的 XML 註解和文檔  

這些改進遵循 SOLID 原則，提供了更好的代碼質量和可維護性。
