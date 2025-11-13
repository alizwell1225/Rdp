# RDP Configuration Batch Template and JSON Format Support

## Overview

Enhanced FormProxySettings with batch configuration template and JSON format support, similar to the MultiClientConfigForm functionality.

## New Features

### 1. Batch Template Configuration (批次設定)

A new "批次設定..." button allows you to configure multiple RDP connections at once using a template.

#### Template Dialog Fields:

- **主機位址基礎 (Host Base Address)**: Base IP address (e.g., "192.168.1.")
- **起始編號 (Starting Number)**: Starting host number (1-255)
- **☑ 自動遞增主機編號 (Auto-increment)**: Automatically increment host number for each connection
- **使用者名稱 (Username)**: Common username for all connections
- **密碼 (Password)**: Common password for all connections
- **☑ 套用到所有連線 (Apply to all)**: Apply template to all connections in the grid

#### How It Works:

When you click "套用" (Apply):
1. For each connection in the grid
2. If auto-increment is enabled: Host = Base + (Start + Index)
3. If auto-increment is disabled: Host = Base + Start
4. Sets the specified username and password

#### Example:

**Template Settings:**
```
主機位址基礎: 192.168.1.
起始編號: 100
☑ 自動遞增主機編號
使用者名稱: Administrator
密碼: MyPassword123
☑ 套用到所有連線
```

**Results:**
```
Connection 0: 192.168.1.100, Administrator, MyPassword123
Connection 1: 192.168.1.101, Administrator, MyPassword123
Connection 2: 192.168.1.102, Administrator, MyPassword123
Connection 3: 192.168.1.103, Administrator, MyPassword123
...
```

### 2. JSON Format Support

A new "使用 JSON 格式儲存" checkbox allows you to choose between XML and JSON storage formats.

#### Format Comparison:

**XML Format** (Default):
- File: `Config/Rdp_Connections.xml`
- Traditional XML serialization
- Backwards compatible

**JSON Format**:
- File: `Config/Rdp_Connections.json`
- Modern JSON format
- More readable and editable
- Better for version control
- UTF-8 encoding support

#### JSON Example:

```json
[
  {
    "Index": 0,
    "Id": "abc-123-def-456",
    "Name": "Server 1",
    "HostName": "192.168.1.100",
    "UserName": "Administrator",
    "EncryptedCredentials": "...",
    "Config": {
      "ScreenWidth": 1920,
      "ScreenHeight": 1080,
      "ColorDepth": 32,
      "EnableCredSspSupport": true,
      "Domain": "",
      "EnableCompression": true,
      "EnableBitmapPersistence": true
    },
    "CreatedTime": "2024-01-01T00:00:00",
    "LastModifiedTime": "2024-01-01T00:00:00",
    "UseCount": 0,
    "IsFavorite": false,
    "GroupName": "預設群組",
    "AutoConnect": false,
    "ConnectionTimeout": 30
  }
]
```

## Code Changes

### 1. RdpConfigurationManager.cs

**Added:**
- `_useJsonFormat` flag to track format preference
- `SetUseJsonFormat(bool useJson)` method to switch formats

**Modified:**
- `SaveConnection()`: Saves in JSON or XML based on flag
- `LoadAllConnections()`: Loads from JSON or XML based on flag

**Usage:**
```csharp
var configManager = RdpConfigurationManager.Instance;

// Enable JSON format
configManager.SetUseJsonFormat(true);

// Save connection (will use JSON format)
configManager.SaveConnection(profile);

// Load connections (will use JSON format)
var profiles = configManager.LoadAllConnections();
```

### 2. FormProxySettings.cs

**Added:**
- `btnApplyTemplate_Click()` event handler
- `chkUseJsonFormat_CheckedChanged()` event handler

**Template Dialog Implementation:**
```csharp
private void btnApplyTemplate_Click(object sender, EventArgs e)
{
    using (var templateForm = new Form())
    {
        // Create template dialog with fields
        // Host base, starting number, username, password
        // Auto-increment checkbox
        // Apply to all checkbox
        
        // On OK click:
        // Loop through connections
        // Apply template values
        // Show confirmation
    }
}
```

### 3. FormProxySettings.Designer.cs

**Added UI Controls:**
- `btnApplyTemplate`: Button at (20, 503), size 120x30
- `chkUseJsonFormat`: Checkbox at (160, 510)

## Usage Guide

### Step 1: Open RDP Configuration

```csharp
var rdpManager = new RdpManager(maxConnections);
var form = new FormProxySettings(rdpManager);
form.ShowDialog();
```

### Step 2: Use Batch Template

1. Click "批次設定..." button
2. Fill in template values:
   - Host base: `192.168.1.`
   - Starting number: `100`
   - Check "自動遞增主機編號"
   - Username: `Administrator`
   - Password: `YourPassword`
   - Check "套用到所有連線"
3. Click "套用"
4. All connections updated with template values

### Step 3: Choose Format

1. Check "使用 JSON 格式儲存" for JSON format
2. Leave unchecked for XML format (default)
3. Click "確定" or "套用" to save

## Benefits

### Batch Template:
- ✅ Saves time when configuring multiple similar connections
- ✅ Reduces manual entry errors
- ✅ Automatically generates sequential IPs
- ✅ Applies common credentials to all connections

### JSON Format:
- ✅ More readable than XML
- ✅ Easier to edit manually
- ✅ Better for version control (git diff)
- ✅ Standard format for modern applications
- ✅ Proper UTF-8 character encoding

## Migration

### From XML to JSON:

1. Open FormProxySettings
2. Check "使用 JSON 格式儲存"
3. Click "套用" or "確定"
4. Connections are automatically converted to JSON format
5. New file created: `Rdp_Connections.json`
6. Old XML file remains unchanged (backup)

### From JSON to XML:

1. Open FormProxySettings
2. Uncheck "使用 JSON 格式儲存"
3. Click "套用" or "確定"
4. Connections are automatically converted to XML format

## Complete Example

```csharp
using LIB_RDP.Core;
using LIB_RDP.UI;

// Create RDP Manager
var rdpManager = new RdpManager(maxConnections: 12);

// Enable JSON format
rdpManager.ConfigurationManager.SetUseJsonFormat(true);

// Open configuration form
using (var form = new FormProxySettings(rdpManager))
{
    if (form.ShowDialog() == DialogResult.OK)
    {
        // User clicked OK, settings saved
        
        // Load all connections
        var profiles = rdpManager.ConfigurationManager.LoadAllConnections();
        
        Console.WriteLine($"Loaded {profiles.Count} connections");
        
        foreach (var profile in profiles)
        {
            Console.WriteLine($"  {profile.Name}: {profile.HostName}");
        }
    }
}
```

## Scenarios

### Scenario 1: Configure 12 Sequential Servers

**Goal:** Configure 12 RDP connections to servers 192.168.1.1 through 192.168.1.12

**Steps:**
1. Open FormProxySettings (with 12 connections)
2. Click "批次設定..."
3. Set:
   - Host base: `192.168.1.`
   - Starting: `1`
   - Check auto-increment
   - Username: `admin`
   - Password: `pass123`
   - Check apply to all
4. Click "套用"
5. Done! All 12 connections configured

### Scenario 2: Same IP, Different Usernames

**Goal:** Configure connections to same server with different users

**Steps:**
1. Click "批次設定..."
2. Set:
   - Host base: `192.168.1.100`
   - Starting: `0`
   - Uncheck auto-increment (all use same IP)
   - Leave username/password empty
3. Click "套用"
4. Manually enter different usernames in grid

### Scenario 3: Export Configuration

**Goal:** Export configuration in JSON format for sharing

**Steps:**
1. Check "使用 JSON 格式儲存"
2. Click "套用"
3. Copy `Config/Rdp_Connections.json` file
4. Share with team
5. Others can import by copying to their Config folder

## Summary

The batch template feature and JSON format support provide:
- ✅ Fast bulk configuration
- ✅ Reduced manual entry
- ✅ Modern storage format
- ✅ Easy sharing and version control
- ✅ Better compatibility with modern tools

Similar functionality to MultiClientConfigForm, adapted for RDP connection management!
