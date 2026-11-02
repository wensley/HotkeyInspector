# Hotkey Inspector

Hotkey Inspector is a Windows desktop tool for checking whether global hotkeys are already registered by Windows or another application.

The safe mode uses `RegisterHotKey` to test availability without injection. An optional native hook DLL can be loaded for real-time process-level attribution.

## Features

- .NET 10 + WPF desktop app
- Global hotkey availability checks
- Common hotkey batch scan
- Capture a shortcut from the keyboard
- Copy results to the clipboard
- Export results to CSV
- Tray icon with open and exit actions
- **368-entry KnownHotkeyCatalog** (271 unique key combinations) covering system, browser, communication, developer, and third-party hotkeys
- **Multi-owner resolution** — 59 combinations have several owners; results are ordered by category priority
- **Runtime process matching** — cross-references catalog entries against running processes

## Project Structure

```text
HotkeyInspector/
├── src/
│   ├── HotkeyInspector.App          # WPF UI
│   ├── HotkeyInspector.Core         # Domain logic + KnownHotkeyCatalog
│   └── HotkeyInspector.Infrastructure  # Win32 P/Invoke + process matching
├── native/
│   └── HookDll/                     # Archived C++ experiment, not built
├── tests/
│   └── HotkeyInspector.Tests/       # xUnit tests for parser, catalog, and rendering
├── docs/
│   └── architecture.md
├── HotkeyInspector.slnx
├── README.md
└── LICENSE
```

## Requirements

- Windows
- .NET SDK 10.0.301 or later

## Build

```powershell
dotnet build HotkeyInspector.slnx
```

## Run

```powershell
dotnet run --project src\HotkeyInspector.App\HotkeyInspector.App.csproj
```

## Publish

```powershell
dotnet publish src\HotkeyInspector.App\HotkeyInspector.App.csproj -c Release -r win-x64 --self-contained false
```

## Test

```powershell
dotnet test HotkeyInspector.slnx
```

## Detection Model

1. Try `RegisterHotKey` — success means available.
2. Error `1409` means occupied; look up the combination in `KnownHotkeyCatalog`.
3. When several applications claim the same combination, the owner from the highest-priority category is reported.
4. Check process names from **all** catalog owners of the combo; a running match takes precedence over the category order.

Windows does not expose a public API for enumerating registered hotkeys. The catalog + process matching approach covers the vast majority of real-world cases. Earlier injection-based hook experiments (`native/HookDll`) were removed from the build and are kept only as an archived reference.
