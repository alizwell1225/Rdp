# FormRDP_Proxy 功能說明

## 概述

FormRDP_Proxy 是基於 FormRDP 的新版本，專為代理控制場景設計，提供更靈活的 RPC 連線管理和 API 控制功能。

## 主要改進

### 1. 獨立的設定頁面
- **設定選單**: 在頂部選單新增了「設定」選項
- **RPC 連線設定**: 可以在專用的設定頁面管理所有 RPC 連線
- **支援最多 24 個 RPC**: 相較於原本的 12 個，擴展至 24 個

### 2. API 控制功能

#### OpenRpcFullscreen(int rpcIndex)
```csharp
// 開啟指定的 RPC 視窗為全螢幕模式
formRdpProxy.OpenRpcFullscreen(0);  // 開啟第 1 個 RPC
formRdpProxy.OpenRpcFullscreen(5);  // 開啟第 6 個 RPC
```

**用途**: 
- 允許外部程式或其他模組指定要顯示哪一個 RPC 視窗
- 自動切換到全螢幕模式，便於遠端控制和查看

### 3. 設定頁面功能

**FormProxySettings** 提供以下功能：
- 設定 RPC 數量（1-24）
- 為每個 RPC 預先設定連線資訊：
  - 主機位址
  - 使用者名稱
  - 密碼
- 設定會自動套用到主畫面

### 4. 移除的功能

- 移除了「設定示範 IP」按鈕
- 改由設定頁面統一管理所有連線資訊

## 使用方式

### 開啟設定頁面
1. 啟動 FormRDP_Proxy
2. 點選頂部選單的「設定(S)」
3. 選擇「RPC 連線設定(C)」
4. 在設定頁面中配置所有 RPC 連線
5. 點選「確定」套用設定

### API 使用範例

```csharp
// 建立 FormRDP_Proxy 實例
var proxyForm = new FormRDP_Proxy();
proxyForm.Show();

// 透過 API 開啟特定 RPC 的全螢幕
proxyForm.OpenRpcFullscreen(2);  // 開啟第 3 個 RPC（索引從 0 開始）
```

### 外部控制場景

這個功能特別適合以下場景：
- Web 服務提供 RPC 選擇介面
- 其他應用程式需要控制顯示特定遠端桌面
- 自動化腳本需要切換不同的 RPC 視窗

## 文件結構

```
RDP_DEMO/
├── FormRDP_Proxy.cs              # 主窗體邏輯
├── FormRDP_Proxy.Designer.cs     # 主窗體設計器
├── FormRDP_Proxy.resx            # 主窗體資源
├── FormProxySettings.cs          # 設定頁面邏輯
├── FormProxySettings.Designer.cs # 設定頁面設計器
└── FormProxySettings.resx        # 設定頁面資源
```

## 資料類別

### ProxySettings
儲存所有 RPC 連線設定
```csharp
public class ProxySettings
{
    public int RpcCount { get; set; }                    // RPC 數量
    public List<ConnectionInfo> Connections { get; set; } // 連線列表
}
```

### ConnectionInfo
單一連線資訊
```csharp
public class ConnectionInfo
{
    public string HostName { get; set; }  // 主機位址
    public string UserName { get; set; }  // 使用者名稱
    public string Password { get; set; }  // 密碼
}
```

## 注意事項

1. **索引從 0 開始**: OpenRpcFullscreen 的參數 rpcIndex 是從 0 開始計算的
2. **Windows 環境**: 此應用程式只能在 Windows 環境編譯和執行
3. **連線驗證**: 在設定頁面保存設定後，仍需在主頁面手動點選「連線」按鈕

## 編譯要求

- .NET 8.0 或更高版本
- Windows 作業系統
- Visual Studio 2022 或更高版本（建議）

