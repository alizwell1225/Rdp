# Multi-Client Configuration Manager - UI Preview

## Main Window Layout

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│ Multi-Client Configuration Manager                                      [_][□][X]│
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  Multi-Client Configuration Manager (in dark blue, bold)                       │
│                                                                                 │
│  ┌─ Client Count ──────────┐                                                   │
│  │                          │                                                   │
│  │  Number of Clients: [12▼]│                                                   │
│  │                          │                                                   │
│  └──────────────────────────┘                                                   │
│                                                                                 │
│  ┌─ Client Configuration ──────────────────────────────────────────────────┐   │
│  │ # │ ☑ │ Display Name │  Host       │ Port  │ Log Path      │ Storage...│   │
│  ├───┼───┼──────────────┼─────────────┼───────┼───────────────┼───────────┤   │
│  │ 0 │ ☑ │ Client 0     │ localhost   │ 50051 │ ./Logs/cli... │ ./Stor... │   │
│  │ 1 │ ☑ │ Client 1     │ localhost   │ 50052 │ ./Logs/cli... │ ./Stor... │   │
│  │ 2 │ ☑ │ Client 2     │ localhost   │ 50053 │ ./Logs/cli... │ ./Stor... │   │
│  │ 3 │ ☑ │ Client 3     │ localhost   │ 50054 │ ./Logs/cli... │ ./Stor... │   │
│  │ 4 │ ☑ │ Client 4     │ localhost   │ 50055 │ ./Logs/cli... │ ./Stor... │   │
│  │ 5 │ ☑ │ Client 5     │ localhost   │ 50056 │ ./Logs/cli... │ ./Stor... │   │
│  │ 6 │ ☑ │ Client 6     │ localhost   │ 50057 │ ./Logs/cli... │ ./Stor... │   │
│  │ 7 │ ☐ │ Client 7     │ localhost   │ 50058 │ ./Logs/cli... │ ./Stor... │   │
│  │ 8 │ ☐ │ Client 8     │ localhost   │ 50059 │ ./Logs/cli... │ ./Stor... │   │
│  │ 9 │ ☑ │ Client 9     │ localhost   │ 50060 │ ./Logs/cli... │ ./Stor... │   │
│  │ 10│ ☑ │ Client 10    │ localhost   │ 50061 │ ./Logs/cli... │ ./Stor... │   │
│  │ 11│ ☑ │ Client 11    │ localhost   │ 50062 │ ./Logs/cli... │ ./Stor... │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
│  [Load Config...] [Apply Template...] [Edit Selected...] [Enable All]          │
│                                                    [Disable All] [Save] [Cancel]│
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Grid Columns Detail

### Column 1: Index (#)
- Read-only
- Shows client number (0-11 for 12 clients)
- Auto-numbered
- Width: 40px

### Column 2: Enabled (☑)
- Checkbox column
- Check = Client enabled
- Uncheck = Client disabled
- Disabled clients shown in gray
- Width: 60px

### Column 3: Display Name
- Editable text
- Default: "Client 0", "Client 1", etc.
- Can be customized for easy identification
- Width: 120px

### Column 4: Host
- Editable text
- Server address or hostname
- Examples: "localhost", "192.168.1.100", "server.example.com"
- Width: 150px

### Column 5: Port
- Editable number
- Valid range: 1-65535
- Default: 50051
- Can be incremented automatically via template
- Width: 70px

### Column 6: Log Path
- Editable text
- Full or relative path to log file
- Example: "./Logs/client_0_grpc.log"
- Width: 200px

### Column 7: Storage Path
- Editable text
- Download/storage directory
- Example: "./Storage/client_0"
- Width: 200px

### Column 8: Config Path
- Editable text
- Path to individual JSON config file
- Example: "./Config/client_0_config.json"
- Width: 250px

## Visual States

### Enabled Client (Row)
```
│ 0 │ ☑ │ Client 0 │ localhost │ 50051 │ ./Logs/client_0... │ ./Storage/... │
```
- Normal background (white)
- Normal text color (black)

### Disabled Client (Row)
```
│ 7 │ ☐ │ Client 7 │ localhost │ 50058 │ ./Logs/client_7... │ ./Storage/... │
```
- Gray background (LightGray)
- Gray text color (DarkGray)

### Selected Row
```
│ 2 │ ☑ │ Client 2 │ localhost │ 50053 │ ./Logs/client_2... │ ./Storage/... │
```
- Highlighted in blue (selection color)
- White text

## Apply Template Dialog

```
┌──────────────────────────────────────┐
│ Apply Template Settings       [X]    │
├──────────────────────────────────────┤
│                                      │
│  Host:                               │
│  [localhost_____________________]    │
│                                      │
│  Base Port:                          │
│  [50051]                             │
│                                      │
│  ☑ Increment port for each client (+1) │
│                                      │
│  Log Path:                           │
│  [./Logs_______________________]     │
│                                      │
│  Storage Path:                       │
│  [./Storage____________________]     │
│                                      │
│                                      │
│              [Apply]    [Cancel]     │
└──────────────────────────────────────┘
```

### Template Dialog Behavior

When "Apply" is clicked:
1. **Host**: Applied to all enabled clients
2. **Port**: 
   - If increment checked: Client 0 = Base, Client 1 = Base+1, Client 2 = Base+2, etc.
   - If not checked: All clients use Base port
3. **Log Path**: Appends `/client_{index}_grpc.log` to path
4. **Storage Path**: Appends `/client_{index}` to path

### Example Template Result

Input:
- Host: `192.168.1.100`
- Base Port: `50051` (increment checked)
- Log Path: `C:\MyApp\Logs`
- Storage Path: `C:\MyApp\Storage`

Output for Client 0:
- Host: `192.168.1.100`
- Port: `50051`
- Log Path: `C:\MyApp\Logs\client_0_grpc.log`
- Storage Path: `C:\MyApp\Storage\client_0`

Output for Client 5:
- Host: `192.168.1.100`
- Port: `50056`
- Log Path: `C:\MyApp\Logs\client_5_grpc.log`
- Storage Path: `C:\MyApp\Storage\client_5`

## Button Functions

### Top Section

**Number of Clients Spinner:**
- Adjustable from 1 to 50
- Changes grid size immediately
- Adds/removes rows as needed
- Preserves existing client data

### Bottom Section (Left to Right)

1. **Load Config...**
   - Opens file dialog
   - Loads multi-client JSON config
   - Refreshes entire grid
   - File filter: `*.json`

2. **Apply Template...**
   - Opens template dialog
   - Applies settings to all enabled clients
   - Efficient bulk configuration
   - Saves time when setting up multiple similar clients

3. **Edit Selected...**
   - Opens GrpcConfigForm for selected client
   - Full individual configuration
   - All GrpcConfig properties available
   - Changes reflected in grid after save

4. **Enable All**
   - Checks all client checkboxes
   - Makes all clients active
   - Useful after bulk disable

5. **Disable All**
   - Unchecks all client checkboxes
   - Deactivates all clients
   - Useful for testing subsets

6. **Save**
   - Saves multi-client config
   - Saves individual config for each enabled client
   - Creates necessary directories
   - Shows success message
   - Closes dialog with OK result

7. **Cancel**
   - Closes dialog without saving
   - Discards all changes
   - Returns Cancel result

## Size and Layout

**Window Size:**
- Default: 1184 x 604 pixels
- Minimum: 1200 x 600 pixels
- Resizable: Yes (width and height)

**Position:**
- Start: CenterScreen
- Can be moved and resized

**Grid:**
- Auto-resizes with window
- Horizontal scrollbar if columns too wide
- Vertical scrollbar if many clients

## Keyboard Shortcuts

- **Tab**: Navigate between cells
- **Space**: Toggle checkbox (on Enabled column)
- **Enter**: Edit cell / Accept changes
- **Escape**: Cancel edit / Close dialog
- **Ctrl+S**: Save (if focus not in grid)
- **F2**: Edit selected cell

## Color Scheme

### Title
- Font: Bold, 12pt
- Color: Dark Blue (#00008B)
- Text: "Multi-Client Configuration Manager"

### Grid Headers
- Background: Light gray
- Text: Black, bold

### Enabled Clients
- Background: White
- Text: Black

### Disabled Clients
- Background: Light Gray (#D3D3D3)
- Text: Dark Gray (#A9A9A9)

### Selected Row
- Background: System selection color (usually blue)
- Text: White

## Workflow Example

### Initial Setup (12 Clients)

1. **Open Form**
   ```
   MultiClientHelper.ShowMultiClientConfigDialog();
   ```

2. **Set Client Count**
   - Adjust spinner to `12`
   - Grid shows 12 rows

3. **Apply Template**
   - Click "Apply Template"
   - Set Host: `localhost`
   - Set Base Port: `50051`
   - Check "Increment port"
   - Set Log Path: `./Logs`
   - Set Storage Path: `./Storage`
   - Click "Apply"

4. **Customize Individual Clients**
   - Select Client 0
   - Click "Edit Selected"
   - Fine-tune settings in GrpcConfigForm
   - Click "Save"
   - Repeat for clients needing special config

5. **Enable/Disable as Needed**
   - Uncheck Client 7 and 8 (not needed)
   - Check all others

6. **Save Configuration**
   - Click "Save"
   - All configs written to disk
   - Dialog closes

### Result

- 10 enabled clients (0-6, 9-11)
- 2 disabled clients (7-8)
- Individual config files created for enabled clients
- Master config saved with all settings

## Success Message

```
┌─────────────────────────────────────────┐
│ Success                          [X]    │
├─────────────────────────────────────────┤
│ ℹ Configuration saved successfully.     │
│                                         │
│   All enabled client configurations     │
│   have been updated.                    │
│                                         │
│                  [OK]                   │
└─────────────────────────────────────────┘
```

## Error Handling

### Invalid Port
```
┌─────────────────────────────────────────┐
│ Validation Error                 [X]    │
├─────────────────────────────────────────┤
│ ⚠ Port must be between 1 and 65535.    │
│                                         │
│   Client 3: Port value invalid          │
│                                         │
│                  [OK]                   │
└─────────────────────────────────────────┘
```

### File Save Error
```
┌─────────────────────────────────────────┐
│ Save Error                       [X]    │
├─────────────────────────────────────────┤
│ ⚠ Error saving configuration:           │
│                                         │
│   Access denied to path                 │
│   C:\Config\multi_client_config.json    │
│                                         │
│                  [OK]                   │
└─────────────────────────────────────────┘
```

## Integration with Existing UI

The Multi-Client Configuration Manager can be launched from:

1. **Menu Item**
   ```
   File → Settings → Multi-Client Configuration
   ```

2. **Button**
   ```
   [Configure Multiple Clients]
   ```

3. **Toolbar Icon**
   ```
   [🔧📋] (Settings + List icon)
   ```

4. **Context Menu**
   ```
   Right-click → Multi-Client Configuration
   ```

## Summary

The Multi-Client Configuration Manager provides a comprehensive, user-friendly interface for managing up to 50 RpcClient instances on a single machine. With features like:

- ✅ All-in-one grid view
- ✅ Bulk operations (template, enable/disable)
- ✅ Individual customization
- ✅ Visual feedback (colors, states)
- ✅ Separate config files
- ✅ Easy to use and understand

Perfect for the requested scenario of running 12 clients on one computer! 🎉
