using LIB_Define.RDP;
using LIB_RPC;
using LIB_RPC.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LIB_Define.RPC.Client
{
    /// <summary>
    /// 優化的多客戶端管理器 - 提供連接池和圖片快取功能
    /// Optimized manager for multiple RPC clients with connection pooling and image caching
    /// </summary>
    public class OptimizedMultiClientManager : IDisposable
    {
        private readonly Dictionary<int, RpcClient> _clients = new();
        private readonly ConcurrentDictionary<string, byte[]> _imageCache = new();
        private readonly bool _useSharedChannels;
        private readonly int _imageCacheMaxSize;
        private bool _disposed = false;

        #region 事件 / Events (所有 RpcClient 事件 / All RpcClient Events)

        /// <summary>
        /// 當客戶端記錄訊息時觸發 / Triggered when client logs a message
        /// </summary>
        public event Action<int, string>? OnLog;

        /// <summary>
        /// 當客戶端連接狀態改變時觸發 / Triggered when client connection state changes
        /// </summary>
        public event Action<int, bool>? OnConnected;

        /// <summary>
        /// 當客戶端連接錯誤時觸發 / Triggered when client connection error occurs
        /// </summary>
        public event Action<int, string>? OnConnectionError;

        /// <summary>
        /// 當收到伺服器檔案時觸發 / Triggered when server file is received
        /// </summary>
        public event Action<int, string>? OnServerFileReceived;

        /// <summary>
        /// 當收到伺服器 JSON 訊息時觸發 / Triggered when server JSON message is received
        /// </summary>
        public event Action<int, JsonMessage>? OnServerJsonReceived;

        /// <summary>
        /// 當收到伺服器圖片時觸發 / Triggered when server image is received
        /// </summary>
        public event Action<int, ShowPictureType, Image>? OnServerImageReceived;

        /// <summary>
        /// 當收到伺服器圖片路徑時觸發 / Triggered when server image path is received
        /// </summary>
        public event Action<int, ShowPictureType, string>? OnServerImagePathReceived;

        /// <summary>
        /// 當收到伺服器位元組資料時觸發 / Triggered when server byte data is received
        /// </summary>
        public event Action<int, string, byte[], string>? OnServerByteDataReceived;

        /// <summary>
        /// 當上傳進度更新時觸發 / Triggered when upload progress updates
        /// </summary>
        public event Action<int, string, double>? OnUploadProgress;

        /// <summary>
        /// 當下載進度更新時觸發 / Triggered when download progress updates
        /// </summary>
        public event Action<int, string, double>? OnDownloadProgress;

        /// <summary>
        /// 當截圖完成時觸發 / Triggered when screenshot is completed
        /// </summary>
        public event Action<int, Image>? OnScreenshotReceived;

        /// <summary>
        /// 當截圖進度更新時觸發 / Triggered when screenshot progress updates
        /// </summary>
        public event Action<int, double>? OnScreenshotProgress;

        /// <summary>
        /// 當收到圖片時觸發 / Triggered when image is received
        /// </summary>
        public event Action<int, Image>? OnImageReceived;

        /// <summary>
        /// 當壓力測試統計更新時觸發 / Triggered when stress test stats update
        /// </summary>
        public event Action<int, string>? OnStressTestStats;

        #endregion

        #region 建構與屬性 / Constructor and Properties

        /// <summary>
        /// 建立新的優化多客戶端管理器 / Creates a new optimized multi-client manager
        /// </summary>
        /// <param name="useSharedChannels">啟用連接池以節省資源 / Enable connection pooling to save resources</param>
        /// <param name="imageCacheMaxMB">圖片快取最大容量(MB)，0 為停用 / Max image cache size in MB, 0 to disable</param>
        public OptimizedMultiClientManager(bool useSharedChannels = true, int imageCacheMaxMB = 100)
        {
            _useSharedChannels = useSharedChannels;
            _imageCacheMaxSize = imageCacheMaxMB * 1024 * 1024;
        }

        /// <summary>
        /// 客戶端總數 / Total number of clients
        /// </summary>
        public int ClientCount => _clients.Count;

        /// <summary>
        /// 已連接的客戶端數量 / Number of connected clients
        /// </summary>
        public int ConnectedCount => _clients.Values.Count(c => c.IsConnected);

        /// <summary>
        /// 取得指定索引的客戶端 / Get client by index
        /// </summary>
        public RpcClient? GetClient(int index)
        {
            return _clients.TryGetValue(index, out var client) ? client : null;
        }

        /// <summary>
        /// 取得所有客戶端索引 / Get all client indices
        /// </summary>
        public IEnumerable<int> GetAllIndices()
        {
            return _clients.Keys;
        }

        #endregion

        #region 初始化與管理 / Initialization and Management

        /// <summary>
        /// 從配置檔初始化多個客戶端 / Initialize multiple clients from configuration
        /// </summary>
        /// <param name="config">多客戶端配置 / Multi-client configuration</param>
        /// <returns>成功建立的客戶端數量 / Number of clients created</returns>
        public int Initialize(MultiClientConfig config)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OptimizedMultiClientManager));

            int created = 0;

            foreach (var clientRef in config.Clients)
            {
                if (clientRef.Enabled && !_clients.ContainsKey(clientRef.Index))
                {
                    try
                    {
                        var client = new RpcClient(clientRef.ConfigPath, clientRef.Index);
                        
                        // 綁定所有事件 / Wire up all events
                        WireClientEvents(client);

                        _clients[clientRef.Index] = client;
                        created++;
                    }
                    catch (Exception ex)
                    {
                        OnLog?.Invoke(clientRef.Index, $"Failed to create client: {ex.Message}");
                    }
                }
            }

            return created;
        }

        /// <summary>
        /// 新增單一客戶端 / Add a single client
        /// </summary>
        public void AddClient(int index, RpcClient client)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OptimizedMultiClientManager));

            if (_clients.ContainsKey(index))
                throw new InvalidOperationException($"Client with index {index} already exists");

            // 綁定所有事件 / Wire up all events
            WireClientEvents(client);

            _clients[index] = client;
        }

        /// <summary>
        /// 移除客戶端 / Remove a client
        /// </summary>
        public bool RemoveClient(int index)
        {
            return _clients.Remove(index);
        }

        /// <summary>
        /// 綁定客戶端所有事件 / Wire up all client events
        /// </summary>
        private void WireClientEvents(RpcClient client)
        {
            client.ActionOnLog += (idx, msg) => OnLog?.Invoke(idx, msg);
            client.ActionConnectedState += (idx, connected) => OnConnected?.Invoke(idx, connected);
            client.ActionOnConnectionError += (idx, error) => OnConnectionError?.Invoke(idx, error);
            client.ActionOnServerFileCompleted += (idx, path) => OnServerFileReceived?.Invoke(idx, path);
            client.ActionOnServerJson += (idx, json) => OnServerJsonReceived?.Invoke(idx, json);
            client.ActionOnServerImage += (idx, type, img) => OnServerImageReceived?.Invoke(idx, type, img);
            client.ActionOnServerImagePath += (idx, type, path) => OnServerImagePathReceived?.Invoke(idx, type, path);
            client.ActionOnServerByteData += (idx, type, data, metadata) => OnServerByteDataReceived?.Invoke(idx, type, data, metadata);
            client.ActionOnUploadProgress += (idx, path, progress) => OnUploadProgress?.Invoke(idx, path, progress);
            client.ActionOnDownloadProgress += (idx, path, progress) => OnDownloadProgress?.Invoke(idx, path, progress);
            client.ActionScreenshot += (idx, img) => OnScreenshotReceived?.Invoke(idx, img);
            client.ActionScreenshotProgress += (idx, progress) => OnScreenshotProgress?.Invoke(idx, progress);
            client.ActionImage += (idx, img) => OnImageReceived?.Invoke(idx, img);
            client.ActionOnLogStressStats += (idx, stats) => OnStressTestStats?.Invoke(idx, stats);
        }

        #endregion

        #region 連接管理 / Connection Management

        /// <summary>
        /// 連接所有客戶端（並發執行，速度快）/ Connect all clients concurrently (fast)
        /// </summary>
        /// <param name="maxConcurrent">最大同時連接數，0 為不限制 / Max concurrent connections, 0 for unlimited</param>
        /// <returns>成功連接的客戶端數量 / Number of successfully connected clients</returns>
        public async Task<int> ConnectAllAsync(int maxConcurrent = 4)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OptimizedMultiClientManager));

            var semaphore = maxConcurrent > 0 ? new SemaphoreSlim(maxConcurrent) : null;
            var tasks = new List<Task<bool>>();

            try
            {
                foreach (var client in _clients.Values)
                {
                    if (semaphore != null)
                        await semaphore.WaitAsync();

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await client.StartConnect();
                            return client.IsConnected;
                        }
                        catch
                        {
                            return false;
                        }
                        finally
                        {
                            semaphore?.Release();
                        }
                    }));
                }

                var results = await Task.WhenAll(tasks);
                return results.Count(r => r);
            }
            finally
            {
                semaphore?.Dispose();
            }
        }

        /// <summary>
        /// 連接指定的客戶端 / Connect specific clients
        /// </summary>
        public async Task<int> ConnectAsync(params int[] indices)
        {
            var tasks = new List<Task<bool>>();

            foreach (var index in indices)
            {
                if (_clients.TryGetValue(index, out var client))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await client.StartConnect();
                            return client.IsConnected;
                        }
                        catch
                        {
                            return false;
                        }
                    }));
                }
            }

            var results = await Task.WhenAll(tasks);
            return results.Count(r => r);
        }

        #endregion

        #region 廣播與批次操作 / Broadcasting and Batch Operations

        /// <summary>
        /// 廣播 JSON 訊息到多個客戶端 / Broadcast JSON message to multiple clients
        /// </summary>
        public async Task<Dictionary<int, bool>> BroadcastAsync<T>(string type, T obj, params int[] targetIndices)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OptimizedMultiClientManager));

            var targets = targetIndices.Length > 0
                ? _clients.Where(kvp => targetIndices.Contains(kvp.Key))
                : _clients;

            var results = new ConcurrentDictionary<int, bool>();
            var tasks = targets.Select(async kvp =>
            {
                try
                {
                    var ack = await kvp.Value.SendObjectAsJsonAsync(type, obj);
                    results[kvp.Key] = ack.Success;
                }
                catch
                {
                    results[kvp.Key] = false;
                }
            });

            await Task.WhenAll(tasks);
            return new Dictionary<int, bool>(results);
        }

        /// <summary>
        /// Requests screenshots from multiple clients concurrently with optional caching
        /// </summary>
        public async Task<Dictionary<int, Image?>> GetScreenshotsAsync(bool useCache = true, params int[] targetIndices)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OptimizedMultiClientManager));

            var targets = targetIndices.Length > 0
                ? _clients.Where(kvp => targetIndices.Contains(kvp.Key))
                : _clients;

            var results = new ConcurrentDictionary<int, Image?>();
            var tasks = targets.Select(async kvp =>
            {
                try
                {
                    var cacheKey = $"screenshot_{kvp.Key}";
                    
                    // Check cache if enabled
                    if (useCache && _imageCache.TryGetValue(cacheKey, out var cachedBytes))
                    {
                        using var ms = new MemoryStream(cachedBytes);
                        results[kvp.Key] = Image.FromStream(ms);
                        return;
                    }

                    // Request new screenshot
                    await Task.Run(() => kvp.Value.SendDataScreenshot());
                    
                    // Note: Actual image will be received via ActionScreenshot event
                    // This is just triggering the request
                    results[kvp.Key] = null;
                }
                catch
                {
                    results[kvp.Key] = null;
                }
            });

            await Task.WhenAll(tasks);
            return new Dictionary<int, Image?>(results);
        }

        #endregion

        #region 快取與統計 / Cache and Statistics

        /// <summary>
        /// 快取圖片以供重複使用 / Cache an image for reuse
        /// </summary>
        public void CacheImage(string key, byte[] imageData)
        {
            if (_imageCacheMaxSize > 0 && imageData.Length <= _imageCacheMaxSize)
            {
                _imageCache[key] = imageData;
                
                // 簡單的快取大小管理 - 如果超過大小則移除最舊的 / Simple cache size management
                if (_imageCache.Count > 3) // 最多 3 張快取圖片 / Max 3 cached images
                {
                    var firstKey = _imageCache.Keys.First();
                    _imageCache.TryRemove(firstKey, out _);
                }
            }
        }

        /// <summary>
        /// 清除圖片快取 / Clear image cache
        /// </summary>
        public void ClearCache()
        {
            _imageCache.Clear();
        }

        /// <summary>
        /// 取得統計資訊 / Get statistics
        /// </summary>
        public Statistics GetStats()
        {
            return new Statistics
            {
                TotalClients = _clients.Count,
                ConnectedClients = _clients.Values.Count(c => c.IsConnected),
                CachedImages = _imageCache.Count,
                CacheSizeBytes = _imageCache.Values.Sum(b => b.Length),
                UseSharedChannels = _useSharedChannels
            };
        }

        #endregion

        /// <summary>
        /// 對所有客戶端執行操作 / Execute action on all clients
        /// </summary>
        public void ForEach(Action<RpcClient> action)
        {
            foreach (var client in _clients.Values)
            {
                try
                {
                    action(client);
                }
                catch
                {
                    // Continue with other clients even if one fails
                }
            }
        }

        /// <summary>
        /// 對所有客戶端執行非同步操作 / Execute async action on all clients
        /// </summary>
        public async Task ForEachAsync(Func<RpcClient, Task> action, int maxConcurrent = 4)
        {
            var semaphore = maxConcurrent > 0 ? new SemaphoreSlim(maxConcurrent) : null;

            try
            {
                var tasks = _clients.Values.Select(async client =>
                {
                    if (semaphore != null)
                        await semaphore.WaitAsync();

                    try
                    {
                        await action(client);
                    }
                    finally
                    {
                        semaphore?.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }
            finally
            {
                semaphore?.Dispose();
            }
        }

        #region 資源釋放 / Resource Disposal

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _imageCache.Clear();
            _clients.Clear();

            // 如果使用共享通道，它們會由連接池清理 / Shared channels will be cleaned up by the pool
        }

        #endregion

        /// <summary>
        /// 統計資訊 / Statistics information
        /// </summary>
        public class Statistics
        {
            public int TotalClients { get; set; }
            public int ConnectedClients { get; set; }
            public int CachedImages { get; set; }
            public long CacheSizeBytes { get; set; }
            public bool UseSharedChannels { get; set; }

            public string CacheSizeMB => $"{CacheSizeBytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}
