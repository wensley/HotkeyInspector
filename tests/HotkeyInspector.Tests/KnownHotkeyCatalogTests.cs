using HotkeyInspector.Core;

namespace HotkeyInspector.Tests;

public sealed class KnownHotkeyCatalogTests
{
    [Fact]
    public void Entries_AreNotSilentlyLost()
    {
        // 368 条写入 → 271 个唯一组合 → 59 个组合有多归属。若目录扩充，请同步更新这些数字。
        Assert.Equal(368, KnownHotkeyCatalog.EntryCount);
        Assert.Equal(271, KnownHotkeyCatalog.GetAllEntries().Select(e => e.Key).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(59, KnownHotkeyCatalog.ConflictCount);
    }

    [Theory]
    [InlineData("Ctrl+Shift+Esc", "任务管理器 / 其他工具")]
    [InlineData("F1", "Windows/应用帮助")]
    [InlineData("F3", "Windows 搜索")]
    [InlineData("Ctrl+T", "浏览器")]
    [InlineData("Alt+Space", "PowerToys PowerToys Run")]
    public void Lookup_PrefersHigherPriorityCategory(string combo, string expectedOwnerZh)
        => Assert.Equal(expectedOwnerZh, KnownHotkeyCatalog.Lookup(combo).OwnerApplication.Zh);

    [Fact]
    public void Lookup_NormalizesModifierOrder()
        => Assert.Equal(
            KnownHotkeyCatalog.Lookup("Ctrl+Shift+T").OwnerApplication.Zh,
            KnownHotkeyCatalog.Lookup("Shift+Ctrl+T").OwnerApplication.Zh);

    [Fact]
    public void Lookup_KeepsAlternativeOwners()
    {
        var matches = KnownHotkeyCatalog.LookupAll("Ctrl+Shift+Esc");

        Assert.Equal(2, matches.Count);
        Assert.Contains(matches, m => m.OwnerApplication.Zh == "Microsoft Teams");
    }

    [Fact]
    public void Lookup_UnknownComboReturnsUnknownOccupied()
        => Assert.Same(KnownHotkeyCatalog.UnknownOccupied, KnownHotkeyCatalog.Lookup("Ctrl+PageUp"));
}
