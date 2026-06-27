namespace HotkeyInspector.Core;

public sealed record KnownHotkeyInfo(
    string OwnerApplication,
    string Usage,
    string[]? ProcessNames = null,
    string Category = "系统");
