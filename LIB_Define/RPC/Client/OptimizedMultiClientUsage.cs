using LIB_Define.RDP;
using LIB_RPC;
using System;
using System.Threading.Tasks;

namespace LIB_Define.RPC.Client
{
    /// <summary>
    /// 簡單使用範例：OptimizedMultiClientManager
    /// Simple usage examples for OptimizedMultiClientManager
    /// </summary>
    public class OptimizedMultiClientUsage
    {
        /// <summary>
        /// 基本使用範例 / Basic usage example
        /// </summary>
        public static async Task BasicExample()
        {
            // 1. 創建管理器 / Create manager
            using var manager = new OptimizedMultiClientManager(
                useSharedChannels: true,    // 啟用通道共享 / Enable channel sharing
                imageCacheMaxMB: 100        // 100MB 圖片快取 / 100MB image cache
            );

            // 2. 設定事件 / Setup events (簡化的事件名稱 / Simplified event names)
            manager.OnLog += (index, message) =>
            {
                Console.WriteLine($"[Client {index}] {message}");
            };

            manager.OnConnected += (index, connected) =>
            {
                Console.WriteLine($"Client {index}: {(connected ? "Connected" : "Disconnected")}");
            };

            manager.OnServerImageReceived += (index, type, image) =>
            {
                Console.WriteLine($"Client {index} received image: {image.Width}x{image.Height}");
            };

            // 3. 初始化客戶端 / Initialize clients (簡化的方法名稱 / Simplified method name)
            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            int created = manager.Initialize(config);
            Console.WriteLine($"Created {created} clients");

            // 4. 連接所有客戶端 / Connect all clients
            Console.WriteLine("Connecting clients...");
            int connected = await manager.ConnectAllAsync(maxConcurrent: 4);
            Console.WriteLine($"Connected {connected} clients");

            // 5. 廣播訊息 / Broadcast (簡化的方法名稱 / Simplified method name)
            var data = new { Message = "Hello", Timestamp = DateTime.Now };
            var results = await manager.BroadcastAsync("test", data);
            Console.WriteLine($"Broadcast successful: {results.Count} clients");

            // 6. 取得統計 / Get stats (簡化的方法名稱 / Simplified method name)
            var stats = manager.GetStats();
            Console.WriteLine($"Statistics:");
            Console.WriteLine($"  Total: {stats.TotalClients}");
            Console.WriteLine($"  Connected: {stats.ConnectedClients}");
            Console.WriteLine($"  Cache: {stats.CachedImages} images, {stats.CacheSizeMB}");
        }

        /// <summary>
        /// 連接指定客戶端範例 / Connect specific clients example
        /// </summary>
        public static async Task ConnectSpecificExample()
        {
            using var manager = new OptimizedMultiClientManager();

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.Initialize(config);

            // 只連接指定的客戶端 / Connect only specific clients (簡化的方法名稱)
            Console.WriteLine("Connecting clients 0, 2, 4, 6...");
            await manager.ConnectAsync(0, 2, 4, 6);
        }

        /// <summary>
        /// 批次操作範例 / Batch operations example
        /// </summary>
        public static async Task BatchExample()
        {
            using var manager = new OptimizedMultiClientManager();

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.Initialize(config);
            await manager.ConnectAllAsync(maxConcurrent: 4);

            // 對所有客戶端執行操作 / Execute on all clients (簡化的方法名稱)
            await manager.ForEachAsync(async client =>
            {
                if (client.IsConnected)
                {
                    Console.WriteLine($"Processing client {client.Index}");
                    await Task.Delay(10);
                }
            }, maxConcurrent: 4);
        }

        /// <summary>
        /// 取得單一客戶端範例 / Get individual client example
        /// </summary>
        public static async Task IndividualClientExample()
        {
            using var manager = new OptimizedMultiClientManager();

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.Initialize(config);
            await manager.ConnectAllAsync(maxConcurrent: 4);

            // 取得特定客戶端 / Get specific client
            var client5 = manager.GetClient(5);
            if (client5 != null && client5.IsConnected)
            {
                Console.WriteLine($"Client 5 is connected");
                
                // 可以直接使用 RpcClient 的所有功能 / Can use all RpcClient features
                await client5.SendObjectAsJsonAsync("custom", new { Action = "test" });
            }

            // 取得所有索引 / Get all indices (簡化的方法名稱)
            var indices = manager.GetAllIndices();
            Console.WriteLine($"Managed clients: {string.Join(", ", indices)}");
        }

        /// <summary>
        /// 所有事件範例 / All events example
        /// </summary>
        public static async Task AllEventsExample()
        {
            using var manager = new OptimizedMultiClientManager();

            // 綁定所有可用的事件 / Wire up all available events
            manager.OnLog += (idx, msg) => Console.WriteLine($"[{idx}] Log: {msg}");
            manager.OnConnected += (idx, conn) => Console.WriteLine($"[{idx}] Connected: {conn}");
            manager.OnConnectionError += (idx, err) => Console.WriteLine($"[{idx}] Error: {err}");
            manager.OnServerFileReceived += (idx, path) => Console.WriteLine($"[{idx}] File: {path}");
            manager.OnServerJsonReceived += (idx, json) => Console.WriteLine($"[{idx}] JSON: {json.Type}");
            manager.OnServerImageReceived += (idx, type, img) => Console.WriteLine($"[{idx}] Image: {type}");
            manager.OnServerImagePathReceived += (idx, type, path) => Console.WriteLine($"[{idx}] ImagePath: {path}");
            manager.OnServerByteDataReceived += (idx, type, data, meta) => Console.WriteLine($"[{idx}] Data: {type}");
            manager.OnUploadProgress += (idx, path, progress) => Console.WriteLine($"[{idx}] Upload: {progress:F1}%");
            manager.OnDownloadProgress += (idx, path, progress) => Console.WriteLine($"[{idx}] Download: {progress:F1}%");
            manager.OnScreenshotReceived += (idx, img) => Console.WriteLine($"[{idx}] Screenshot: {img.Width}x{img.Height}");
            manager.OnScreenshotProgress += (idx, progress) => Console.WriteLine($"[{idx}] Screenshot: {progress:F1}%");
            manager.OnImageReceived += (idx, img) => Console.WriteLine($"[{idx}] Image: {img.Width}x{img.Height}");
            manager.OnStressTestStats += (idx, stats) => Console.WriteLine($"[{idx}] StressTest: {stats}");

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.Initialize(config);
            await manager.ConnectAllAsync();
        }
    }
}
            Console.WriteLine($"  快取 / Cache: {stats.CachedImages} 張圖片 / images, {stats.CacheSizeMB}");
        }

        /// <summary>
        /// 選擇客戶端範例
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
            var results = await manager.SendJsonAsync(
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
