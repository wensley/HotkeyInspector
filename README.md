# Hotkey Inspector

Hotkey Inspector is a Windows desktop tool for checking whether global hotkeys are already registered by Windows or another application.

This repository is the safe, no-injection version first: it uses the Windows `RegisterHotKey` API to test availability without injecting code into other processes. A native hook bridge can be added later for deeper attribution.

## Features

- .NET 10 + WPF desktop app
- Global hotkey availability checks
- Common hotkey batch scan
- Capture a shortcut from the keyboard
- Copy results to the clipboard
- Export results to CSV
- Tray icon with open and exit actions
- Layered project structure ready for HookBridge and installer work

## Project Structure

```text
HotkeyInspector/
├── src/
│   ├── HotkeyInspector.App
│   ├── HotkeyInspector.Core
│   └── HotkeyInspector.Infrastructure
├── docs/
│   └── architecture.md
├── HotkeyInspector.slnx
├── README.md
└── LICENSE
```

## Requirements

- Windows
- .NET SDK 10.0.301 or later

Check your SDK:

```powershell
dotnet --version
```

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

## Detection Notes

Windows does not expose a complete public API for enumerating all globally registered hotkeys. This app checks whether a hotkey is available by temporarily trying to register it. If registration fails with Windows error `1409`, the hotkey is already registered.

This tells you that a hotkey is occupied, but it does not always identify which application owns it. Process attribution is planned for a future optional HookBridge module.
