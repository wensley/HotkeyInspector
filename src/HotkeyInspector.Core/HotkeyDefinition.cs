namespace HotkeyInspector.Core;

public sealed record HotkeyDefinition(HotkeyModifier Modifiers, uint VirtualKey, string DisplayText);
