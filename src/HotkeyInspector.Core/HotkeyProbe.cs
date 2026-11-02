namespace HotkeyInspector.Core;

/// <summary>
/// 探测某个组合能否注册为全局热键。实现方负责"注册后立即注销"，
/// 让领域层不依赖任何 Win32 类型，也便于用假实现做单测。
/// </summary>
public interface IHotkeyProbe
{
    /// <summary>尝试获取并立即释放该组合；<paramref name="errorCode"/> 为 Win32 错误码（成功时为 0）。</summary>
    bool TryAcquire(HotkeyDefinition hotkey, int id, out int errorCode);
}

/// <summary>正在运行的进程视图，用于把占用热键归因到具体应用。</summary>
public interface IRunningProcessSource
{
    IReadOnlyList<ProcessCandidate> GetRunningProcesses();

    /// <summary>在目录给出的进程名中找出正在运行的项，返回逗号分隔的命中名；无命中返回 null。</summary>
    string? FindMatch(IReadOnlyList<string>? processNames);
}

public sealed record ProcessCandidate(int ProcessId, string ProcessName);

public static class HotkeyErrorCodes
{
    public const int AlreadyRegistered = 1409;
}
