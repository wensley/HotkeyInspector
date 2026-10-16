using System.Runtime.InteropServices;

namespace HotkeyInspector.App;

/// <summary>
/// 全局低级键盘钩子（WH_KEYBOARD_LL）。用于「全屏并锁定以捕获快捷键」：
/// 锁定期间拦截所有按键（含 Win、Alt+Tab 等被系统抢占的组合），把最终组合键回报给界面。
/// Ctrl+Alt+Del（安全注意序列）无法被钩子捕获。
/// </summary>
public sealed class KeyboardHook : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104;
    private const int WmSysKeyUp = 0x0105;
    private const uint LlkhfInjected = 0x10;
    private const uint LlkhfLowerIlInjected = 0x02;

    private readonly HookProc _proc;
    private IntPtr _handle = IntPtr.Zero;
    private readonly HashSet<string> _activeModifiers = [];

    public KeyboardHook()
    {
        _proc = HookCallback;
    }

    public bool IsRunning => _handle != IntPtr.Zero;

    /// <summary>捕获到完整组合键（非修饰键按下）时触发，参数为归一化后的组合文本，如 Ctrl+Alt+K。</summary>
    public event Action<string>? ComboCaptured;

    /// <summary>当前已按下的修饰键发生变化时触发（用于实时预览）。</summary>
    public event Action<string>? ModifiersChanged;

    /// <summary>在无修饰键时按下 Esc，请求退出捕获。</summary>
    public event Action? CancelRequested;

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _activeModifiers.Clear();
        _handle = SetWindowsHookEx(WhKeyboardLl, _proc, GetModuleHandle(null), 0);
        if (_handle == IntPtr.Zero)
        {
            throw new InvalidOperationException($"安装键盘钩子失败，错误码 {Marshal.GetLastWin32Error()}。");
        }
    }

    public void Stop()
    {
        if (_handle == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(_handle);
        _handle = IntPtr.Zero;
        _activeModifiers.Clear();
    }

    public void Dispose() => Stop();

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(_handle, nCode, wParam, lParam);
        }

        var message = wParam.ToInt32();
        var data = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
        var flags = data.Flags;
        var injected = (flags & LlkhfInjected) != 0 || (flags & LlkhfLowerIlInjected) != 0;
        if (injected)
        {
            return CallNextHookEx(_handle, nCode, wParam, lParam);
        }

        var isDown = message is WmKeyDown or WmSysKeyDown;
        var isUp = message is WmKeyUp or WmSysKeyUp;
        if (!isDown && !isUp)
        {
            return CallNextHookEx(_handle, nCode, wParam, lParam);
        }

        var vk = (uint)data.VirtualKeyCode;

        if (VirtualKeys.ModifierOf(vk) is { } modifier)
        {
            if (isDown)
            {
                if (_activeModifiers.Add(modifier))
                {
                    RaiseModifiers();
                }
            }
            else if (_activeModifiers.Remove(modifier))
            {
                RaiseModifiers();
            }

            return 1; // 拦截修饰键，防止 Win/Alt 触发系统动作
        }

        if (isDown)
        {
            if (VirtualKeys.Name(vk) is { } name)
            {
                if (_activeModifiers.Count == 0 && name == "Esc")
                {
                    CancelRequested?.Invoke();
                    return 1;
                }

                var combo = BuildCombo(name);
                ComboCaptured?.Invoke(combo);
            }
        }

        return 1; // 锁定期间拦截所有按键
    }

    private string BuildCombo(string key)
    {
        var parts = new List<string>();
        foreach (var mod in new[] { "Win", "Ctrl", "Alt", "Shift" })
        {
            if (_activeModifiers.Contains(mod))
            {
                parts.Add(mod);
            }
        }

        parts.Add(key);
        return string.Join('+', parts);
    }

    private void RaiseModifiers()
    {
        var parts = new List<string>();
        foreach (var mod in new[] { "Win", "Ctrl", "Alt", "Shift" })
        {
            if (_activeModifiers.Contains(mod))
            {
                parts.Add(mod);
            }
        }

        ModifiersChanged?.Invoke(parts.Count == 0 ? string.Empty : string.Join('+', parts));
    }

    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHookStruct
    {
        public uint VirtualKeyCode;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
}
