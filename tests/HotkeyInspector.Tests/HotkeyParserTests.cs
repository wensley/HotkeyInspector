using HotkeyInspector.Core;

namespace HotkeyInspector.Tests;

public sealed class HotkeyParserTests
{
    [Theory]
    [InlineData("Ctrl+Alt+K", "Ctrl+Alt+K")]
    [InlineData("Alt+Ctrl+K", "Ctrl+Alt+K")]
    [InlineData("shift+control+p", "Ctrl+Shift+P")]
    [InlineData("Ctrl-Alt-K", "Ctrl+Alt+K")]
    [InlineData("Ctrl+PAGEUP", "Ctrl+PageUp")]
    [InlineData("Ctrl+PGUP", "Ctrl+PageUp")]
    [InlineData("Ctrl+PRTSC", "Ctrl+PrintScreen")]
    [InlineData("Ctrl+-", "Ctrl+Minus")]
    [InlineData("Alt+Shift+-", "Alt+Shift+Minus")]
    [InlineData("Ctrl+Shift+]", "Ctrl+Shift+BracketRight")]
    [InlineData("Ctrl+Plus", "Ctrl+Equal")]
    [InlineData("Ctrl++", "Ctrl+Equal")]
    [InlineData("Win+Period", "Win+Period")]
    public void Parse_CanonicalizesDisplayText(string input, string expected)
        => Assert.Equal(expected, HotkeyParser.Parse(input).DisplayText);

    [Fact]
    public void Parse_CombinesModifierFlags()
    {
        var hotkey = HotkeyParser.Parse("alt+control+k");

        Assert.Equal<HotkeyModifier>(HotkeyModifier.Control | HotkeyModifier.Alt, hotkey.Modifiers);
        Assert.Equal(0x4Bu, hotkey.VirtualKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ctrl+Alt")]
    [InlineData("K+J")]
    [InlineData("Ctrl+Nope")]
    public void Parse_RejectsInvalidInput(string? input)
        => Assert.Throws<HotkeyParseException>(() => HotkeyParser.Parse(input));
}
