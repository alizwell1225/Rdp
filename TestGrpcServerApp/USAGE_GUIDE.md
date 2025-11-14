# TestGrpcServerApp - 使用指南 / Usage Guide

## 概述 / Overview

TestGrpcServerApp 是一個 .NET 8 Windows Forms 測試應用程式，用於簡化 gRPC 伺服器功能的測試和演示。它提供了一個用戶友好的界面來配置、啟動、停止和測試 gRPC 伺服器操作，無需編寫代碼。

TestGrpcServerApp is a .NET 8 Windows Forms test application designed to simplify testing and demonstration of gRPC server functionality. It provides a user-friendly interface to configure, start, stop, and test gRPC server operations without writing code.

## 主要功能 / Main Features

### 伺服器控制 / Server Control
- **開啟伺服器設定 (Open Server Config)**: 配置伺服器主機、端口、存儲路徑和日誌路徑
- **啟動伺服器 (Start Server)**: 使用當前配置啟動 gRPC 伺服器
- **停止伺服器 (Stop Server)**: 優雅地停止運行中的伺服器
- **自動啟動 (Auto-start on Launch)**: 啟用後應用程式啟動時自動啟動伺服器

### 測試操作 / Test Operations
- **發送 JSON (Send JSON)**: 向所有連接的客戶端廣播 JSON 消息，帶有驗證和格式化功能
- **發送文件 (Send File)**: 向所有連接的客戶端推送任何文件
- **發送圖片 (Send Image)**: 專門發送從選定路徑的圖片文件

### 日誌記錄 / Logging
- **即時日誌顯示**: 實時查看伺服器事件和操作
- **查看日誌文件 (View Log Files)**: 在文件資源管理器中打開日誌目錄
- **清除日誌 (Clear Log)**: 清除當前日誌顯示

## 快速開始 / Quick Start

### 1. 啟動應用程式 / Launch Application
執行 `TestGrpcServerApp.exe`

### 2. 配置伺服器 / Configure Server
1. 點擊 **"開啟伺服器設定"** 按鈕
2. 設定以下選項：
   - **Host**: 伺服器主機地址（默認：localhost）
   - **Port**: 伺服器端口號（默認：50051）
   - **Storage Path**: 文件存儲目錄
   - **Log Path**: 日誌文件位置
3. 點擊 **"OK"** 保存配置

### 3. 啟動伺服器 / Start Server
1. (可選) 勾選 **"自動啟動"** 複選框，以便應用程式啟動時自動啟動伺服器
2. 點擊 **"啟動伺服器"** 按鈕開始接受客戶端連接

### 4. 監控日誌 / Monitor Logs
在日誌窗口中查看伺服器事件：
- 伺服器啟動/停止
- 客戶端連接/斷開連接
- 文件上傳
- 廣播操作

### 5. 測試功能 / Test Features

#### 發送 JSON 消息 / Send JSON Message
1. 確保伺服器正在運行
2. 點擊 **"發送 JSON"** 按鈕
3. 在對話框中：
   - 輸入消息類型（例如："test_message"）
   - 輸入或貼上 JSON 內容
   - 點擊 **"Validate"** 檢查 JSON 語法
   - 點擊 **"Format"** 自動格式化 JSON
   - 點擊 **"Send"** 廣播消息
4. 在日誌中查看確認信息

#### 發送文件 / Send File
1. 確保伺服器正在運行
2. 點擊 **"發送文件"** 按鈕
3. 從文件選擇器中選擇任何文件
4. 文件將被推送到所有連接的客戶端
5. 在日誌中查看確認信息

#### 發送圖片 / Send Image
1. 確保伺服器正在運行
2. 點擊 **"發送圖片 (路徑)"** 按鈕
3. 選擇圖片文件（PNG, JPG, JPEG, BMP, GIF）
4. 圖片將被推送到所有連接的客戶端
5. 在日誌中查看確認信息

## 配置文件 / Configuration File

配置存儲在相對於應用程式目錄的 `Config/ServerConfig.json` 中：

```json
{
  "Host": "localhost",
  "Port": 50051,
  "StorageRoot": "C:\\Path\\To\\Storage",
  "LogFilePath": "C:\\Path\\To\\Log\\grpc.log"
}
```

## 架構 / Architecture

```
TestGrpcServerApp (UI Layer)
    ↓ uses
LIB_Define.RPC.GrpcServerHelper (Business Logic Layer)
    ↓ wraps
LIB_RPC.IServerApi (Core gRPC Implementation)
```

**關鍵優勢 / Key Benefits:**
- 解耦架構：UI 與 gRPC 實現分離
- 事件驅動：實時更新，無需輪詢
- 易於集成：可在其他項目中重用 GrpcServerHelper
- 配置持久化：設定自動保存

## 疑難排解 / Troubleshooting

### 伺服器無法啟動 / Server Fails to Start
- 檢查端口是否已被使用
- 驗證主機配置是否正確
- 查看日誌文件以獲取詳細錯誤消息

### 無法發送消息 / Cannot Send Messages
- 確保伺服器正在運行（啟動伺服器按鈕應被禁用）
- 驗證至少有一個客戶端已連接
- 檢查網絡連接

### 日誌目錄無法打開 / Log Directory Not Opening
- 應用程式嘗試使用 Windows 資源管理器打開日誌目錄
- 確保日誌路徑存在且可訪問
- 檢查 Windows 文件權限

## 技術規格 / Technical Specifications

- **Framework**: .NET 8.0 (Windows)
- **UI Framework**: Windows Forms
- **Dependencies**: 
  - LIB_Define (provides GrpcServerHelper)
  - LIB_Log (provides LoggerServer)
- **Configuration Format**: JSON
- **Log Format**: Text with timestamps
- **Supported Image Formats**: PNG, JPG, JPEG, BMP, GIF

## 開發者信息 / Developer Information

### 擴展應用程式 / Extending the Application

要擴展此應用程式：

1. 引用 `GrpcServerHelper` 類進行伺服器操作
2. 訂閱事件以獲取實時更新
3. 調用異步方法進行操作（StartServerAsync、BroadcastJsonAsync 等）
4. 通過 try-catch 和事件回調處理錯誤

範例代碼 / Example Code:
```csharp
var serverHelper = new GrpcServerHelper();
serverHelper.OnLog += (msg) => Console.WriteLine(msg);
await serverHelper.StartServerAsync();
await serverHelper.BroadcastJsonAsync("test", "{\"data\":\"value\"}");
await serverHelper.StopServerAsync();
```

### 事件處理 / Event Handling

應用程式響應以下伺服器事件：

- `OnLog`: 來自伺服器操作的日誌消息
- `OnServerStarted`: 伺服器成功啟動
- `OnServerStopped`: 伺服器停止
- `OnServerStartFailed`: 伺服器啟動失敗
- `OnClientConnected`: 客戶端連接到伺服器
- `OnClientDisconnected`: 客戶端從伺服器斷開連接
- `OnFileAdded`: 文件添加到伺服器存儲
- `OnFileUploadCompleted`: 來自客戶端的文件上傳完成

## 許可證 / License

這是 Rdp 項目的一部分。有關許可證信息，請參閱主存儲庫。

This is part of the Rdp project. See the main repository for license information.

---

**版本 / Version**: 1.0  
**更新日期 / Last Updated**: 2024-11-14
