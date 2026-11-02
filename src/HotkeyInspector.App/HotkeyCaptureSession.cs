namespace HotkeyInspector.App;

/// <summary>
/// 全屏锁定捕获的状态机：负责低级键盘钩子的启停与事件转发，
/// 窗口只负责窗口样式、提示文字和最终结果展示。
/// </summary>
public sealed class HotkeyCaptureSession : IDisposable
{
    private readonly KeyboardHook _hook;

    public HotkeyCaptureSession()
        : this(new KeyboardHook())
    {
    }

    public HotkeyCaptureSession(KeyboardHook hook)
    {
        _hook = hook;
        _hook.ComboCaptured += combo => ComboCaptured?.Invoke(combo);
        _hook.ModifiersChanged += modifiers => ModifiersChanged?.Invoke(modifiers);
        _hook.CancelRequested += () => CancelRequested?.Invoke();
    }

    public bool IsActive { get; private set; }

    public event Action<string>? ComboCaptured;

    public event Action<string>? ModifiersChanged;

    public event Action? CancelRequested;

    /// <summary>安装钩子；失败时返回 false 并给出可展示的错误信息。</summary>
    public bool TryStart(out string? error)
    {
        if (IsActive)
        {
            error = null;
            return true;
        }

        try
        {
            _hook.Start();
            IsActive = true;
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public void Stop()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        _hook.Stop();
    }

    public void Dispose()
    {
        Stop();
        _hook.Dispose();
    }
}
