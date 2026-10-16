namespace HotkeyInspector.Core;

public sealed class HotkeyParseException(LocalizedText message) : Exception(message.Zh)
{
    public LocalizedText Detail { get; } = message;
}
