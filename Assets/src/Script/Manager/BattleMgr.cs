using UnityEngine;

/// <summary>
/// 战斗入口管理器，负责创建与回收战斗根节点。
/// </summary>
public sealed class BattleMgr : Singleton<BattleMgr>
{
    private const string BattleRootAssetName = "battleRoot";

    private GameObject m_BattleRootInstance;

    public bool IsBattleRunning => m_BattleRootInstance != null;

    public GameObject StartBattle()
    {
        if (m_BattleRootInstance != null)
        {
            return m_BattleRootInstance;
        }

        GameObject battleRootPrefab = ResMgr.inst.LoadPrefab(BattleRootAssetName);
        if (battleRootPrefab == null)
        {
            Debug.LogError("BattleMgr.StartBattle failed, battleRoot prefab is not found.");
            return null;
        }

        m_BattleRootInstance = Object.Instantiate(battleRootPrefab);
        m_BattleRootInstance.name = battleRootPrefab.name;
        AttachBattleBackgroundFitter(m_BattleRootInstance);
        return m_BattleRootInstance;
    }

    public void EndBattle()
    {
        if (m_BattleRootInstance == null)
        {
            return;
        }

        Object.Destroy(m_BattleRootInstance);
        m_BattleRootInstance = null;
    }

    private static void AttachBattleBackgroundFitter(GameObject battleRootInstance)
    {
        if (battleRootInstance == null)
        {
            return;
        }

        Transform battleBackground = battleRootInstance.transform.Find("battleBG");
        if (battleBackground == null)
        {
            return;
        }

        BattleBackgroundFitter fitter = battleBackground.GetComponent<BattleBackgroundFitter>();
        if (fitter == null)
        {
            fitter = battleBackground.gameObject.AddComponent<BattleBackgroundFitter>();
        }

        fitter.ApplyBackground();
    }
}
