using System;
using System.Collections.Generic;

/// <summary>
/// 业务 UI 定义表，负责 UI 名称、资源名与层级的映射。
/// </summary>
public static class UIFormDefine
{
    private static readonly Dictionary<string, UIFormConfig> s_ConfigByUIName = new Dictionary<string, UIFormConfig>(StringComparer.Ordinal);

    static UIFormDefine()
    {
        Register(new UIFormConfig("BattleMainView", "BattleMainView", UIGroupName.Normal, false, 0));
    }

    public static bool TryGet(string uiName, out UIFormConfig config)
    {
        if (string.IsNullOrWhiteSpace(uiName))
        {
            config = default;
            return false;
        }

        if (s_ConfigByUIName.TryGetValue(uiName, out config))
        {
            return true;
        }

        config = CreateDefault(uiName);
        s_ConfigByUIName[uiName] = config;
        return true;
    }

    public static UIFormConfig Get(string uiName)
    {
        if (!TryGet(uiName, out UIFormConfig config))
        {
            throw new ArgumentException("UI name is invalid.", nameof(uiName));
        }

        return config;
    }

    public static void Register(UIFormConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.UIName))
        {
            throw new ArgumentException("UI name is invalid.", nameof(config));
        }

        s_ConfigByUIName[config.UIName] = config;
    }

    private static UIFormConfig CreateDefault(string uiName)
    {
        return new UIFormConfig(uiName, uiName, UIGroupName.Normal, false, 0);
    }
}

/// <summary>
/// 单个 UI 的静态配置。
/// </summary>
public readonly struct UIFormConfig
{
    public UIFormConfig(string uiName, string assetName, string groupName, bool pauseCoveredUIForm, int priority)
    {
        UIName = uiName;
        AssetName = assetName;
        GroupName = groupName;
        PauseCoveredUIForm = pauseCoveredUIForm;
        Priority = priority;
    }

    public string UIName { get; }

    public string AssetName { get; }

    public string GroupName { get; }

    public bool PauseCoveredUIForm { get; }

    public int Priority { get; }
}
