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
- **800+ entry KnownHotkeyCatalog** covering system, browser, communication, developer, and third-party hotkeys
- **Runtime process matching** — cross-references catalog entries against running processes
- **GUI process enumeration** — shows candidate processes when attribution is uncertain
- **Optional native HookDll** for live `RegisterHotKey` interception (see `native/`)

## Project Structure

```text
HotkeyInspector/
├── src/
│   ├── HotkeyInspector.App          # WPF UI
│   ├── HotkeyInspector.Core         # Domain logic + KnownHotkeyCatalog
│   ├── HotkeyInspector.Infrastructure  # Win32 P/Invoke + process matching
│   └── HotkeyInspector.HookBridge   # Managed bridge for native hook DLL
├── native/
│   └── HookDll/                     # C++ DLL: RegisterHotKey hook (optional)
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

## Detection Model

1. Try `RegisterHotKey` — success means available.
2. Error `1409` means occupied; look up the combination in `KnownHotkeyCatalog`.
3. If the catalog entry has known process names, check which are actually running.
4. If still uncertain, enumerate all visible GUI windows as candidates.
5. **Optional**: compile and deploy `native/HookDll` for live interception of all `RegisterHotKey` calls system-wide.

Windows does not expose a public API for enumerating registered hotkeys. The catalog + process matching approach covers the vast majority of real-world cases. The optional native hook provides complete coverage by intercepting `RegisterHotKey` at the API level.
