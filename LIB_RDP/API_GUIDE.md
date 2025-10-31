# LIB_RDP API 使用指南

## 目錄
1. [基本使用](#基本使用)
2. [進階功能](#進階功能)
3. [API 參考](#api-參考)
4. [最佳實踐](#最佳實踐)
5. [範例程式](#範例程式)

---

## 基本使用

### 1. 建立單一 RDP 連線

#### 使用 RdpConnection 直接連線
```csharp
using LIB_RDP.Core;
using LIB_RDP.Models;

// 建立連線
var connection = new RdpConnection();

// 配置連線設定
connection.Configure(new RdpConfig
{
    ScreenWidth = 1920,
    ScreenHeight = 1080,
    ColorDepth = 32,
    EnableCompression = true
});

// 同步連線
bool success = connection.Connect("192.168.1.100", "username", "password");

// 或使用非同步連線（推薦）
bool connected = await connection.ConnectAsync("192.168.1.100", "username", "password", timeoutSeconds: 30);
```

#### 使用 Builder 模式（推薦）
```csharp
using LIB_RDP.Builders;

// 使用流暢的 API 建立連線
var connection = await new RdpConnectionBuilder()
    .WithHost("192.168.1.100")
    .WithCredentials("username", "password")
    .WithResolution(1920, 1080)
    .WithColorDepth(32)
    .WithTimeout(30)
    .BuildAndConnectAsync();
```

### 2. 使用 RdpManager 管理多個連線

```csharp
using LIB_RDP.Core;
using LIB_RDP.Interfaces;

// 建立管理器（最多50個連線）
IRdpManager manager = new RdpManager(maxConnections: 50);

// 方式1：建立連線然後手動連接
var conn1 = manager.CreateConnection();
conn1.Configure(new RdpConfig { ScreenWidth = 1920, ScreenHeight = 1080 });
await conn1.ConnectAsync("192.168.1.100", "user1", "pass1");

// 方式2：建立並立即連接（推薦）
var conn2 = await manager.CreateAndConnectAsync("192.168.1.101", "user2", "pass2");

// 查詢活動連線
Console.WriteLine($"活動連線數: {manager.ActiveConnectionCount}");
Console.WriteLine($"總連線數: {manager.TotalConnectionCount}");

// 移除特定連線
manager.RemoveConnection(conn1.ConnectionId);

// 斷開所有連線
manager.DisconnectAll();
```

---

## 進階功能

### 3. 批次操作多個連線

```csharp
using LIB_RDP.Helpers;

var manager = new RdpManager();
var batchOps = new RdpBatchOperations(manager);

// 批次連線到多個主機
var connectionInfos = new List<(string, string, string)>
{
    ("192.168.1.100", "user1", "pass1"),
    ("192.168.1.101", "user2", "pass2"),
    ("192.168.1.102", "user3", "pass3")
};

var results = await batchOps.ConnectMultipleAsync(connectionInfos);

// 檢查結果
foreach (var result in results)
{
    if (result.Success)
        Console.WriteLine($"✓ {result.HostName} 連線成功");
    else
        Console.WriteLine($"✗ {result.HostName} 連線失敗: {result.Message}");
}

// 取得健康狀態
var healthStatuses = batchOps.GetHealthStatus();
foreach (var status in healthStatuses)
{
    Console.WriteLine($"{status.HostName}: {(status.IsHealthy ? "健康" : "異常")}");
}

// 取得統計摘要
var summary = batchOps.GetStatisticsSummary();
Console.WriteLine(summary.ToString());
```

### 4. 連線配置管理

```csharp
using LIB_RDP.Core;
using LIB_RDP.Models;

var configManager = RdpConfigurationManager.Instance;

// 建立連線配置
var profile = new RdpConnectionProfile
{
    Name = "辦公室電腦",
    HostName = "192.168.1.100",
    UserName = "admin",
    GroupName = "工作",
    AutoConnect = true,
    Config = new RdpConfig
    {
        ScreenWidth = 1920,
        ScreenHeight = 1080,
        ColorDepth = 32
    }
};

// 設定憑證（會自動加密）
profile.SetCredentials("admin", "password123");

// 儲存配置
configManager.SaveConnection(profile);

// 載入所有配置
var profiles = configManager.LoadAllConnections();

// 使用配置建立連線
var savedProfile = configManager.LoadConnection(profile.Id);
if (savedProfile != null)
{
    var connection = await new RdpConnectionBuilder()
        .FromProfile(savedProfile)
        .WithCredentials(savedProfile.UserName, savedProfile.GetSecureCredentials().GetPassword())
        .BuildAndConnectAsync();
}

// 導出配置
configManager.ExportConnections("rdp_backup.json");

// 導入配置
int importedCount = configManager.ImportConnections("rdp_backup.json");
```

### 5. 事件處理

```csharp
var connection = new RdpConnection();

// 監聽連線狀態變更
connection.ConnectionStateChanged += (sender, e) =>
{
    var conn = sender as RdpConnection;
    Console.WriteLine($"連線狀態: {conn.State}, 已連線: {conn.IsConnected}");
    
    if (conn.State == RdpState.Connected)
    {
        Console.WriteLine("連線成功！");
    }
    else if (conn.State == RdpState.Error)
    {
        Console.WriteLine("連線發生錯誤");
    }
};

// 監聽連線超時
connection.ConnectionTimeoutOccurred += (sender, ex) =>
{
    Console.WriteLine($"連線超時: {ex.Message}");
};

await connection.ConnectAsync("192.168.1.100", "user", "pass");
```

### 6. 畫面擷取

```csharp
var connection = await manager.CreateAndConnectAsync("192.168.1.100", "user", "pass");

if (connection.IsConnected)
{
    // 擷取遠端畫面
    using (var screenshot = connection.GetScreenshot())
    {
        if (screenshot != null)
        {
            screenshot.Save("remote_screen.png");
            Console.WriteLine("已儲存畫面截圖");
        }
    }
}
```

---

## API 參考

### IRdpManager 介面

#### 方法
- `IRdpConnection CreateConnection()` - 建立新連線
- `Task<IRdpConnection> CreateAndConnectAsync(string, string, string, int)` - 建立並連接
- `void RemoveConnection(string)` - 移除連線
- `IEnumerable<IRdpConnection> GetConnectionsByHost(string)` - 根據主機查找連線
- `IEnumerable<RdpConnectionStats> GetConnectionStats()` - 取得統計資訊
- `void DisconnectAll()` - 斷開所有連線
- `int CleanupInactiveConnections()` - 清理無效連線

#### 屬性
- `IEnumerable<IRdpConnection> ActiveConnections` - 活動連線
- `IEnumerable<IRdpConnection> AllConnections` - 所有連線
- `int MaxConnections` - 最大連線數
- `int ActiveConnectionCount` - 活動連線數
- `int TotalConnectionCount` - 總連線數

### IRdpConnection 介面

#### 方法
- `bool Connect(string, string, string)` - 同步連線
- `Task<bool> ConnectAsync(string, string, string, int)` - 非同步連線
- `void Disconnect()` - 斷開連線
- `void Configure(RdpConfig)` - 設定配置
- `RdpConfig GetRdpConfig()` - 取得配置
- `RdpConnectionStats GetConnectionStats()` - 取得統計
- `string GetHostName()` - 取得主機名稱
- `string GetUserName()` - 取得使用者名稱
- `Bitmap GetScreenshot()` - 擷取畫面

#### 屬性
- `bool IsConnected` - 是否已連線
- `RdpState State` - 連線狀態
- `string ConnectionId` - 連線ID
- `int RetryCount` - 重試次數
- `bool IsRetrying` - 是否重試中

#### 事件
- `event EventHandler ConnectionStateChanged` - 狀態變更事件
- `event EventHandler<RdpConnectionTimeoutException> ConnectionTimeoutOccurred` - 超時事件

---

## 最佳實踐

### 1. 資源管理
```csharp
// 使用 using 確保資源釋放
using (var manager = new RdpManager())
{
    var connection = await manager.CreateAndConnectAsync("host", "user", "pass");
    
    // 使用連線...
    
    // 離開 using 區塊時會自動清理
}
```

### 2. 錯誤處理
```csharp
try
{
    var connection = await manager.CreateAndConnectAsync("host", "user", "pass");
}
catch (RdpConnectionTimeoutException ex)
{
    Console.WriteLine($"連線超時: {ex.TimeoutSeconds} 秒");
}
catch (RdpAuthenticationException ex)
{
    Console.WriteLine($"認證失敗: {ex.Message}");
}
catch (RdpException ex)
{
    Console.WriteLine($"RDP 錯誤 [{ex.ErrorCode}]: {ex.Message}");
}
```

### 3. 連線狀態檢查
```csharp
var connection = manager.CreateConnection();
await connection.ConnectAsync("host", "user", "pass");

// 等待連線完成
int maxWait = 30;
int waited = 0;
while (!connection.IsConnected && connection.State != RdpState.Error && waited < maxWait)
{
    await Task.Delay(1000);
    waited++;
}

if (connection.IsConnected)
{
    Console.WriteLine("連線成功");
}
```

### 4. 定期健康檢查
```csharp
var manager = new RdpManager();
var batchOps = new RdpBatchOperations(manager);

// 每分鐘檢查一次
var timer = new System.Timers.Timer(60000);
timer.Elapsed += (s, e) =>
{
    var summary = batchOps.GetStatisticsSummary();
    Console.WriteLine($"健康檢查: {summary}");
    
    // 清理無效連線
    int cleaned = manager.CleanupInactiveConnections();
    if (cleaned > 0)
        Console.WriteLine($"已清理 {cleaned} 個無效連線");
};
timer.Start();
```

---

## 範例程式

### 完整範例：管理多個 RDP 連線

```csharp
using System;
using System.Threading.Tasks;
using LIB_RDP.Core;
using LIB_RDP.Builders;
using LIB_RDP.Helpers;
using LIB_RDP.Models;

class Program
{
    static async Task Main(string[] args)
    {
        // 建立管理器
        using (var manager = new RdpManager(maxConnections: 10))
        {
            // 建立批次操作輔助類別
            var batchOps = new RdpBatchOperations(manager);
            
            // 準備連線資訊
            var hosts = new List<(string Host, string User, string Pass)>
            {
                ("192.168.1.100", "admin", "pass1"),
                ("192.168.1.101", "admin", "pass2"),
                ("192.168.1.102", "admin", "pass3")
            };
            
            // 批次連線
            Console.WriteLine("開始批次連線...");
            var results = await batchOps.ConnectMultipleAsync(hosts);
            
            // 顯示結果
            int successCount = 0;
            foreach (var result in results)
            {
                if (result.Success)
                {
                    Console.WriteLine($"✓ {result.HostName} - {result.Message}");
                    successCount++;
                }
                else
                {
                    Console.WriteLine($"✗ {result.HostName} - {result.Message}");
                }
            }
            
            Console.WriteLine($"\n成功連線: {successCount}/{hosts.Count}");
            
            // 顯示統計摘要
            var summary = batchOps.GetStatisticsSummary();
            Console.WriteLine($"\n統計摘要:\n{summary}");
            
            // 取得並顯示健康狀態
            Console.WriteLine("\n連線健康狀態:");
            var healthStatuses = batchOps.GetHealthStatus();
            foreach (var status in healthStatuses)
            {
                string healthIcon = status.IsHealthy ? "✓" : "✗";
                Console.WriteLine($"{healthIcon} {status.HostName} - {status.State} " +
                                $"(連線時長: {status.ConnectedDuration:hh\\:mm\\:ss})");
            }
            
            // 等待一段時間
            Console.WriteLine("\n按任意鍵斷開所有連線...");
            Console.ReadKey();
            
            // 斷開所有連線
            manager.DisconnectAll();
            Console.WriteLine("已斷開所有連線");
        }
    }
}
```

---

## API 改進摘要

本次 API 改進新增了以下功能：

### 新增的介面
1. **IRdpConfigurationManager** - 配置管理介面

### 增強的介面
1. **IRdpManager** - 新增以下方法和屬性：
   - `CreateAndConnectAsync()` - 建立並連接
   - `GetConnectionsByHost()` - 根據主機查找
   - `GetConnectionStats()` - 取得統計
   - `DisconnectAll()` - 斷開所有
   - `CleanupInactiveConnections()` - 清理無效連線
   - `AllConnections` 屬性
   - 各種計數屬性

2. **IRdpConnection** - 新增以下方法和屬性：
   - `GetRdpConfig()` - 取得配置
   - `GetConnectionStats()` - 取得統計
   - `GetHostName()` - 取得主機
   - `GetUserName()` - 取得使用者
   - `ConnectionId` 屬性
   - `RetryCount` 屬性
   - `IsRetrying` 屬性
   - 事件支援

### 新增的類別
1. **RdpConnectionBuilder** - 流暢 API 建構器
2. **RdpBatchOperations** - 批次操作輔助類別
3. **BatchConnectionResult** - 批次連線結果
4. **ConnectionHealthStatus** - 連線健康狀態
5. **BatchStatisticsSummary** - 批次統計摘要

這些改進使得 LIB_RDP API 更加完整、易用，並且提供了更好的可擴展性，讓 RDP_DEMO 能夠更方便地使用這些功能。
