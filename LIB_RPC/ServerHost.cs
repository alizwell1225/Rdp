using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LIB_RPC.Abstractions;

namespace LIB_RPC
{
    public sealed class ServerHost : IAsyncDisposable
    {
        private readonly GrpcConfig _config;
        private readonly GrpcLogger _logger;
        private readonly IScreenCapture _screenCapture;
        private IHost? _host;
        
        // Event raised when a file is added to storage (upload or push)
        public event EventHandler<string>? FileAdded;
        
        // Event raised when file upload from client starts
        public event EventHandler<string>? FileUploadStarted;
        
        // Event raised when file upload from client completes
        public event EventHandler<string>? FileUploadCompleted;
        
        // Event raised when file upload from client fails
        public event EventHandler<(string Path, string Error)>? FileUploadFailed;
        
        // Event raised when server starts pushing a file
        public event EventHandler<string>? FilePushStarted;
        
        // Event raised when server completes pushing a file
        public event EventHandler<string>? FilePushCompleted;
        
        // Event raised when server fails to push a file
        public event EventHandler<(string Path, string Error)>? FilePushFailed;
        
        // Event raised when broadcast is sent
        public event EventHandler<(string Type, int ClientCount)>? BroadcastSent;
        
        // Event raised when broadcast fails
        public event EventHandler<(string Type, string Error)>? BroadcastFailed;
        
        // Event raised when a client connects
        public event EventHandler<string>? ClientConnected;
        
        // Event raised when a client disconnects
        public event EventHandler<string>? ClientDisconnected;

        public ServerHost(GrpcConfig config, GrpcLogger logger, IScreenCapture? screenCapture = null)
        {
            _config = config;
            _logger = logger;
            _screenCapture = screenCapture ?? new ScreenCapture();
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
                    services.AddSingleton(_screenCapture);
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
            // Wire up RemoteChannelService events
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
                    
                    svc.FileUploadStarted += (s, path) =>
                    {
                        try
                        {
                            FileUploadStarted?.Invoke(this, path);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"FileUploadStarted handler threw: {ex.Message}");
                        }
                    };
                    
                    svc.FileUploadCompleted += (s, path) =>
                    {
                        try
                        {
                            FileUploadCompleted?.Invoke(this, path);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"FileUploadCompleted handler threw: {ex.Message}");
                        }
                    };
                    
                    svc.FileUploadFailed += (s, args) =>
                    {
                        try
                        {
                            FileUploadFailed?.Invoke(this, args);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"FileUploadFailed handler threw: {ex.Message}");
                        }
                    };
                    
                    svc.ClientConnected += (s, clientId) =>
                    {
                        try
                        {
                            ClientConnected?.Invoke(this, clientId);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"ClientConnected handler threw: {ex.Message}");
                        }
                    };
                    
                    svc.ClientDisconnected += (s, clientId) =>
                    {
                        try
                        {
                            ClientDisconnected?.Invoke(this, clientId);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"ClientDisconnected handler threw: {ex.Message}");
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to attach event handlers: {ex.Message}");
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

        public async Task BroadcastJsonAsync(string type, string json, CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Server not started");
            var svc = _host.Services.GetService(typeof(RemoteChannelService)) as RemoteChannelService
                      ?? throw new InvalidOperationException("RemoteChannelService not resolved");
            
            try
            {
                var clientCount = await svc.BroadcastJsonAsync(type, json, ct);
                BroadcastSent?.Invoke(this, (type, clientCount));
                _logger.Info($"Broadcast sent: type={type}, clients={clientCount}");
            }
            catch (Exception ex)
            {
                BroadcastFailed?.Invoke(this, (type, ex.Message));
                _logger.Error($"Broadcast failed: type={type}, error={ex.Message}");
                throw;
            }
        }

        public async Task PushFileAsync(string filePath, CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Server not started");
            var svc = _host.Services.GetService(typeof(RemoteChannelService)) as RemoteChannelService
                      ?? throw new InvalidOperationException("RemoteChannelService not resolved");
            
            try
            {
                FilePushStarted?.Invoke(this, filePath);
                _logger.Info($"File push started: {filePath}");
                
                await svc.BroadcastFileAsync(filePath, ct);
                
                FilePushCompleted?.Invoke(this, filePath);
                _logger.Info($"File push completed: {filePath}");
            }
            catch (Exception ex)
            {
                FilePushFailed?.Invoke(this, (filePath, ex.Message));
                _logger.Error($"File push failed: {filePath} - {ex.Message}");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}
