# Architecture

Hotkey Inspector starts with a no-injection architecture so the app is easy to run, review, and distribute.

## Projects

`HotkeyInspector.App`

WPF desktop UI. Owns window state, tray icon behavior, keyboard capture, result display, clipboard copy, and CSV export.

`HotkeyInspector.Core`

Domain logic. Owns hotkey parsing, canonicalization (`ComboKey`/`KeyCatalog`), common hotkey generation, the availability service, and the KnownHotkeyCatalog. It defines the `IHotkeyProbe` and `IRunningProcessSource` ports and depends on **no** Win32/Infrastructure code, so it is unit-testable without Windows APIs.

`HotkeyInspector.Infrastructure`

Native platform boundary: implements `IHotkeyProbe` (`Win32HotkeyProbe`) and `IRunningProcessSource` (`RunningProcessSource`), plus the Win32 P/Invoke used by the probe.

## Detection Model

1. Parse text such as `Ctrl+Alt+K`.
2. Convert modifiers and key to Win32 values.
3. Call `RegisterHotKey`.
4. If registration succeeds, immediately call `UnregisterHotKey`.
5. If registration fails with error `1409`, the hotkey is occupied.
6. Look up the hotkey in `KnownHotkeyCatalog` (368 entries, 271 unique combinations, 59 combinations with multiple owners).
7. Cross-reference `ProcessNames` from all owners against running processes for real-time attribution.

The injection-based hook experiment (native HookDll + managed HookBridge) was removed: it was never wired into the app, and IAT patching cannot be made reliable or safe enough to ship.

## Process Matching (Safe mode)

When `RegisterHotKey` fails with error 1409:

1. The service looks up the hotkey in `KnownHotkeyCatalog`, which may return several owners.
2. The highest-priority owner (see category ordering below) becomes the default answer.
3. Process names from **all** owners are checked against running processes. If one matches, that owner is promoted (for example `Ctrl+Shift+Esc` reports Microsoft Teams only when `Teams.exe` is actually running).
4. Otherwise the catalog attribution is kept.

Windows does not expose an API to enumerate registered hotkeys, so candidate windows cannot be inferred; only catalog knowledge and process matching are available in safe mode.

## App structure

`MainWindow` is UI-only. Three collaborators keep it from becoming a god class:

- `HotkeyCaptureSession` — fullscreen capture state machine (hook lifetime, event forwarding).
- `HotkeyResultsModel` — single source of truth for results (language-neutral rows + projected view models, TSV/CSV export).
- `WpfKeyMapper` — WPF `Key` → Core key tokens.
- `SystemReservedHotkeys` (Core) — the system-reserved shortcut list rendered by the window.

## KnownHotkeyCatalog

The catalog contains 368 entries covering 271 unique key combinations. 59 combinations are claimed by more than one application; all owners are retained and ordered by category priority:

`系统` > `浏览器` > `截图/录制` > `即时通讯` > `开发者工具` > `生产力工具` > `启动器` > `媒体` > `游戏` > `外设驱动` > `其他`

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

Within a category, registration order is preserved (stable sort), so earlier, more specific entries win ties.

Each entry optionally specifies `ProcessNames`, enabling runtime process matching to attribute an occupied hotkey to a specific running application.

## Key vocabulary and canonicalization

`HotkeyInspector.Core/KeyCatalog.cs` is the single source of truth for key names: it maps textual tokens and Win32 virtual-key codes to canonical labels (`PageUp`, `BracketRight`, `NumpadPlus`, `Equal`, …).

- `HotkeyParser` accepts `+` or `-` separators, is case-insensitive, and always emits modifiers in `Win, Ctrl, Alt, Shift` order.
- `ComboKey.Normalize` produces the lookup form used by both the catalog and the knowledge base, so `Shift+Ctrl+T`, `ctrl+shift+t`, and `Ctrl+Shift+T` resolve identically.
- macOS spellings (`Cmd`, `Command`, `Super`) are deliberately **not** mapped to `Win`; they are treated as unknown tokens so macOS shortcuts are not misattributed to Windows ones.

## Planned Extensions

- `installer/HotkeyInspector.Setup`: WiX-based MSI installer.
- Toast or overlay notifications when the user checks an occupied shortcut.
