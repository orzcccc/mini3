using System.Collections.Generic;

/// <summary>
/// 业务 UI 分层名称与默认深度。
/// </summary>
public static class UIGroupName
{
    public const string Background = "Background";
    public const string Normal = "Normal";
    public const string Popup = "Popup";
    public const string Top = "Top";
    public const string Guide = "Guide";
    public const string Loading = "Loading";

    private static readonly UIGroupConfig[] s_DefaultGroups =
    {
        new UIGroupConfig(Background, 0),
        new UIGroupConfig(Normal, 100),
        new UIGroupConfig(Popup, 200),
        new UIGroupConfig(Top, 300),
        new UIGroupConfig(Guide, 400),
        new UIGroupConfig(Loading, 500)
    };

    public static IReadOnlyList<UIGroupConfig> DefaultGroups => s_DefaultGroups;
}

/// <summary>
/// UI 分层配置。
/// </summary>
public struct UIGroupConfig
{
    public UIGroupConfig(string name, int depth)
    {
        Name = name;
        Depth = depth;
    }

    public string Name { get; }

    public int Depth { get; }
}
