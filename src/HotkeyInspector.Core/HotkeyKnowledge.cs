using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotkeyInspector.Core;

public sealed class KnowledgeMeta
{
    public string Version { get; init; } = string.Empty;
    public string Updated { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
}

public sealed class KnowledgeBase
{
    public KnowledgeMeta Meta { get; init; } = new();
    public List<KnowledgeCategory> Categories { get; init; } = [];
}

public sealed class KnowledgeCategory
{
    public string Id { get; init; } = string.Empty;
    public LocalizedText Name { get; init; } = new("", "");
    public LocalizedText Desc { get; init; } = new("", "");
    public List<KnowledgeApp> Apps { get; init; } = [];
}

public sealed class KnowledgeApp
{
    public string Id { get; init; } = string.Empty;
    public LocalizedText Name { get; init; } = new("", "");
    public LocalizedText? Vendor { get; init; }
    public LocalizedText? Note { get; init; }
    public List<KnowledgeGroup> Groups { get; init; } = [];
}

public sealed class KnowledgeGroup
{
    public LocalizedText Name { get; init; } = new("", "");
    public List<KnowledgeItem> Items { get; init; } = [];
}

public sealed class KnowledgeItem
{
    [JsonPropertyName("k")]
    public string K { get; init; } = string.Empty;

    [JsonPropertyName("zh")]
    public string Zh { get; init; } = string.Empty;

    [JsonPropertyName("en")]
    public string En { get; init; } = string.Empty;

    public LocalizedText Action => new(Zh, En);
}

/// <summary>一条命中记录：包含所在分类 / 应用 / 分组上下文与动作说明。</summary>
public sealed record KnowledgeMatch(
    LocalizedText Category,
    LocalizedText App,
    LocalizedText Group,
    LocalizedText Action);

/// <summary>从嵌入资源加载的快捷键知识库（9 分类 / 60 应用 / 3000+ 条目），并按归一化键位建立索引。</summary>
public static class HotkeyKnowledge
{
    private const string ResourceName = "HotkeyInspector.Core.Data.hotkeys.json";

    private static readonly Lazy<KnowledgeBase> LazyBase = new(Load, isThreadSafe: true);
    private static readonly Lazy<Dictionary<string, List<KnowledgeMatch>>> LazyIndex = new(BuildIndex, isThreadSafe: true);

    public static KnowledgeBase Base => LazyBase.Value;

    public static IEnumerable<KnowledgeMatch> FindMatches(string displayText)
        => LazyIndex.Value.TryGetValue(ComboKey.Normalize(displayText), out var list) ? list : [];

    private static Dictionary<string, List<KnowledgeMatch>> BuildIndex()
    {
        var index = new Dictionary<string, List<KnowledgeMatch>>(StringComparer.Ordinal);
        foreach (var category in Base.Categories)
        {
            foreach (var app in category.Apps)
            {
                foreach (var group in app.Groups)
                {
                    foreach (var item in group.Items)
                    {
                        var key = ComboKey.Normalize(item.K);
                        if (key.Length == 0)
                        {
                            continue;
                        }

                        if (!index.TryGetValue(key, out var list))
                        {
                            list = [];
                            index[key] = list;
                        }

                        list.Add(new KnowledgeMatch(category.Name, app.Name, group.Name, item.Action));
                    }
                }
            }
        }

        return index;
    }

    private static KnowledgeBase Load()
    {
        try
        {
            using var stream = typeof(HotkeyKnowledge).Assembly.GetManifestResourceStream(ResourceName);
            if (stream is null)
            {
                Debug.WriteLine($"[HotkeyKnowledge] 找不到嵌入资源 {ResourceName}，知识库匹配将不可用。");
                return new KnowledgeBase();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<KnowledgeBase>(stream, options) ?? new KnowledgeBase();
        }
        catch (Exception ex)
        {
            // 知识库只是辅助信息，加载失败时降级为空库，不影响快捷键检测主流程。
            Debug.WriteLine($"[HotkeyKnowledge] 加载知识库失败：{ex}");
            return new KnowledgeBase();
        }
    }
}
