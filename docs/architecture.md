# Architecture

Hotkey Inspector starts with a no-injection architecture so the app is easy to run, review, and distribute.

## Projects

`HotkeyInspector.App`

WPF desktop UI. Owns window state, tray icon behavior, keyboard capture, result display, clipboard copy, and CSV export.

`HotkeyInspector.Core`

Domain logic. Owns hotkey parsing, common hotkey generation, availability checking service, and the KnownHotkeyCatalog (large curated database of system and third-party hotkeys).

`HotkeyInspector.Infrastructure`

Native platform boundary. Owns Win32 P/Invoke (`RegisterHotKey`/`UnregisterHotKey`), `ProcessHotkeyMatcher` (runtime process matching against catalog entries), and `ProcessWindowEnumerator` for listing active GUI windows.

`HotkeyInspector.HookBridge` (optional)

Managed bridge that reads from the shared memory-mapped file written by the native HookDll. Provides `HotkeyHookService` for querying observed `RegisterHotKey` calls.

## Detection Model

### Safe mode (always active)
1. Parse text such as `Ctrl+Alt+K`.
2. Convert modifiers and key to Win32 values.
3. Call `RegisterHotKey`.
4. If registration succeeds, immediately call `UnregisterHotKey`.
5. If registration fails with error `1409`, the hotkey is occupied.
6. Look up the hotkey in `KnownHotkeyCatalog` (800+ entries across 10 categories).
7. Cross-reference catalog entry's `ProcessNames` against running GUI processes for real-time attribution.

### Hook mode (optional, requires native/HookDll)
1. Load HookDll.dll via `SetWindowsHookEx(WH_CBT)` to inject into all window-creating processes.
2. Each injected instance IAT-patches `RegisterHotKey` in user32.dll imports.
3. All `RegisterHotKey` calls are logged to a global shared memory-mapped file.
4. `HotkeyHookService` (managed) reads the log and provides real process-level attribution.

## Process Matching (Safe mode)

When `RegisterHotKey` fails with error 1409 for a hotkey not in the catalog:

1. The service enumerates all top-level windows on the desktop.
2. For each window, it retrieves: process ID, process name, window title, and window class.
3. These candidates are displayed in the results grid under "候选进程".
4. If the catalog entry for the hotkey specifies known process names, the service checks which of those are currently running and reports the match.

## KnownHotkeyCatalog

The catalog contains 800+ entries organized into categories:

| Category | Examples |
|----------|----------|
| 系统 (System) | Win+D, Win+E, Alt+Tab, Ctrl+Shift+Esc, PrintScreen |
| 浏览器 (Browsers) | Chrome, Edge, Firefox, Brave, Opera |
| 截图/录制 (Screenshot) | ShareX, Snipaste, Greenshot, OBS, NVIDIA, AMD |
| 即时通讯 (Communication) | Discord, Teams, Slack, Zoom, Telegram, WhatsApp |
| 开发者工具 (Developer) | VS Code, Visual Studio, JetBrains IDEs, Windows Terminal |
| 生产力工具 (Productivity) | PowerToys, AutoHotkey, Everything, Ditto, Listary |
| 启动器 (Launchers) | PowerToys Run, Flow Launcher, Wox |
| 媒体 (Media) | Spotify, MPC-HC/BE |
| 游戏 (Gaming) | Steam, SteelSeries GG |
| 外设驱动 (Peripherals) | Logitech G HUB, Razer Synapse, Corsair iCUE |

Each entry optionally specifies `ProcessNames`, enabling runtime process matching to attribute an occupied hotkey to a specific running application.

## Planned Extensions

- `installer/HotkeyInspector.Setup`: WiX-based MSI installer.
- Toast or overlay notifications when the user checks an occupied shortcut.
