using LIB_Define.RDP;
using LIB_Define.RPC.Client_org;
using LIB_RPC;
using System;
using System.Threading.Tasks;

namespace LIB_Define.RPC.Client
{
    /// <summary>
    /// 簡單使用範例：OptimizedMultiClientManager
    /// Simple usage examples for OptimizedMultiClientManager
    /// </summary>
    public static class OptimizedMultiClientUsage
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

            // 2. 設定所有事件 / Setup all events (14 events available)
            manager.OnLog += (index, message) =>
            {
                Console.WriteLine($"[Client {index}] Log: {message}");
            };

            manager.OnConnected += (index, connected) =>
            {
                Console.WriteLine($"[Client {index}] Connected: {connected}");
            };

            manager.OnConnectionError += (index, error) =>
            {
                Console.WriteLine($"[Client {index}] Error: {error}");
            };

            manager.OnServerFileReceived += (index, path) =>
            {
                Console.WriteLine($"[Client {index}] File received: {path}");
            };

            manager.OnServerJsonReceived += (index, json) =>
            {
                Console.WriteLine($"[Client {index}] JSON received: {json.Type}");
            };

            manager.OnServerImageReceived += (index, type, image) =>
            {
                Console.WriteLine($"[Client {index}] Image received: {type}, {image.Width}x{image.Height}");
            };

            manager.OnServerImagePathReceived += (index, type, path) =>
            {
                Console.WriteLine($"[Client {index}] Image path received: {type}, {path}");
            };

            manager.OnServerByteDataReceived += (index, type, data, metadata) =>
            {
                Console.WriteLine($"[Client {index}] Byte data received: {type}, {data.Length} bytes");
            };

            manager.OnUploadProgress += (index, path, progress) =>
            {
                Console.WriteLine($"[Client {index}] Upload progress: {path}, {progress:F1}%");
            };

            manager.OnDownloadProgress += (index, path, progress) =>
            {
                Console.WriteLine($"[Client {index}] Download progress: {path}, {progress:F1}%");
            };

            manager.OnScreenshotReceived += (index, image) =>
            {
                Console.WriteLine($"[Client {index}] Screenshot: {image.Width}x{image.Height}");
            };

            manager.OnScreenshotProgress += (index, progress) =>
            {
                Console.WriteLine($"[Client {index}] Screenshot progress: {progress:F1}%");
            };

            manager.OnImageReceived += (index, image) =>
            {
                Console.WriteLine($"[Client {index}] Image: {image.Width}x{image.Height}");
            };

            manager.OnStressTestStats += (index, stats) =>
            {
                Console.WriteLine($"[Client {index}] Stress test stats: {stats}");
            };

            // 3. 初始化客戶端 / Initialize clients
            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            int created = manager.Initialize(config);
            Console.WriteLine($"Created {created} clients");

            // 4. 連接所有客戶端 / Connect all clients
            Console.WriteLine("Connecting clients...");
            int connected = await manager.ConnectAllAsync(config,maxConcurrent: 4);
            Console.WriteLine($"Connected {connected} clients");

            // 5. 廣播訊息 / Broadcast message
            var data = new { Message = "Hello", Timestamp = DateTime.Now };
            var results = await manager.BroadcastAsync("test", data);
            Console.WriteLine($"Broadcast successful: {results.Count} clients");

            // 6. 取得統計 / Get statistics
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

            // 只連接指定的客戶端 / Connect only specific clients
            Console.WriteLine("Connecting clients 0, 2, 4, 6...");
            int connected = await manager.ConnectAsync(0, 2, 4, 6);
            Console.WriteLine($"Connected {connected} clients");
        }

        /// <summary>
        /// 批次操作範例 / Batch operations example
        /// </summary>
        public static async Task BatchExample()
        {
            using var manager = new OptimizedMultiClientManager();

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.Initialize(config);
            await manager.ConnectAllAsync(config, maxConcurrent: 4);

            // 對所有客戶端執行操作 / Execute on all clients
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
            await manager.ConnectAllAsync(config, maxConcurrent: 4);

            // 取得特定客戶端 / Get specific client
            var client5 = manager.GetClient(5);
            if (client5 != null && client5.IsConnected)
            {
                Console.WriteLine($"Client 5 is connected");
                
                // 可以直接使用 RpcClient 的所有功能 / Can use all RpcClient features
                await client5.SendObjectAsJsonAsync("custom", new { Action = "test" });
            }

            // 取得所有索引 / Get all indices
            var indices = manager.GetAllIndices();
            Console.WriteLine($"Managed clients: {string.Join(", ", indices)}");
        }

        /// <summary>
        /// 快取管理範例 / Cache management example
        /// </summary>
        public static void CacheExample()
        {
            using var manager = new OptimizedMultiClientManager(
                useSharedChannels: true,
                imageCacheMaxMB: 200  // 較大的快取 / Larger cache
            );

            var config = MultiClientConfig.Load("./Config/multi_client_config.json");
            manager.Initialize(config);

            // 清除快取 / Clear cache
            manager.ClearCache();
            Console.WriteLine("Cache cleared");

            // 查看統計 / Check stats
            var stats = manager.GetStats();
            Console.WriteLine($"Cache: {stats.CachedImages} images, {stats.CacheSizeMB}");
        }
    }
}
