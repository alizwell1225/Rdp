using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace LIB_RPC
{
    /// <summary>
    /// Manages a pool of gRPC channels to enable connection reuse across multiple clients.
    /// This reduces memory footprint and improves performance when multiple clients connect to the same endpoints.
    /// </summary>
    public class GrpcChannelPool : IDisposable
    {
        private static readonly Lazy<GrpcChannelPool> _instance = new(() => new GrpcChannelPool());
        
        /// <summary>
        /// Global singleton instance of the channel pool
        /// </summary>
        public static GrpcChannelPool Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, ChannelEntry> _channels = new();
        private readonly Timer _cleanupTimer;
        private readonly int _idleTimeoutSeconds;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new channel pool with specified idle timeout
        /// </summary>
        /// <param name="idleTimeoutSeconds">Time in seconds before an unused channel is disposed</param>
        public GrpcChannelPool(int idleTimeoutSeconds = 300)
        {
            _idleTimeoutSeconds = idleTimeoutSeconds;
            
            // Run cleanup every minute
            _cleanupTimer = new Timer(CleanupIdleChannels, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Gets or creates a gRPC channel for the specified endpoint
        /// </summary>
        /// <param name="host">Server host address</param>
        /// <param name="port">Server port</param>
        /// <param name="useTls">Whether to use TLS encryption</param>
        /// <returns>A reusable GrpcChannel instance</returns>
        public GrpcChannel GetOrCreateChannel(string host, int port, bool useTls = false)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GrpcChannelPool));

            var key = GetChannelKey(host, port, useTls);

            var entry = _channels.GetOrAdd(key, _ =>
            {
                var address = useTls ? $"https://{host}:{port}" : $"http://{host}:{port}";
                var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
                {
                    MaxReceiveMessageSize = 100 * 1024 * 1024, // 100MB
                    MaxSendMessageSize = 100 * 1024 * 1024,    // 100MB
                    HttpHandler = new System.Net.Http.SocketsHttpHandler
                    {
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                        EnableMultipleHttp2Connections = true
                    }
                });

                return new ChannelEntry(channel);
            });

            entry.UpdateLastAccess();
            return entry.Channel;
        }

        /// <summary>
        /// Releases a channel reference, decrementing its reference count
        /// </summary>
        /// <param name="host">Server host address</param>
        /// <param name="port">Server port</param>
        /// <param name="useTls">Whether the channel uses TLS</param>
        public void ReleaseChannel(string host, int port, bool useTls = false)
        {
            var key = GetChannelKey(host, port, useTls);
            if (_channels.TryGetValue(key, out var entry))
            {
                entry.DecrementReference();
            }
        }

        /// <summary>
        /// Gets the number of active references for a channel
        /// </summary>
        public int GetChannelReferenceCount(string host, int port, bool useTls = false)
        {
            var key = GetChannelKey(host, port, useTls);
            return _channels.TryGetValue(key, out var entry) ? entry.ReferenceCount : 0;
        }

        private string GetChannelKey(string host, int port, bool useTls)
        {
            return $"{(useTls ? "https" : "http")}://{host}:{port}";
        }

        private void CleanupIdleChannels(object? state)
        {
            if (_disposed) return;

            var now = DateTime.UtcNow;
            var keysToRemove = new System.Collections.Generic.List<string>();

            foreach (var kvp in _channels)
            {
                var entry = kvp.Value;
                
                // Only cleanup channels that are no longer referenced and have been idle
                if (entry.ReferenceCount <= 0 && 
                    (now - entry.LastAccess).TotalSeconds > _idleTimeoutSeconds)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (_channels.TryRemove(key, out var entry))
                {
                    try
                    {
                        entry.Dispose();
                    }
                    catch
                    {
                        // Ignore disposal errors
                    }
                }
            }
        }

        /// <summary>
        /// Forces disposal of all cached channels
        /// </summary>
        public async Task ShutdownAllAsync()
        {
            foreach (var entry in _channels.Values)
            {
                try
                {
                    await entry.Channel.ShutdownAsync();
                    entry.Dispose();
                }
                catch
                {
                    // Ignore shutdown errors
                }
            }
            _channels.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cleanupTimer?.Dispose();
            
            // Dispose all channels
            foreach (var entry in _channels.Values)
            {
                try
                {
                    entry.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            _channels.Clear();
        }

        /// <summary>
        /// Internal class to track channel usage
        /// </summary>
        private class ChannelEntry : IDisposable
        {
            public GrpcChannel Channel { get; }
            public DateTime LastAccess { get; private set; }
            private int _referenceCount = 1; // Start with 1 reference

            public int ReferenceCount => _referenceCount;

            public ChannelEntry(GrpcChannel channel)
            {
                Channel = channel;
                LastAccess = DateTime.UtcNow;
            }

            public void UpdateLastAccess()
            {
                LastAccess = DateTime.UtcNow;
                Interlocked.Increment(ref _referenceCount);
            }

            public void DecrementReference()
            {
                Interlocked.Decrement(ref _referenceCount);
            }

            public void Dispose()
            {
                Channel?.Dispose();
            }
        }
    }
}
