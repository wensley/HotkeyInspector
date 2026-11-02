using HotkeyInspector.Core;

namespace HotkeyInspector.Tests;

public sealed class HotkeyCheckResultTests
{
    private static HotkeyCheckResult Available()
    {
        var hotkey = HotkeyParser.Parse("Ctrl+Alt+Shift+F24");
        return new HotkeyCheckResult(hotkey, true, 0, KnownHotkeyCatalog.Available, null, [], []);
    }

    [Fact]
    public void Available_DoesNotClaimUnavailable()
    {
        var text = Available().Render(Language.Zh);

        Assert.Equal("可用", text.Status);
        Assert.Equal("无", text.OwnerApplication);
        Assert.DoesNotContain("不可用", text.Detail);
        Assert.Contains("可注册", text.Detail);
    }

    [Fact]
    public void Available_RendersInEnglish()
    {
        var text = Available().Render(Language.En);

        Assert.Equal("Available", text.Status);
        Assert.Equal("None", text.OwnerApplication);
    }

    [Fact]
    public void Occupied1409_UsesCatalogAttribution()
    {
        var hotkey = HotkeyParser.Parse("Ctrl+Shift+Esc");
        var result = new HotkeyCheckResult(
            hotkey, false, 1409, KnownHotkeyCatalog.Lookup(hotkey), null, [], []);
        var text = result.Render(Language.En);

        Assert.Equal("In use", text.Status);
        Assert.Equal("Task Manager / other tools", text.OwnerApplication);
        Assert.Contains("Open Task Manager directly", text.Detail);
    }

    [Fact]
    public void Occupied_WithMatchedProcess_UsesProcessNameAsOwner()
    {
        var hotkey = HotkeyParser.Parse("Ctrl+Shift+Esc");
        var result = new HotkeyCheckResult(
            hotkey, false, 1409, KnownHotkeyCatalog.Lookup(hotkey), "Teams", [], []);
        var text = result.Render(Language.Zh);

        Assert.Equal("Teams", text.OwnerApplication);
        Assert.Contains("Teams", text.Detail);
    }

    [Fact]
    public void OtherErrors_AppendWin32Message()
    {
        var hotkey = HotkeyParser.Parse("Ctrl+Alt+K");
        var result = new HotkeyCheckResult(
            hotkey, false, 87, KnownHotkeyCatalog.UnknownOccupied, null, [], []);
        var text = result.Render(Language.En);

        Assert.Contains("Error:", text.Detail);
    }
}
