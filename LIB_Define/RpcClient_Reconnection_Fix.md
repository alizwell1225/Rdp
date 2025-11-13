# RpcClient Reconnection Fix - Server Port Change Issue

## Problem Description

When using `StartAllClient()` to start multiple RpcClient instances:
- First connection attempt worked correctly and properly detected connection status
- When the server changed port, auto-reconnection failed to properly detect the new connection status
- Clients couldn't reconnect to the server at the new port even though config was updated

## Root Cause Analysis

The issue was in the `RpcClient.ConnectAsync()` method:

```csharp
private async Task ConnectAsync(bool retry = false)
{
    lock (_connectionLock)
    {
        if (_api == null || _config == null)
        {
            // Create new API and register event handlers
            _api = new GrpcClientApi(_config);
            // ... event handler registration
        }
        // ❌ PROBLEM: No else branch to update existing API's config
    }
    await _api.ConnectAsync(retry);
}
```

**The Problem:**
1. When `_api` was first created, it captured a reference to `_config` at that moment
2. When `LoadConfig()` or `SaveConfig()` was called to update the config (e.g., new port), it updated `RpcClient._config`
3. BUT the existing `GrpcClientApi._config` still pointed to the OLD configuration
4. On reconnection attempts, the API used the old host/port instead of the new one
5. Connection detection failed because clients were trying to connect to the wrong port

## Solution

Added an `else` branch to update the existing API's configuration when it already exists:

```csharp
private async Task ConnectAsync(bool retry = false)
{
    lock (_connectionLock)
    {
        if (_api == null || _config == null)
        {
            // Create new API instance
            if (_config == null)
                _config ??= new GrpcConfig();
            _api ??= new GrpcClientApi(_config);
            // Register event handlers...
        }
        else
        {
            // ✅ SOLUTION: Update existing API with new config
            _api.UpdateConfig(_config);
        }
    }
    await Task.Delay(10);
    await _api.ConnectAsync(retry);
}
```

## Benefits of This Fix

1. **Proper Config Propagation**: Config changes (Host, Port, LogFilePath, etc.) now properly propagate to the existing API instance
2. **Correct Reconnection**: Clients can now reconnect when server changes port
3. **Preserves Event Handlers**: No need to recreate API and re-register all event handlers
4. **Performance**: Reuses existing API instance instead of creating new ones
5. **Auto-Reconnection Works**: The auto-reconnection logic now correctly connects to the updated server address

## Testing Scenarios

### Scenario 1: Server Port Change
```csharp
// Start clients connecting to localhost:50051
private List<RpcClient> clients = new List<RpcClient>();

public async void StartAllClient()
{
    InitAllClient();  // Creates clients from config files
    foreach (var client in clients)
    {
        await client.Connect(true);  // Connect to port 50051
    }
}

// Later: Server moves to port 50052
// Update client config and reconnect
foreach (var client in clients)
{
    client.SaveConfig("localhost", "50052");  // Updates config
}
// Clients will now properly reconnect to port 50052
```

### Scenario 2: Multi-Client with Different Ports
```csharp
// 12 clients connecting to different servers
// Client 0 → localhost:50051
// Client 1 → localhost:50052
// ...
// Client 11 → localhost:50062

// If any server changes port:
clients[0].SaveConfig("localhost", "50061");  // Server moved to 50061
// Client 0 will properly reconnect to the new port
// Connection state detection works correctly
```

## Related Code

### GrpcClientApi.UpdateConfig()
The fix relies on this method in `GrpcClientApi`:

```csharp
/// <summary>
/// Updates the configuration with a new instance.
/// </summary>
/// <param name="config">The new configuration.</param>
public void UpdateConfig(GrpcConfig config)
{
    _config = config;
}
```

### Auto-Reconnection Logic
The auto-reconnection logic in `RpcClient` now works correctly:

```csharp
private async void AutoReStartWork()
{
    lock (_connectionLock)
    {
        if (autoReStart && !IsConnected)
            shouldReconnect = true;
        else
            return;
    }

    try
    {
        await Task.Delay(ReconnectDelayMs);
        lock (_connectionLock)
        {
            shouldReconnect = !IsConnected && autoReStart;
            if (shouldReconnect)
            {
                DisconnectAsync();
                ConnectAsync(shouldReconnect);  // Now uses updated config!
            }
        }
    }
    catch (Exception ex)
    {
        AppendTextSafe($"Auto-reconnect failed: {ex.Message}\r\n");
    }
}
```

## Files Modified

1. **LIB_Define/RpcClient.cs**
   - Modified `ConnectAsync()` method to update existing API config
   - Added 5 lines in else branch

## Commit

- **Hash**: 7cb8422
- **Message**: Fix RpcClient config update issue when server port changes
- **Date**: 2025-11-13

## Impact

- ✅ Fixes reconnection when server changes port
- ✅ Improves multi-client scenario reliability
- ✅ No breaking changes to existing API
- ✅ No performance degradation
- ✅ Maintains backward compatibility

## Additional Notes

This fix is particularly important for:
- Load testing scenarios with multiple clients
- Development/testing environments where server ports may change
- Production scenarios with server failover or port reconfiguration
- Any use case involving dynamic server configuration
