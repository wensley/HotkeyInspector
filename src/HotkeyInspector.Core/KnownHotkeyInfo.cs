namespace HotkeyInspector.Core;

public sealed record KnownHotkeyInfo(
    LocalizedText OwnerApplication,
    LocalizedText Usage,
    string[]? ProcessNames = null,
    LocalizedText? Category = null);
