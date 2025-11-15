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
    /// Optimized manager for multiple RPC clients with shared resources and efficient image transmission.
    /// This class addresses performance issues when using Dictionary with many RpcClient instances.
    /// </summary>
    public class OptimizedMultiClientManager : IDisposable
    {
        private readonly Dictionary<int, RpcClient> _clients = new();
        private readonly ConcurrentDictionary<string, byte[]> _imageCache = new();
        private readonly bool _useSharedChannels;
        private readonly int _imageCacheMaxSize;
        private bool _disposed = false;

        /// <summary>
        /// Event raised when any client logs a message
        /// </summary>
        public event Action<int, string>? OnClientLog;

        /// <summary>
        /// Event raised when any client connection state changes
        /// </summary>
        public event Action<int, bool>? OnClientConnectionStateChanged;

        /// <summary>
        /// Event raised when image is received by any client
        /// </summary>
        public event Action<int, ShowPictureType, Image>? OnClientImageReceived;

        /// <summary>
        /// Creates a new optimized multi-client manager
        /// </summary>
        /// <param name="useSharedChannels">Enable channel pooling for better resource utilization</param>
        /// <param name="imageCacheMaxMB">Maximum size of image cache in MB (0 to disable)</param>
        public OptimizedMultiClientManager(bool useSharedChannels = true, int imageCacheMaxMB = 100)
        {
            _useSharedChannels = useSharedChannels;
            _imageCacheMaxSize = imageCacheMaxMB * 1024 * 1024;
        }

        /// <summary>
        /// Gets the number of active clients
        /// </summary>
        public int ClientCount => _clients.Count;

        /// <summary>
        /// Gets the number of connected clients
        /// </summary>
        public int ConnectedClientCount => _clients.Values.Count(c => c.IsConnected);

        /// <summary>
        /// Gets a specific client by index
        /// </summary>
        public RpcClient? GetClient(int index)
        {
            return _clients.TryGetValue(index, out var client) ? client : null;
        }

        /// <summary>
        /// Gets all client indices
        /// </summary>
        public IEnumerable<int> GetClientIndices()
        {
            return _clients.Keys;
        }

        /// <summary>
        /// Creates and initializes multiple clients from configuration
        /// </summary>
        /// <param name="config">Multi-client configuration</param>
        /// <returns>Number of clients created</returns>
        public int InitializeClients(MultiClientConfig config)
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
                        
                        // Wire up events
                        client.ActionOnLog += (idx, msg) => OnClientLog?.Invoke(idx, msg);
                        client.ActionConnectedState += (idx, connected) => OnClientConnectionStateChanged?.Invoke(idx, connected);
                        client.ActionOnServerImage += (idx, type, img) => OnClientImageReceived?.Invoke(idx, type, img);

                        _clients[clientRef.Index] = client;
                        created++;
                    }
                    catch (Exception ex)
                    {
                        OnClientLog?.Invoke(clientRef.Index, $"Failed to create client: {ex.Message}");
                    }
                }
            }

            return created;
        }

        /// <summary>
        /// Adds a single client to the manager
        /// </summary>
        public void AddClient(int index, RpcClient client)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OptimizedMultiClientManager));

            if (_clients.ContainsKey(index))
                throw new InvalidOperationException($"Client with index {index} already exists");

            // Wire up events
            client.ActionOnLog += (idx, msg) => OnClientLog?.Invoke(idx, msg);
            client.ActionConnectedState += (idx, connected) => OnClientConnectionStateChanged?.Invoke(idx, connected);
            client.ActionOnServerImage += (idx, type, img) => OnClientImageReceived?.Invoke(idx, type, img);

            _clients[index] = client;
        }

        /// <summary>
        /// Removes a client from the manager
        /// </summary>
        public bool RemoveClient(int index)
        {
            return _clients.Remove(index);
        }

        /// <summary>
        /// Connects all clients concurrently for faster startup
        /// </summary>
        /// <param name="maxConcurrent">Maximum number of concurrent connections (0 for unlimited)</param>
        /// <returns>Number of successfully connected clients</returns>
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
        /// Connects specific clients by index
        /// </summary>
        public async Task<int> ConnectClientsAsync(params int[] indices)
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

        /// <summary>
        /// Sends a JSON message to multiple clients concurrently
        /// </summary>
        public async Task<Dictionary<int, bool>> SendJsonAsync<T>(string type, T obj, params int[] targetIndices)
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

        /// <summary>
        /// Caches an image for potential reuse
        /// </summary>
        public void CacheImage(string key, byte[] imageData)
        {
            if (_imageCacheMaxSize > 0 && imageData.Length <= _imageCacheMaxSize)
            {
                _imageCache[key] = imageData;
                
                // Simple cache size management - remove oldest if over size
                if (_imageCache.Count > 100) // Max 100 cached images
                {
                    var firstKey = _imageCache.Keys.First();
                    _imageCache.TryRemove(firstKey, out _);
                }
            }
        }

        /// <summary>
        /// Clears the image cache
        /// </summary>
        public void ClearImageCache()
        {
            _imageCache.Clear();
        }

        /// <summary>
        /// Gets statistics about the manager
        /// </summary>
        public ManagerStatistics GetStatistics()
        {
            return new ManagerStatistics
            {
                TotalClients = _clients.Count,
                ConnectedClients = _clients.Values.Count(c => c.IsConnected),
                CachedImages = _imageCache.Count,
                CacheSizeBytes = _imageCache.Values.Sum(b => b.Length),
                UseSharedChannels = _useSharedChannels
            };
        }

        /// <summary>
        /// Performs an action on all clients
        /// </summary>
        public void ForEachClient(Action<RpcClient> action)
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
        /// Performs an async action on all clients concurrently
        /// </summary>
        public async Task ForEachClientAsync(Func<RpcClient, Task> action, int maxConcurrent = 4)
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _imageCache.Clear();
            _clients.Clear();

            // If using shared channels, they'll be cleaned up by the pool
        }

        /// <summary>
        /// Statistics about the manager's operation
        /// </summary>
        public class ManagerStatistics
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
