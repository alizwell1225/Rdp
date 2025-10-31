# Remote Desktop Control Library

## 專案概述
這是一個基於 .NET Framework 4.6.1 開發的遠端桌面控制程式庫，提供 RDP（Remote Desktop Protocol）連線管理與控制功能。

## 主要功能
1. 遠端連線管理
   - 多重連線支援：同時管理多個 RDP 連線
   - 連線狀態監控：即時追蹤連線狀態變化
   - 自動重連機制：連線中斷時自動嘗試重新連線
   - 連線參數設定：解析度、顏色深度等客製化設定

2. 遠端畫面控制
   - 即時畫面預覽：低延遲的畫面更新
   - 智慧縮放：自動適應視窗大小
   - 全螢幕模式：支援 ESC 快速切換
   - 畫面品質調整：可設定更新頻率和壓縮比例

3. 使用者介面
   - 多視窗預覽：同時顯示多個遠端畫面
   - 工具列自動隱藏：可自訂延遲時間
   - 快速切換：支援多連線間的快速切換
   - 狀態指示：清晰顯示連線狀態

檔案結構如下：
Lib_Rdp/
├── Core/
│   ├── RdpManager.cs
│   ├── RdpConnection.cs
│   └── RdpViewer.cs
├── UI/
│   ├── FormPreview.cs
│   └── FormPreview.Designer.cs
├── Interfaces/
│   ├── IRdpConnection.cs
│   └── IRdpManager.cs
├── Models/
│   ├── RdpConfig.cs
│   └── RdpState.cs
├── References/
│   ├── AxMSTSCLib.dll
│   └── MSTSCLib.dll
└── README.md

## 技術架構
### 核心類別
1. `RdpManager`
   - 管理多個 RDP 連線
   - 連線池管理
   - 資源釋放控制
   - 使用 ConcurrentDictionary 實現執行緒安全的連線管理
   - 自動產生唯一的連線 ID

2. `RdpConnection`
   - 單一連線設定
   - 連線狀態處理
   - 事件回調管理
   - 整合 AxMsRdpClient9NotSafeForScripting 控制項
   - 支援連線狀態追蹤
   - 提供畫面擷取功能

3. `RdpViewer`
   - 畫面預覽控制
   - 畫面更新處理
   - 效能優化
   - 使用 Timer 定時更新畫面
   - 實作雙緩衝繪圖提升效能
   - 自動管理畫面資源釋放
   - 美化的狀態顯示介面
   - 支援不同連線狀態的視覺回饋
   - 抗鋸齒文字渲染

### UI 元件
1. `FormPreview`
   - 遠端桌面預覽視窗
   - 支援全螢幕切換功能
   - 整合 RdpViewer 控制項
   - 提供工具列操作介面
   - ESC 鍵退出全螢幕模式

### 介面定義 
```csharp
public interface IRdpConnection
{
    bool Connect(string hostName, string userName, string password);
    void Disconnect();
    void Configure(RdpConfig config);
    bool IsConnected { get; }
    RdpState State { get; }
    Bitmap GetScreenshot();
}

public interface IRdpManager
{
    IRdpConnection CreateConnection();
    void RemoveConnection(string connectionId);
    IEnumerable<IRdpConnection> ActiveConnections { get; }
}
```

csharp
// 建立遠端連線視窗
var rdpForm = new FormRDPShow("remote-pc", "username", "password");
rdpForm.ShowDialog();
// 或是使用連線管理器
var rdpManager = new RdpManager();
var connection = rdpManager.CreateConnection();
connection.Configure(new RdpConfig
{
ScreenWidth = 1920,
ScreenHeight = 1080,
ColorDepth = 32
});
connection.Connect("remote-pc", "username", "password");

```

## 注意事項
1. 需要適當的權限才能建立 RDP 連線
2. 建議實作連線重試機制
3. 記得適當處理資源釋放
4. 注意多執行緒同步問題
5. 需要正確引用 AxMSTSCLib.dll 和 MSTSCLib.dll
6. 畫面更新頻率可能影響效能，建議根據需求調整
7. 在高併發場景下注意記憶體使用量

## 效能建議
1. 適當設定畫面更新間隔（預設 100ms）
2. 根據網路狀況調整壓縮比例
3. 使用智慧縮放減少資源消耗
4. 適時釋放未使用的連線資源

## 錯誤處理
1. 連線異常：自動重試機制
2. 畫面更新失敗：自動恢復機制
3. 資源釋放：確保正確實作 IDisposable
4. 狀態同步：即時更新 UI 顯示

## 相依性
- .NET Framework 4.6.1
- AxMSTSCLib.dll
- MSTSCLib.dll

## 後續優化方向
1. 實作連線重試機制
2. 加入連線記錄功能
3. 支援更多 RDP 進階設定
4. 效能優化與監控

接下來，我建議按照以下順序開發：
建立核心類別：
RdpManager
RdpConnection
RdpViewer
實作基本功能：
連線建立
畫面擷取
連線管理
加入進階功能：
多連線支援
效能優化
錯誤處理

## 實作細節
1. 連線管理
   - 使用 ConcurrentDictionary 確保執行緒安全
   - 自動管理連線生命週期
   - 實作 IDisposable 介面確保資源正確釋放

2. 畫面更新機制
   - 使用 100ms 間隔的計時器進行畫面更新
   - 實作最佳化的繪圖機制
   - 自動處理畫面縮放

3. 錯誤處理
   - 完整的連線狀態追蹤
   - 異常捕捉與狀態回報
   - 連線失敗自動切換狀態

## API 說明

### RdpConnection 類別
主要負責 RDP 連線的建立和管理

### RdpViewer 類別
提供遠端畫面顯示和控制功能
