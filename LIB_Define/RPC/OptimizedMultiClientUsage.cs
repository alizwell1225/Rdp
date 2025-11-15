using LIB_Define.RDP;
using LIB_RPC;
using System;
using System.Threading.Tasks;

namespace LIB_Define.RPC
{
    /// <summary>
    /// 簡單使用範例：OptimizedMultiClientManager
    /// Simple usage example for OptimizedMultiClientManager
    /// </summary>
    public class OptimizedMultiClientUsage
    {
        /// <summary>
        /// 基本使用範例 / Basic usage example
        /// </summary>
        public static async Task BasicUsageExample()
        {
            // 1. 創建優化的多客戶端管理器 / Create optimized multi-client manager
            using var manager = new OptimizedMultiClientManager(
                useSharedChannels: true,    // 啟用通道共享 / Enable channel sharing
                imageCacheMaxMB: 100        // 100MB 圖片快取 / 100MB image cache
            );

            // 2. 設定事件處理 / Setup event handlers
            manager.OnClientLog += (index, message) =>
            {
                Console.WriteLine($"[Client {index}] {message}");
            };

            manager.OnClientConnectionStateChanged += (index, connected) =>
            {
                Console.WriteLine($"Client {index}: {(connected ? "已連接" : "已斷線")} / {(connected ? "Connected" : "Disconnected")}");
            };

            manager.OnClientImageReceived += (index, type, image) =>
            {
                Console.WriteLine($"Client {index} 收到圖片 / received image: {image.Width}x{image.Height}");
            };

            // 3. 從配置文件初始化客戶端 / Initialize clients from config
            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            int created = manager.InitializeClients(config);
            Console.WriteLine($"已創建 / Created {created} 個客戶端 / clients");

            // 4. 並發連接所有客戶端 / Connect all clients concurrently
            Console.WriteLine("正在連接客戶端... / Connecting clients...");
            int connected = await manager.ConnectAllAsync(maxConcurrent: 4);
            Console.WriteLine($"已連接 / Connected {connected} 個客戶端 / clients");

            // 5. 廣播訊息到所有客戶端 / Broadcast message to all clients
            var data = new { Message = "Hello", Timestamp = DateTime.Now };
            var results = await manager.BroadcastJsonAsync("test", data);
            Console.WriteLine($"廣播成功 / Broadcast successful: {results.Count} 個客戶端 / clients");

            // 6. 獲取統計資訊 / Get statistics
            var stats = manager.GetStatistics();
            Console.WriteLine($"統計 / Statistics:");
            Console.WriteLine($"  總數 / Total: {stats.TotalClients}");
            Console.WriteLine($"  已連接 / Connected: {stats.ConnectedClients}");
            Console.WriteLine($"  快取 / Cache: {stats.CachedImages} 張圖片 / images, {stats.CacheSizeMB}");
        }

        /// <summary>
        /// 選擇性客戶端操作範例 / Selective client operations example
        /// </summary>
        public static async Task SelectiveClientsExample()
        {
            using var manager = new OptimizedMultiClientManager(
                useSharedChannels: true,
                imageCacheMaxMB: 50
            );

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.InitializeClients(config);

            // 只連接特定的客戶端 / Connect only specific clients
            Console.WriteLine("連接客戶端 0, 2, 4, 6 / Connecting clients 0, 2, 4, 6");
            await manager.ConnectClientsAsync(0, 2, 4, 6);

            // 只發送到特定客戶端 / Send to specific clients only
            var results = await manager.BroadcastJsonAsync(
                "selective",
                new { Target = "特定客戶端 / Specific clients" },
                0, 2  // 只發送到客戶端 0 和 2 / Only to clients 0 and 2
            );

            Console.WriteLine($"發送到 / Sent to {results.Count} 個客戶端 / clients");
        }

        /// <summary>
        /// 批次操作範例 / Batch operations example
        /// </summary>
        public static async Task BatchOperationsExample()
        {
            using var manager = new OptimizedMultiClientManager(
                useSharedChannels: true,
                imageCacheMaxMB: 100
            );

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.InitializeClients(config);
            await manager.ConnectAllAsync(maxConcurrent: 4);

            // 對所有客戶端執行自訂操作 / Execute custom operation on all clients
            await manager.ForEachClientAsync(async client =>
            {
                if (client.IsConnected)
                {
                    Console.WriteLine($"處理客戶端 / Processing client {client.Index}");
                    // 這裡可以執行任何操作 / You can perform any operation here
                    await Task.Delay(10);
                }
            }, maxConcurrent: 4);
        }

        /// <summary>
        /// 取得單一客戶端範例 / Get individual client example
        /// </summary>
        public static async Task IndividualClientExample()
        {
            using var manager = new OptimizedMultiClientManager(
                useSharedChannels: true,
                imageCacheMaxMB: 100
            );

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.InitializeClients(config);
            await manager.ConnectAllAsync(maxConcurrent: 4);

            // 取得特定的客戶端並直接操作 / Get specific client and operate directly
            var client5 = manager.GetClient(5);
            if (client5 != null && client5.IsConnected)
            {
                Console.WriteLine($"客戶端 5 已連接 / Client 5 is connected");
                
                // 可以直接使用 RpcClient 的所有功能 / Can use all RpcClient features directly
                await client5.SendObjectAsJsonAsync("custom", new { Action = "test" });
            }

            // 取得所有客戶端索引 / Get all client indices
            var indices = manager.GetClientIndices();
            Console.WriteLine($"管理中的客戶端 / Managed clients: {string.Join(", ", indices)}");
        }
    }
}
