# Log Architecture Diagram

## Component Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Code                          │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                         GrpcLogger                               │
│  (Inherits from LoggerBase)                                     │
│                                                                  │
│  Methods:                                                        │
│  - Info(message)                                                 │
│  - Warn(message)                                                 │
│  - Error(message)                                                │
│  - Debug(message)                                                │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                        LoggerBase                                │
│  (Abstract base class with file rotation)                       │
│                                                                  │
│  Features:                                                       │
│  - Async queuing (BlockingCollection)                           │
│  - Background worker thread                                      │
│  - Automatic file rotation                                       │
│  - Versioned file naming                                         │
│  - Exception safety                                              │
│  - OnLine event for custom handling                              │
└───┬─────────────────────────┬────────────────────────┬──────────┘
    │                         │                        │
    ▼                         ▼                        ▼
┌────────┐            ┌──────────────┐        ┌──────────────┐
│Console │            │  Log Files   │        │Event Handlers│
│Output  │            │              │        │   (OnLine)   │
│(opt.)  │            │ - app.log    │        └──────────────┘
└────────┘            │ - app_v0001  │
                      │ - app_v0002  │
                      │ - ...        │
                      └──────────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │   Log Viewer    │
                    │  (WinForms App) │
                    │                 │
                    │  - Load logs    │
                    │  - Filter       │
                    │  - Search       │
                    │  - Display      │
                    └─────────────────┘
```

## Data Flow

```
1. Application calls logger method
   ↓
2. LogEntry created with timestamp, level, message
   ↓
3. Entry added to BlockingCollection (thread-safe queue)
   ↓
4. Background worker dequeues entry
   ↓
5. Check if file rotation needed (entries >= MaxLogEntriesPerFile)
   ├─ Yes → Close current file, open new versioned file
   └─ No  → Continue with current file
   ↓
6. Write formatted entry to file
   ↓
7. Flush to disk (ensures data safety)
```

## File Rotation Logic

```
Initial state:
  app.log (0 entries)

After 20,000 entries:
  app.log (20,000 entries) → closed
  app_v0001.log (0 entries) → opened

After 40,000 entries:
  app.log (20,000 entries)
  app_v0001.log (20,000 entries) → closed
  app_v0002.log (0 entries) → opened

And so on...
```

## Class Hierarchy

```
                    ┌──────────────┐
                    │  IDisposable │
                    └──────┬───────┘
                           │
                           │
                    ┌──────▼───────┐
                    │  LoggerBase  │
                    │  (abstract)  │
                    └──────┬───────┘
                           │
                           │
                    ┌──────▼───────┐
                    │  GrpcLogger  │
                    │   (sealed)   │
                    └──────────────┘
```

## Thread Safety

```
Main Thread(s)                    Background Worker Thread
     │                                     │
     ├─ Info("msg") ────────┐              │
     │                      │              │
     ├─ Warn("msg") ────────┤              │
     │                      │              │
     ├─ Error("msg") ───────┤              │
     │                      │              │
     │                  ┌───▼────────┐     │
     │                  │ Blocking   │     │
     │                  │ Collection │     │
     │                  │  (Queue)   │     │
     │                  └───┬────────┘     │
     │                      │              │
     │                      └──────────────┼─ Dequeue entry
     │                                     │
     │                                     ├─ Check rotation
     │                                     │
     │                                     ├─ Write to file
     │                                     │
     │                                     ├─ Flush
     │                                     │
     │                                     ▼
     │                                 Loop back
     │
     ▼
Continue execution
(non-blocking)
```

## Log Viewer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      LogViewerForm                           │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Top Panel (Directory Selection)        │    │
│  │  [Directory: ___________________] [Browse] [Load]   │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                  Filter Panel                        │    │
│  │  [✓] Date Filter [From: __] [To: __]                │    │
│  │  Keyword: [________] Level: [All ▼]                 │    │
│  │  [Apply Filter] [Clear Filter]                       │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              DataGridView (Log Display)              │    │
│  │  Timestamp         | Level | Message      | File    │    │
│  │  2025-11-05 10:30  | INFO  | App started  | app.log │    │
│  │  2025-11-05 10:31  | WARN  | Warning msg  | app.log │    │
│  │  2025-11-05 10:32  | ERROR | Error msg    | app.log │    │
│  │  ...                                                  │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Status: Total Records: 1,234 (of 5,000)            │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘

Data Processing:
  Load All Files → Parse Each Line → Store in _allRecords
       ↓
  Apply Filters → Filter Records → Store in _filteredRecords
       ↓
  Bind to Grid → Display in DataGridView
```

## Configuration Flow

```
┌─────────────────┐
│   GrpcConfig    │
├─────────────────┤
│ LogFilePath     │──────┐
│ Max...PerFile   │      │
│ EnableConsole   │      │
│ ForceAbandon    │      │
└─────────────────┘      │
                         ▼
                  ┌──────────────┐
                  │  GrpcLogger  │
                  │ (Constructor)│
                  └──────┬───────┘
                         │
                         ▼
                  ┌──────────────┐
                  │  LoggerBase  │
                  │ (Constructor)│
                  └──────┬───────┘
                         │
                         ▼
            ┌─────────────────────────┐
            │  Initialize Components   │
            ├─────────────────────────┤
            │ - Create queue           │
            │ - Create cancellation    │
            │ - Create directory       │
            │ - Start worker thread    │
            └─────────────────────────┘
```
