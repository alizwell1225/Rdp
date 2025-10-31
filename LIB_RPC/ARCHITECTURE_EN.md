# Architecture Optimization Document

## Overview

This document explains the optimization of coupling between GrpcClientApp, GrpcServerApp, and LIB_RPC.

## Key Improvements

### 1. Abstraction Layer

#### Interface Definitions
- **IClientApi**: Abstract interface for client API, hiding implementation details
- **IServerApi**: Abstract interface for server API, providing clear contracts
- **IScreenCapture**: Screen capture abstraction, allowing platform-specific implementations

#### Data Transfer Objects (DTOs)
- **JsonMessage**: Isolates UI layer from Protocol Buffer types
- **JsonAcknowledgment**: DTO for JSON acknowledgment responses
- **FileTransferResult**: DTO for file transfer results

**Benefits**:
- UI layer no longer directly depends on `RdpGrpc.Proto` namespace
- Easier unit testing (can use mocks)
- Better backward compatibility

### 2. Platform Independence

**Before**: LIB_RPC targeted `net8.0-windows` with `UseWindowsForms`

**Now**: 
- LIB_RPC targets `net8.0` (cross-platform)
- ScreenCapture uses conditional compilation for Windows-specific features
- Uses dependency injection to pass IScreenCapture implementation

**Benefits**:
- Library can be used on Linux/macOS (except screenshot functionality)
- Better cross-platform support
- Reduced dependency on specific UI frameworks

### 3. Design Patterns

#### Builder Pattern
```csharp
var config = new GrpcConfigBuilder()
    .WithHost("192.168.1.100")
    .WithPort(50051)
    .WithPassword("secure-password")
    .WithMaxChunkSize(128 * 1024)
    .Build();
```

#### Factory Pattern
```csharp
var client = GrpcApiFactory.CreateClient(config);
var server = GrpcApiFactory.CreateServer();
```

**Benefits**:
- More intuitive API
- Compile-time validation
- Centralized instance creation logic

### 4. Dependency Injection

**ServerHost** and **RemoteChannelService** now accept `IScreenCapture` parameter:

```csharp
public ServerHost(GrpcConfig config, GrpcLogger logger, IScreenCapture? screenCapture = null)
{
    _screenCapture = screenCapture ?? new ScreenCapture();
}
```

**Benefits**:
- Easier unit testing
- Swappable implementations
- Follows SOLID principles (Dependency Inversion)

### 5. Removed Duplication

**Removed**: `GrpcServerController.cs` (duplicate functionality with `GrpcServerApi.cs`)

**Benefits**:
- Reduced maintenance burden
- Prevents logic divergence
- Clearer code structure

## Architecture Diagrams

### Before Optimization
```
┌─────────────────┐         ┌─────────────────┐
│ GrpcClientApp   │         │ GrpcServerApp   │
│  (UI Layer)     │         │  (UI Layer)     │
│                 │         │                 │
│  ┌──────────┐   │         │  ┌──────────┐   │
│  │ClientForm├───┼─────┐   │  │ServerForm├───┼─────┐
│  └──────────┘   │     │   │  └──────────┘   │     │
└─────────────────┘     │   └─────────────────┘     │
                        │                           │
                        ↓                           ↓
        Direct dependency on RdpGrpc.Proto    Direct dependency on LIB_RPC
        Direct use of GrpcClientApi         Uses GrpcServerController
                        │                    or GrpcServerApi (duplication)
                        │                           │
                        └───────────┬───────────────┘
                                    ↓
                        ┌─────────────────────┐
                        │      LIB_RPC        │
                        │ (net8.0-windows)    │
                        │ Depends on WinForms │
                        └─────────────────────┘
```

### After Optimization
```
┌─────────────────┐         ┌─────────────────┐
│ GrpcClientApp   │         │ GrpcServerApp   │
│  (UI Layer)     │         │  (UI Layer)     │
│                 │         │                 │
│  ┌──────────┐   │         │  ┌──────────┐   │
│  │ClientForm│   │         │  │ServerForm│   │
│  └────┬─────┘   │         │  └────┬─────┘   │
└───────┼─────────┘         └───────┼─────────┘
        │                           │
        │ Uses Interface            │ Uses Interface
        ↓                           ↓
   IClientApi                  IServerApi
        ↑                           ↑
        │                           │
┌───────┴─────────────────────────┴─────────────┐
│            LIB_RPC.Abstractions               │
│  ┌──────────┐  ┌──────────┐  ┌────────────┐  │
│  │IClientApi│  │IServerApi│  │IScreenCapture│ │
│  └──────────┘  └──────────┘  └────────────┘  │
│                                               │
│  ┌─────────────────────────────────────┐     │
│  │  DTOs (JsonMessage, etc.)           │     │
│  └─────────────────────────────────────┘     │
└───────────────────────────────────────────────┘
                    ↑
                    │ Implements
┌───────────────────┴───────────────────────────┐
│            LIB_RPC (net8.0)                   │
│                                               │
│  ┌──────────────┐      ┌──────────────┐      │
│  │GrpcClientApi │      │GrpcServerApi │      │
│  │implements    │      │implements    │      │
│  │IClientApi    │      │IServerApi    │      │
│  └──────────────┘      └──────────────┘      │
│                                               │
│  ┌──────────────┐      ┌──────────────┐      │
│  │ScreenCapture │      │  ServerHost  │      │
│  │implements    │      │  (DI ready)  │      │
│  │IScreenCapture│      └──────────────┘      │
│  └──────────────┘                            │
│                                               │
│  ┌─────────────────────────────────────┐     │
│  │  GrpcConfigBuilder, GrpcApiFactory  │     │
│  └─────────────────────────────────────┘     │
└───────────────────────────────────────────────┘
```

## Coupling Improvements

### Before (Coupling Issues)
1. **Tight Coupling**: UI directly uses Protocol Buffer types
2. **Platform Coupling**: Library bound to Windows Forms
3. **Code Duplication**: GrpcServerController and GrpcServerApi
4. **Lack of Abstraction**: Difficult to replace implementations or test
5. **Hard-coded Dependencies**: Direct instantiation of concrete classes

### After (Benefits)
1. **Loose Coupling**: Isolation through interfaces and DTOs
2. **Cross-platform**: Core library independent of UI frameworks
3. **No Duplication**: Unified use of GrpcServerApi
4. **Testability**: All dependencies can be injected and mocked
5. **Flexibility**: Factory and builder patterns

## Testing Improvements

### Before
```csharp
// Hard to test - direct dependency on concrete classes
var form = new ClientForm();
// Cannot inject mock dependencies
```

### Now
```csharp
// Easy to test - uses interfaces
IClientApi mockClient = new MockClientApi();
var form = new ClientForm(mockClient); // Assuming it accepts IClientApi

// Or use factory for integration tests
var client = GrpcApiFactory.CreateClient(testConfig);
```

## Usage Examples

### Client Usage with New API
```csharp
// Create config using builder
var config = new GrpcConfigBuilder()
    .WithHost("server.example.com")
    .WithPort(50051)
    .WithPassword("my-secret")
    .Build();

// Create client using factory
IClientApi client = GrpcApiFactory.CreateClient(config);

// Subscribe to events (now using DTOs)
client.OnServerJson += msg => Console.WriteLine($"Type: {msg.Type}, JSON: {msg.Json}");

// Connect and use
await client.ConnectAsync();
var result = await client.SendJsonAsync("ping", "{}");
Console.WriteLine($"Success: {result.Success}");
```

### Server Usage with New API
```csharp
// Create server using factory
IServerApi server = GrpcApiFactory.CreateServer();

// Subscribe to events
server.OnLog += msg => Console.WriteLine(msg);
server.OnFileAdded += path => Console.WriteLine($"New file: {path}");

// Configure and start
server.UpdateConfig("0.0.0.0", 50051);
await server.StartAsync();

// Broadcast message
await server.BroadcastJsonAsync("notification", "{\"message\": \"Hello\"}");
```

## Backward Compatibility

Existing code can still use `GrpcClientApi` and `GrpcServerApi`, but migration to interfaces is recommended:

```csharp
// Old code (still works)
var client = new GrpcClientApi(config);

// New code (recommended)
IClientApi client = GrpcApiFactory.CreateClient(config);
```

## Future Enhancement Suggestions

1. **Service Locator Pattern**: Consider adding a service container for more complex DI scenarios
2. **Config Validation**: Add more validation logic to GrpcConfigBuilder
3. **Logger Abstraction**: Abstract GrpcLogger to ILogger interface
4. **Retry Strategy**: Add retry and reconnection mechanisms in IClientApi
5. **Health Checks**: Add server health check endpoints

## Performance Impact

These improvements have minimal performance impact:
- DTO conversion overhead is minimal (simple property copying)
- Interface calls perform identically to direct calls (after JIT optimization)
- Builder pattern is only used once during initialization

## Summary

This optimization significantly reduces coupling between layers:

✅ **Interface Segregation**: UI layer only depends on abstract interfaces  
✅ **Platform Independence**: Core library is cross-platform  
✅ **Testability**: All dependencies can be injected  
✅ **Maintainability**: Removed duplicate code  
✅ **Extensibility**: Design patterns enable easy extension  
✅ **Documentation**: Clear XML comments and documentation  

These improvements follow SOLID principles and provide better code quality and maintainability.

## Migration Guide

### For Client Applications

**Before:**
```csharp
using RdpGrpc;
var api = new GrpcClientApi(config);
api.OnServerJson += env => HandleProtoMessage(env);
```

**After:**
```csharp
using LIB_RPC.Abstractions;
IClientApi api = GrpcApiFactory.CreateClient(config);
api.OnServerJson += msg => HandleDtoMessage(msg);
```

### For Server Applications

**Before:**
```csharp
var controller = new GrpcServerController();
controller.UpdateConfig(host, port);
```

**After:**
```csharp
IServerApi controller = GrpcApiFactory.CreateServer();
controller.UpdateConfig(host, port);
```

## Conclusion

These architectural improvements establish a solid foundation for:
- **Scalability**: Easy to add new features without breaking existing code
- **Maintainability**: Clear separation of concerns
- **Quality**: Better testability leads to higher code quality
- **Flexibility**: Easy to swap implementations or add new platforms

The decoupling achieved through these changes makes the codebase more professional and enterprise-ready.
