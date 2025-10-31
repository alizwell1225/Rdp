using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LIB_RPC
{
    public sealed class ServerHost : IAsyncDisposable
    {
        private readonly GrpcConfig _config;
        private readonly GrpcLogger _logger;
        private IHost? _host;
        // Event raised when a file is added to storage (upload or push)
        public event EventHandler<string>? FileAdded;

        public ServerHost(GrpcConfig config, GrpcLogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_host != null) return;
            _config.EnsureFolders();
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(_config);
                    services.AddSingleton(_logger);
                    services.AddSingleton<InternalAuthInterceptor>();
                    // Register RemoteChannelService so we can resolve and call broadcast methods from UI layer
                    services.AddSingleton<RemoteChannelService>();
                    services.AddGrpc(options =>
                    {
                        options.Interceptors.Add<InternalAuthInterceptor>();
                    });
                })
                .ConfigureLogging(_ => { })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGrpcService<RemoteChannelService>();
                        });
                    });
                    webBuilder.UseKestrel(o =>
                    {
                        o.ListenAnyIP(_config.Port, listen => listen.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
                    });
                })
                .Build();
            _logger.Info($"Starting gRPC server on port {_config.Port}");
            // Wire up RemoteChannelService FileUploaded event to our FileAdded
            try
            {
                var svc = _host.Services.GetService(typeof(RemoteChannelService)) as RemoteChannelService;
                if (svc != null)
                {
                    svc.FileUploaded += (s, path) =>
                    {
                        try
                        {
                            FileAdded?.Invoke(this, path);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"FileAdded handler threw: {ex.Message}");
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to attach FileUploaded handler: {ex.Message}");
            }
            await _host.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_host == null) return;
            _logger.Info("Stopping gRPC server");
            await _host.StopAsync(cancellationToken);
            await _host.WaitForShutdownAsync(cancellationToken);
            _host = null;
        }

        public Task BroadcastJsonAsync(string type, string json, CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Server not started");
            var svc = _host.Services.GetService(typeof(RemoteChannelService)) as RemoteChannelService
                      ?? throw new InvalidOperationException("RemoteChannelService not resolved");
            return svc.BroadcastJsonAsync(type, json, ct);
        }

        public Task PushFileAsync(string filePath, CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Server not started");
            var svc = _host.Services.GetService(typeof(RemoteChannelService)) as RemoteChannelService
                      ?? throw new InvalidOperationException("RemoteChannelService not resolved");
            return svc.BroadcastFileAsync(filePath, ct);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}
