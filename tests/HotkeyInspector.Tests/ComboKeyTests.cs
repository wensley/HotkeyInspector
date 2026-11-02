using HotkeyInspector.Core;

namespace HotkeyInspector.Tests;

public sealed class ComboKeyTests
{
    [Theory]
    [InlineData("Shift+Ctrl+T", "Ctrl+Shift+T")]
    [InlineData("control + c", "Ctrl+C")]
    [InlineData("Ctrl+Plus", "Ctrl+Equal")]
    [InlineData("Ctrl+Shift+]", "Ctrl+Shift+BracketRight")]
    [InlineData("Win+.", "Win+Period")]
    [InlineData("Ctrl+PgUp", "Ctrl+PageUp")]
    public void Normalize_ProducesCanonicalForm(string input, string expected)
        => Assert.Equal(expected, ComboKey.Normalize(input));

    [Fact]
    public void Normalize_DoesNotMapMacCommandToWin()
        => Assert.NotEqual(ComboKey.Normalize("Win+C"), ComboKey.Normalize("Cmd+C"));

    [Fact]
    public void Normalize_KeepsEmptyInputEmpty()
        => Assert.Equal(string.Empty, ComboKey.Normalize("  "));
}
