namespace HotkeyInspector.Core;

public sealed record HotkeyCheckResult(
    HotkeyDefinition Hotkey,
    bool IsAvailable,
    int ErrorCode,
    string Status,
    string Detail,
    string OwnerApplication);
