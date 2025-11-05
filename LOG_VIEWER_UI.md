# Log Viewer User Interface

## Main Window Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Log Viewer                                                          ╍ ☐ ✕  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ╔════════════════════════════════════════════════════════════════════════╗ │
│  ║ Log Directory                                                          ║ │
│  ║                                                                        ║ │
│  ║ Directory:                                                             ║ │
│  ║ ┌──────────────────────────────────────────────────┐                  ║ │
│  ║ │ C:\logs                                          │ [Browse...] [Load Logs] │
│  ║ └──────────────────────────────────────────────────┘                  ║ │
│  ╚════════════════════════════════════════════════════════════════════════╝ │
│                                                                              │
│  ╔════════════════════════════════════════════════════════════════════════╗ │
│  ║ Filters                                                                ║ │
│  ║                                                                        ║ │
│  ║ ☑ Enable Date Filter  From: [11/01/2025▼] To: [11/05/2025▼]         ║ │
│  ║ Keyword: [error            ] Level: [All    ▼] [Apply Filter] [Clear Filter] │
│  ╚════════════════════════════════════════════════════════════════════════╝ │
│                                                                              │
│  ┌────────────┬───────┬────────────────────────────────────┬──────────────┐ │
│  │ Timestamp  │ Level │ Message                            │ File Name    │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ INFO  │ Application started                │ app.log      │ │
│  │ 10:30:45   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ INFO  │ Connected to server                │ app.log      │ │
│  │ 10:30:46   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ WARN  │ Connection timeout, retrying...    │ app.log      │ │
│  │ 10:31:12   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ ERROR │ Failed to connect: timeout         │ app.log      │ │
│  │ 10:31:45   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ INFO  │ Retrying connection...             │ app.log      │ │
│  │ 10:32:01   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ INFO  │ Successfully connected             │ app.log      │ │
│  │ 10:32:15   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ DEBUG │ Processing request ID: 12345       │ app_v0001.log│ │
│  │ 10:33:20   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ 2025-11-05 │ INFO  │ Request completed successfully     │ app_v0001.log│ │
│  │ 10:33:22   │       │                                    │              │ │
│  ├────────────┼───────┼────────────────────────────────────┼──────────────┤ │
│  │ ...        │       │                                    │              │ │
│  └────────────┴───────┴────────────────────────────────────┴──────────────┘ │
│                                                                              │
│  Total Records: 1,234 (of 5,000)                                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

## UI Components

### 1. Directory Selection Panel (Top)
- **Label**: "Directory:"
- **TextBox**: Shows current log directory path
- **Browse Button**: Opens folder browser dialog
- **Load Logs Button**: Reads all .log files from directory

### 2. Filter Panel (Middle-Top)
- **Enable Date Filter Checkbox**: Toggle date range filtering
- **From DatePicker**: Start date for filtering (disabled when unchecked)
- **To DatePicker**: End date for filtering (disabled when unchecked)
- **Keyword TextBox**: Search text (case-insensitive)
- **Level ComboBox**: Dropdown with options: All, Debug, Info, Warn, Error
- **Apply Filter Button**: Apply current filter criteria
- **Clear Filter Button**: Reset all filters to default

### 3. Log Data Grid (Main Area)
Columns:
- **Timestamp**: Date and time of log entry (180px width)
- **Level**: Log level indicator (80px width)
- **Message**: Log message content (auto-fills remaining space)
- **File Name**: Source log file name (200px width)

Features:
- Full row selection
- Read-only
- Auto-sized columns
- Sortable by clicking column headers

### 4. Status Bar (Bottom)
- Shows: "Total Records: X (of Y)"
  - X = filtered records count
  - Y = total loaded records count

## User Workflow

### Loading Logs
```
1. Click [Browse...] → Select directory → Click "Select Folder"
2. Click [Load Logs] → Wait for loading → See success message
3. View all logs in grid
```

### Filtering Logs
```
Option 1: Date Filter
├─ Check "Enable Date Filter"
├─ Select From date
├─ Select To date
└─ Click [Apply Filter]

Option 2: Keyword Filter
├─ Enter keyword in textbox
└─ Click [Apply Filter]

Option 3: Level Filter
├─ Select level from dropdown
└─ Click [Apply Filter]

Option 4: Combined Filter
├─ Set any combination of above
└─ Click [Apply Filter]
```

### Clearing Filters
```
Click [Clear Filter] → All filters reset → View all loaded logs
```

## Color Coding (Optional Enhancement)

Could add row colors based on log level:
- **Debug**: Light gray background
- **Info**: White background (default)
- **Warn**: Light yellow background
- **Error**: Light red background

## Keyboard Shortcuts (Future Enhancement)

- **Ctrl+O**: Open/Browse directory
- **Ctrl+L**: Load logs
- **Ctrl+F**: Focus on keyword filter
- **Ctrl+R**: Clear filters
- **F5**: Reload logs

## Example Usage Scenarios

### Scenario 1: Find all errors today
```
1. Load logs from directory
2. Check "Enable Date Filter"
3. Set From and To to today's date
4. Select "Error" from Level dropdown
5. Click [Apply Filter]
→ Shows only today's error messages
```

### Scenario 2: Search for specific keyword
```
1. Load logs from directory
2. Enter "connection" in Keyword textbox
3. Click [Apply Filter]
→ Shows all logs containing "connection"
```

### Scenario 3: Review warnings from last week
```
1. Load logs from directory
2. Check "Enable Date Filter"
3. Set date range to last week
4. Select "Warn" from Level dropdown
5. Click [Apply Filter]
→ Shows all warnings from last week
```

### Scenario 4: Complex filter
```
1. Load logs from directory
2. Check "Enable Date Filter"
3. Set date range to last 3 days
4. Enter "timeout" in Keyword
5. Select "Error" from Level
6. Click [Apply Filter]
→ Shows error logs containing "timeout" from last 3 days
```

## Performance Notes

- **Loading**: Reads all files at once (may take time for large log sets)
- **Filtering**: In-memory filtering (very fast)
- **Display**: DataGridView handles large datasets efficiently
- **Recommendation**: Keep individual log files under 10MB for best performance

## Data Display Format

### Timestamp Column
- Format: `yyyy-MM-dd HH:mm:ss.fff`
- Example: `2025-11-05 10:30:45.123`
- Sortable

### Level Column
- Values: DEBUG, INFO, WARN, ERROR
- All caps for consistency
- Sortable

### Message Column
- Full log message text
- Word-wrapped if too long
- Searchable

### File Name Column
- Shows source file name only (not full path)
- Example: `app.log`, `app_v0001.log`
- Helps identify which rotated file contains the log

## Window Properties

- **Title**: "Log Viewer"
- **Size**: 1200 x 800 pixels
- **Start Position**: Center screen
- **Resizable**: Yes
- **Minimum Size**: 800 x 600 pixels (recommended)

## Error Handling

### Directory Not Found
```
Message: "Directory does not exist!"
Type: Error dialog
Action: User must select valid directory
```

### No Log Files Found
```
Message: "No .log files found in directory"
Type: Information dialog
Action: User must check directory or create logs
```

### File Read Error
```
Message: "Error loading logs: [error message]"
Type: Error dialog
Action: Check file permissions and try again
```

## Future Enhancement Ideas

1. **Export filtered results** to CSV or text file
2. **Real-time monitoring** mode (watch for new log entries)
3. **Color coding** based on log level
4. **Custom column sorting**
5. **Regex support** for keyword filtering
6. **Multiple file format support** (not just .log)
7. **Log statistics** dashboard (counts by level, timeline chart)
8. **Bookmark** favorite filters
9. **Dark mode** theme support
10. **Multi-language** interface (English/Chinese toggle)
