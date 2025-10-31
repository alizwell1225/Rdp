# Improvements & Roadmap

## 已完成功能

- 雙向 JSON 訊息 (bidi streaming)
- 檔案上傳 (client -> server)
- 檔案下載 (server -> client)
- 螢幕截圖串流 (server -> client)
- 基本 metadata 密碼驗證
- 設定 (組態物件 + 資料夾建立)
- 檔案 / 下載 / 截圖 進度事件 (client 端)
- 抽離螢幕擷取工具 `ScreenCapture`

## 建議的後續強化

| 類別 | 項目 | 說明 | 難度 |
|------|------|------|------|
| 安全 | TLS/HTTPS | 啟用 Kestrel HTTPS 與自簽或公有憑證 | 中 |
| 安全 | JWT/HMAC | 取代簡單 Base64 密碼 | 中 |
| 傳輸 | Chunk 壓縮 | 對檔案與截圖分塊 gzip / zstd | 中 |
| 傳輸 | Hash 校驗 | 每檔案 SHA256 或分塊 MD5 + 總摘要 | 中 |
| 可靠 | 自動重連 | Channel 或串流斷線後回補 | 高 |
| 可靠 | 心跳 Ping | 定期 JSON ping/ack 偵測連線 | 低 |
| 效能 | 流控/背壓 | 使用 Channel&lt;T&gt;/Pipe 避免整檔進記憶體 | 中 |
| 體驗 | 多螢幕 | 列舉螢幕 + 指定 monitorIndex | 低 |
| 體驗 | 進度更細緻 | 事件含已傳 bytes / 總 bytes | 低 |
| 擴充 | 指令通道 | JSON type = command; 可擴充遠端控制 | 中 |
| 擴充 | JSON Schema | 對特定 type 做 schema 驗證 | 中 |
| 資料 | 日誌輪替 | Serilog rolling file / size 限制 | 低 |
| 部署 | Docker 化 | server 容器化（僅限需要截圖時使用 Windows Base） | 高 |
| AOT | Source Gen | System.Text.Json Source Generator | 低 |

## 事件建議擴充欄位

```csharp
public event Action<string, long, long, double>? OnUploadDetail; // path, sentBytes, totalBytes, percent
```

## 範例延伸：檔案上傳前計算雜湊

1. 客戶端計算 SHA256 -> 新增 JSON 先送 { type="file-intent", hash, size }
2. 伺服端檢查是否已存在相同 hash -> 若存在可回覆略過。

## 版本控管建議

- 為 gRPC service 加上 `option go_package` / `csharp_namespace` 明確命名
- 後續 breaking 變更以 `v2` 新 service 名稱 or package 避免相容性問題

## 測試建議

- 加入單元測試：拆分 FileChunk 切割邏輯、Screenshot 長度 > 0 檢查
- 整合測試：使用 InProcessServer 建立 Memory Channel 驗證 JsonStream

## 部署注意

- 若啟用 TLS，記得更改 client `GrpcChannel.ForAddress("https://...")`
- 若在防火牆後，開放 gRPC 埠 (預設 50051)

---
產生時間:
