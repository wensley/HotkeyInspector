# Native Hook DLL

HookDll.dll is an optional native module that provides real-time,
per-process attribution of `RegisterHotKey` calls.

> **WARNING**: This DLL must be compiled for x64 on Windows.
> It requires Administrator privileges to install a global CBT hook.
> Some antivirus software may flag DLL injection behaviour.

## Build

**Requirements**: Visual Studio 2022, Windows SDK 10.0, x64 toolchain

1. Open `native/HookDll/HookDll.vcxproj` in Visual Studio
2. Build Release x64
3. The output is `bin/x64/Release/HookDll.dll`

## Manual Build (MSBuild)

```powershell
msbuild native\HookDll\HookDll.vcxproj /p:Configuration=Release /p:Platform=x64
```

## How it Works

1. The DLL installs a `WH_CBT` global hook via `SetWindowsHookEx`.
   Windows loads the DLL into any process that creates a window.
2. On load, the DLL IAT-patches `RegisterHotKey` in user32.dll imports.
3. Every call to `RegisterHotKey` is intercepted and logged to a
   shared memory-mapped file (`Global\HotkeyInspectorSHM`).
4. The managed `HotkeyInspector.HookBridge` project reads from this
   shared memory to display live attribution data.

## Usage

The app automatically detects if HookDll is loaded.
To activate hook mode:

```csharp
// In MainWindow.xaml.cs or a service
using HotkeyInspector.HookBridge;

var hookService = new HotkeyHookService();
if (hookService.IsHookActive)
{
    var registrations = hookService.GetActiveRegistrations();
    // registrations: (processId, modifiers, virtualKey)
}
```
