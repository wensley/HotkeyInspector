# Native Hook DLL (archived)

`HookDll.dll` is an archived experiment that attempted to attribute `RegisterHotKey`
calls to processes by injecting into window-creating processes.

**It is not part of the product:**

- not built by `HotkeyInspector.slnx`
- not loaded by the app
- the managed bridge (`HotkeyInspector.HookBridge`) has been deleted

The approach was abandoned because IAT patching only covers statically imported
calls, `GetProcAddress` bypasses it, the shared-memory log has no cross-process
synchronization, and injection triggers antivirus heuristics. It is kept only for
historical reference; do not ship or enable it.

For the supported detection model, see `docs/architecture.md`.
