using UnityEngine;
using GameFramework.Event;
using UnityGameFramework.Runtime;

/// <summary>
/// 业务启动入口，负责在框架组件就绪后初始化项目业务系统。
/// </summary>
public sealed class GameLauncher : MonoBehaviour
{
    private bool m_Launched;

    private void Start()
    {
        Launch();
    }

    public void Launch()
    {
        if (m_Launched)
        {
            return;
        }

        if (!CheckFrameworkReady())
        {
            return;
        }

        m_Launched = true;
        InitManagers();
        InitGameModules();
    }

    private static bool CheckFrameworkReady()
    {
        if (GameEntry.GetComponent<BaseComponent>() == null)
        {
            Debug.LogError("GameLauncher 启动失败：未找到 BaseComponent，请确认场景中已放置 GameFramework prefab。");
            return false;
        }

        if (GameEntry.GetComponent<DataTableComponent>() == null)
        {
            Debug.LogError("GameLauncher 启动失败：未找到 DataTableComponent，请确认 GameFramework prefab 配置完整。");
            return false;
        }

        if (GameEntry.GetComponent<EventComponent>() == null)
        {
            Debug.LogError("GameLauncher 启动失败：未找到 EventComponent，请确认 GameFramework prefab 配置完整。");
            return false;
        }

        if (GameEntry.GetComponent<UIComponent>() == null)
        {
            Debug.LogError("GameLauncher 启动失败：未找到 UIComponent，请确认 GameFramework prefab 配置完整。");
            return false;
        }

        return true;
    }

    private static void InitManagers()
    {
        int loadedTableCount = TableMgr.inst.LoadAll();
        EventMgr.inst.SetDefaultHandler(OnDefaultGameEvent);
        ResMgr.inst.RebuildRegistry();
        Debug.Log($"GameLauncher 初始化完成，已加载数据表数量: {loadedTableCount}");
    }

    private static void InitGameModules()
    {
        _ = UIMgr.inst;
        UIMgr.inst.Open("MainCardView");
    }

    private static void OnDefaultGameEvent(object sender, GameEventArgs e)
    {
        Debug.LogWarning($"未处理的框架事件，Id = {e.Id}, 类型 = {e.GetType().Name}");
    }
}
