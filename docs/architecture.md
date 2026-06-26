# Architecture

Hotkey Inspector starts with a no-injection architecture so the app is easy to run, review, and distribute.

## Projects

`HotkeyInspector.App`

WPF desktop UI. Owns window state, tray icon behavior, keyboard capture, result display, clipboard copy, and CSV export.

`HotkeyInspector.Core`

Domain logic. Owns hotkey parsing, common hotkey generation, and the availability checking service.

`HotkeyInspector.Infrastructure`

Native platform boundary. Owns direct Win32 calls such as `RegisterHotKey` and `UnregisterHotKey`.

## Current Detection Model

The safe detector tries to temporarily register a hotkey:

1. Parse text such as `Ctrl+Alt+K`.
2. Convert modifiers and key to Win32 values.
3. Call `RegisterHotKey`.
4. If registration succeeds, immediately call `UnregisterHotKey`.
5. If registration fails with error `1409`, report the hotkey as blocked.

## Planned Extensions

- `HotkeyInspector.HookBridge`: optional native bridge for observing calls to `RegisterHotKey`.
- `native/HookDll`: optional C++ MinHook-based module.
- `installer/HotkeyInspector.Setup`: WiX-based MSI installer.
- Toast or overlay notifications when the user checks an occupied shortcut.
