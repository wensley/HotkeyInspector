using HotkeyInspector.Core;

namespace HotkeyInspector.Tests;

public sealed class HotkeyKnowledgeTests
{
    [Fact]
    public void Base_ContainsExpectedItems()
    {
        var items = HotkeyKnowledge.Base.Categories
            .Sum(category => category.Apps.Sum(app => app.Groups.Sum(group => group.Items.Count)));

        Assert.Equal(3056, items);
    }

    [Fact]
    public void FindMatches_ReturnsKnownShortcut()
        => Assert.NotEmpty(HotkeyKnowledge.FindMatches("Ctrl+Shift+Esc"));

    [Fact]
    public void FindMatches_NormalizesPunctuationKeys()
        => Assert.NotEmpty(HotkeyKnowledge.FindMatches("Ctrl+Shift+]"));

    [Fact]
    public void FindMatches_UnknownComboIsEmpty()
        => Assert.Empty(HotkeyKnowledge.FindMatches("Qqq+Zzz"));
}
