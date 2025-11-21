using System.Diagnostics;
using System.Text.Json;
using LIB_Log;
using LIB_RPC;
using LIB_RPC.Abstractions;
using LIB_RPC.API;

namespace LIB_Define.RPC;

public class RpcServer
{
    private readonly IServerApi _controller;
    private readonly Logger _logger;
    
    // Monitoring fields
    private int _totalRequestsReceived = 0;
    private int _totalBytesReceived = 0;
    private readonly Stopwatch _serverRuntime = new();
    
    // Events for external notification
    public Action<int, string>? ActionOnLog;
    public Action<int, string>? ActionOnFileAdded;
    public Action<int, string>? ActionOnFileUploadCompleted;
    public Action<int, string>? ActionOnClientConnected;
    public Action<int, string>? ActionOnClientDisconnected;
    public Action<int, string, int>? ActionOnBroadcastSent;
    public Action<int, string>? ActionOnServerStarted;
    public Action<int>? ActionOnServerStopped;
    public Action<int, string>? ActionOnServerError;
    
    public int Index { get; private set; }
    public bool IsRunning { get; private set; }
    public GrpcConfig? Config => _controller?.Config;
    
    public RpcServer(string? logPath = null, int index = 0)
    {
        Index = index;
        _controller = new GrpcServerApi();
        
        // Wire controller events
        _controller.OnLog += ControllerOnLog;
        _controller.OnFileAdded += ControllerOnFileAdded;
        _controller.OnFileUploadCompleted += ControllerOnFileUploadCompleted;
        _controller.OnClientConnected += ControllerOnClientConnected;
        _controller.OnClientDisconnected += ControllerOnClientDisconnected;
        _controller.OnBroadcastSent += ControllerOnBroadcastSent;
        _controller.OnServerStarted += ControllerOnServerStarted;
        _controller.OnServerStopped += ControllerOnServerStopped;
        _controller.OnServerStartFailed += ControllerOnServerStartFailed;
        
        if (!string.IsNullOrEmpty(logPath))
        {
            _logger = new Logger(logPath, "rpcserver");
        }
    }
    
    #region Configuration
    
    public void UpdateConfig(string host, int port)
    {
        _controller.UpdateConfig(host, port);
        AppendLog($"Config updated: {host}:{port}");
    }
    
    #endregion
    
    #region Server Control
    
    public async Task StartAsync()
    {
        try
        {
            await _controller.StartAsync();
            IsRunning = true;
            _serverRuntime.Restart();
            _totalRequestsReceived = 0;
            _totalBytesReceived = 0;
            AppendLog("Server started successfully");
        }
        catch (Exception ex)
        {
            AppendLog($"Failed to start server: {ex.Message}");
            throw;
        }
    }
    
    public async Task StopAsync()
    {
        try
        {
            await _controller.StopAsync();
            IsRunning = false;
            _serverRuntime.Stop();
            AppendLog("Server stopped");
        }
        catch (Exception ex)
        {
            AppendLog($"Error stopping server: {ex.Message}");
            throw;
        }
    }
    
    #endregion
    
    #region File Operations
    
    public string[] GetFiles()
    {
        return _controller.GetFiles();
    }
    
    public async Task<(bool Success, string Error)> PushFileAsync(string filePath, bool useAckMode = true, CancellationToken ct = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return (false, "File not found");
            }
            
            if (useAckMode)
            {
                var result = await _controller.PushFileWithAckAsync(filePath, 0, ct);
                AppendLog($"Pushed file (ACK): {Path.GetFileName(filePath)}, Clients: {result.ClientsReached}");
                return (result.Success, result.Error);
            }
            else
            {
                await _controller.PushFileAsync(filePath);
                AppendLog($"Pushed file (Stream): {Path.GetFileName(filePath)}");
                return (true, string.Empty);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Push file error: {ex.Message}");
            return (false, ex.Message);
        }
    }
    
    #endregion
    
    #region JSON Broadcasting
    
    public async Task<(bool Success, int ClientsReached, string Error)> BroadcastJsonAsync(string type, string json, bool useAckMode = true, CancellationToken ct = default)
    {
        try
        {
            if (useAckMode)
            {
                var result = await _controller.BroadcastWithAckAsync(type, json, 0, ct);
                AppendLog($"Broadcast (ACK) type={type}: Success={result.Success}, Clients={result.ClientsReached}");
                return result;
            }
            else
            {
                await _controller.BroadcastJsonAsync(type, json);
                AppendLog($"Broadcast (Stream) type={type}");
                return (true, 0, string.Empty);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Broadcast error: {ex.Message}");
            return (false, 0, ex.Message);
        }
    }
    
    /// <summary>
    /// Broadcast a generic object as JSON to all clients
    /// </summary>
    public async Task<(bool Success, int ClientsReached, string Error)> BroadcastObjectAsync<T>(string type, T obj, bool useAckMode = true, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions 
            { 
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            });
            
            return await BroadcastJsonAsync(type, json, useAckMode, ct);
        }
        catch (Exception ex)
        {
            AppendLog($"Failed to serialize/broadcast {typeof(T).Name}: {ex.Message}");
            return (false, 0, ex.Message);
        }
    }
    
    /// <summary>
    /// Broadcast FlowChartOBJ to all clients
    /// </summary>
    public async Task<(bool Success, int ClientsReached, string Error)> BroadcastFlowChartAsync(FlowChartOBJ flowChart, bool useAckMode = true, CancellationToken ct = default)
    {
        // Sync colors before sending
        flowChart.SyncColorsToArgb();
        return await BroadcastObjectAsync("flowchart", flowChart, useAckMode, ct);
    }
    
    #endregion
    
    #region Image Broadcasting
    
    /// <summary>
    /// Broadcast image by path to all clients
    /// </summary>
    public async Task<(bool Success, int ClientsReached, string Error)> BroadcastImageByPathAsync(ShowPictureType pictureType, string imagePath, bool useAckMode = true, CancellationToken ct = default)
    {
        try
        {
            if (!File.Exists(imagePath))
            {
                return (false, 0, "Image file not found");
            }
            
            var imageMsg = new ImageTransferMessage
            {
                PictureType = pictureType,
                ImagePath = imagePath,
                FileName = Path.GetFileName(imagePath)
            };
            
            var result = await BroadcastObjectAsync("image", imageMsg, useAckMode, ct);
            AppendLog($"Broadcast image by path: {imagePath} (Type: {pictureType})");
            return result;
        }
        catch (Exception ex)
        {
            AppendLog($"Broadcast image by path error: {ex.Message}");
            return (false, 0, ex.Message);
        }
    }
    
    /// <summary>
    /// Broadcast image by Image object to all clients
    /// </summary>
    public async Task<(bool Success, int ClientsReached, string Error)> BroadcastImageAsync(ShowPictureType pictureType, Image image, string fileName = "image.png", bool useAckMode = true, CancellationToken ct = default)
    {
        try
        {
            // Convert image to base64
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var imageBytes = ms.ToArray();
            var base64 = Convert.ToBase64String(imageBytes);
            
            var imageMsg = new ImageTransferMessage
            {
                PictureType = pictureType,
                ImageDataBase64 = base64,
                FileName = fileName
            };
            
            var result = await BroadcastObjectAsync("image", imageMsg, useAckMode, ct);
            AppendLog($"Broadcast image data: {imageBytes.Length} bytes (Type: {pictureType})");
            return result;
        }
        catch (Exception ex)
        {
            AppendLog($"Broadcast image error: {ex.Message}");
            return (false, 0, ex.Message);
        }
    }
    
    #endregion
    
    #region Statistics
    
    public (int TotalRequests, int TotalBytes, TimeSpan Runtime) GetStatistics()
    {
        return (_totalRequestsReceived, _totalBytesReceived, _serverRuntime.Elapsed);
    }
    
    public void ResetStatistics()
    {
        _totalRequestsReceived = 0;
        _totalBytesReceived = 0;
        _serverRuntime.Restart();
        AppendLog("Statistics reset");
    }
    
    #endregion
    
    #region Event Handlers
    
    private void ControllerOnLog(string line)
    {
        _logger?.Info(line);
        ActionOnLog?.Invoke(Index, line);
    }
    
    private void ControllerOnFileAdded(string path)
    {
        _totalRequestsReceived++;
        _logger?.Info($"File added: {Path.GetFileName(path)}");
        ActionOnFileAdded?.Invoke(Index, path);
    }
    
    private void ControllerOnFileUploadCompleted(string fileName)
    {
        _totalRequestsReceived++;
        
        if (_controller.Config != null)
        {
            var fileInfo = new FileInfo(Path.Combine(_controller.Config.StorageRoot, fileName));
            if (fileInfo.Exists)
            {
                _totalBytesReceived += (int)fileInfo.Length;
            }
        }
        
        _logger?.Info($"File upload completed: {fileName}");
        ActionOnFileUploadCompleted?.Invoke(Index, fileName);
    }
    
    private void ControllerOnClientConnected(string clientId)
    {
        _logger?.Info($"Client connected: {clientId}");
        ActionOnClientConnected?.Invoke(Index, clientId);
    }
    
    private void ControllerOnClientDisconnected(string clientId)
    {
        _logger?.Info($"Client disconnected: {clientId}");
        ActionOnClientDisconnected?.Invoke(Index, clientId);
    }
    
    private void ControllerOnBroadcastSent(string type, int clientCount)
    {
        _logger?.Info($"Broadcast sent: type={type}, clients={clientCount}");
        ActionOnBroadcastSent?.Invoke(Index, type, clientCount);
    }
    
    private void ControllerOnServerStarted()
    {
        _logger?.Info("Server started");
        ActionOnServerStarted?.Invoke(Index, "Server started successfully");
    }
    
    private void ControllerOnServerStopped()
    {
        _logger?.Info("Server stopped");
        ActionOnServerStopped?.Invoke(Index);
    }
    
    private void ControllerOnServerStartFailed(string error)
    {
        _logger?.Error($"Server start failed: {error}");
        ActionOnServerError?.Invoke(Index, error);
    }
    
    #endregion
    
    #region Helper Methods
    
    private void AppendLog(string message)
    {
        _logger?.Info(message);
        ActionOnLog?.Invoke(Index, message);
    }
    
    #endregion
}