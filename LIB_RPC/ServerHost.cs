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
            
            // Check if there are any connected clients before attempting to broadcast
            var totalClients = svc.GetDuplexClientCount();
            if (totalClients == 0)
            {
                var errorMsg = "No clients connected to receive broadcast";
                _logger.Warn($"Broadcast JSON failed: {errorMsg}");
                BroadcastFailed?.Invoke(this, (type, errorMsg));
                return;
            }
            
            try
            {
                _logger.Info($"Broadcasting JSON: type={type} to {totalClients} clients");
                var clientCount = await svc.BroadcastJsonAsync(type, json, ct);
                BroadcastSent?.Invoke(this, (type, clientCount));
                _logger.Info($"Broadcast sent: type={type}, succeeded={clientCount}/{totalClients}");
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
            
            // Check if there are any connected file push clients before attempting to push
            var totalClients = svc.GetFilePushClientCount();
            if (totalClients == 0)
            {
                var errorMsg = "No clients connected to receive file push";
                _logger.Warn($"File push failed: {errorMsg}");
                FilePushFailed?.Invoke(this, (filePath, errorMsg));
                return;
            }
            
            try
            {
                FilePushStarted?.Invoke(this, filePath);
                _logger.Info($"Pushing file: {filePath} to {totalClients} clients");
                
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

        /// <summary>
        /// Broadcast JSON with ACK and retry support
        /// </summary>
        public async Task<(bool Success, int ClientsReached, string Error)> BroadcastWithAckAsync(
            string type, 
            string json, 
            int retryCount = 0,
            CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Server not started");
            var svc = _host.Services.GetService(typeof(RemoteChannelService)) as RemoteChannelService
                      ?? throw new InvalidOperationException("RemoteChannelService not resolved");

            int attempt = 0;
            int maxAttempts = retryCount + 1; // Initial attempt + retries

            while (attempt < maxAttempts)
            {
                try
                {
                    var request = new RdpGrpc.Proto.BroadcastRequest
                    {
                        Type = type,
                        Json = json
                    };

                    var response = await svc.BroadcastWithAck(request, null!);

                    if (response.Success && response.ClientsReached > 0)
                    {
                        BroadcastSent?.Invoke(this, (type, response.ClientsReached));
                        _logger.Info($"[BroadcastWithAck] Success: {response.ClientsReached} clients reached (Attempt {attempt + 1}/{maxAttempts})");
                        return (true, response.ClientsReached, string.Empty);
                    }
                    else if (attempt < maxAttempts - 1)
                    {
                        // Not last attempt, retry
                        int delayMs = (int)Math.Pow(2, attempt) * 100; // Exponential backoff: 100ms, 200ms, 400ms...
                        _logger.Info($"[BroadcastWithAck] Attempt {attempt + 1}/{maxAttempts} failed: {response.Error}. Retrying in {delayMs}ms...");
                        await Task.Delay(delayMs, ct);
                    }
                    else
                    {
                        // Last attempt failed
                        BroadcastFailed?.Invoke(this, (type, response.Error));
                        _logger.Warn($"[BroadcastWithAck] Failed after {maxAttempts} attempts: {response.Error}");
                        return (false, 0, response.Error);
                    }
                }
                catch (Exception ex)
                {
                    if (attempt < maxAttempts - 1)
                    {
                        int delayMs = (int)Math.Pow(2, attempt) * 100;
                        _logger.Info($"[BroadcastWithAck] Attempt {attempt + 1}/{maxAttempts} exception: {ex.Message}. Retrying in {delayMs}ms...");
                        await Task.Delay(delayMs, ct);
                    }
                    else
                    {
                        BroadcastFailed?.Invoke(this, (type, ex.Message));
                        _logger.Error($"[BroadcastWithAck] Exception after {maxAttempts} attempts: {ex.Message}");
                        return (false, 0, ex.Message);
                    }
                }

                attempt++;
                ct.ThrowIfCancellationRequested();
            }

            return (false, 0, "Maximum retry attempts exceeded");
        }

        /// <summary>
        /// Push file with ACK and retry support
        /// </summary>
        public async Task<(bool Success, int ClientsReached, string Error)> PushFileWithAckAsync(
            string filePath, 
            int retryCount = 0,
            CancellationToken ct = default)
        {
            if (_host == null) throw new InvalidOperationException("Server not started");
            var svc = _host.Services.GetService(typeof(RemoteChannelService)) as RemoteChannelService
                      ?? throw new InvalidOperationException("RemoteChannelService not resolved");

            int attempt = 0;
            int maxAttempts = retryCount + 1; // Initial attempt + retries

            while (attempt < maxAttempts)
            {
                try
                {
                    var request = new RdpGrpc.Proto.PushFileRequest
                    {
                        FilePath = Path.GetFileName(filePath)
                    };

                    var response = await svc.PushFileWithAck(request, null!);

                    if (response.Success && response.ClientsReached > 0)
                    {
                        FilePushCompleted?.Invoke(this, filePath);
                        _logger.Info($"[PushFileWithAck] Success: {response.ClientsReached} clients reached (Attempt {attempt + 1}/{maxAttempts})");
                        return (true, response.ClientsReached, string.Empty);
                    }
                    else if (attempt < maxAttempts - 1)
                    {
                        // Not last attempt, retry
                        int delayMs = (int)Math.Pow(2, attempt) * 100; // Exponential backoff
                        _logger.Info($"[PushFileWithAck] Attempt {attempt + 1}/{maxAttempts} failed: {response.Error}. Retrying in {delayMs}ms...");
                        await Task.Delay(delayMs, ct);
                    }
                    else
                    {
                        // Last attempt failed
                        FilePushFailed?.Invoke(this, (filePath, response.Error));
                        _logger.Warn($"[PushFileWithAck] Failed after {maxAttempts} attempts: {response.Error}");
                        return (false, 0, response.Error);
                    }
                }
                catch (Exception ex)
                {
                    if (attempt < maxAttempts - 1)
                    {
                        int delayMs = (int)Math.Pow(2, attempt) * 100;
                        _logger.Info($"[PushFileWithAck] Attempt {attempt + 1}/{maxAttempts} exception: {ex.Message}. Retrying in {delayMs}ms...");
                        await Task.Delay(delayMs, ct);
                    }
                    else
                    {
                        FilePushFailed?.Invoke(this, (filePath, ex.Message));
                        _logger.Error($"[PushFileWithAck] Exception after {maxAttempts} attempts: {ex.Message}");
                        return (false, 0, ex.Message);
                    }
                }

                attempt++;
                ct.ThrowIfCancellationRequested();
            }

            return (false, 0, "Maximum retry attempts exceeded");
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}
