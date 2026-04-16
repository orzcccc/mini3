using System;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Mini3
{
    /// <summary>
    /// 业务 UI 门面，统一封装 UGF 的 UI 打开关闭流程。
    /// </summary>
    public sealed class UIMgr : Singleton<UIMgr>
    {
        private const string UICanvasAssetName = "UICanvas";
        private const string UIRootNodeName = "UIRoot";
        private const string LowLayerNodeName = "LowLayer";
        private const string MainLayerNodeName = "MainLayer";
        private const string MiddleLayerNodeName = "MiddleLayer";
        private const string HighLayerNodeName = "HighLayer";
        private const string TopLayerNodeName = "TopLayer";

        private readonly Dictionary<string, int> m_SerialIdByUIName = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<int, string> m_UINameBySerialId = new Dictionary<int, string>();

        private GameObject m_UICanvasInstance;
        private Transform m_UIRoot;
        private Transform m_LowLayer;
        private Transform m_MainLayer;
        private Transform m_MiddleLayer;
        private Transform m_HighLayer;
        private Transform m_TopLayer;

        protected override void OnInit()
        {
            base.OnInit();
            InitUICanvas();
            EnsureDefaultGroups();
            RegisterFrameworkEvents();
        }

        public int Open(string uiName, object userData = null)
        {
            UIFormConfig config = UIFormDefine.Get(uiName);
            if (!ResMgr.inst.TryGetPrefabPath(config.AssetName, out string resourcePath))
            {
                resourcePath = config.AssetName;
            }

            EnsureGroup(config.GroupName);

            int serialId = GetUIComponent().OpenUIForm(resourcePath, config.GroupName, config.Priority, config.PauseCoveredUIForm, userData);
            if (serialId > 0)
            {
                m_SerialIdByUIName[config.UIName] = serialId;
                m_UINameBySerialId[serialId] = config.UIName;
            }

            return serialId;
        }

        public void Close(string uiName, object userData = null)
        {
            if (!m_SerialIdByUIName.TryGetValue(uiName, out int serialId))
            {
                UIForm uiFormByAsset = TryGetUIFormByConfig(uiName);
                if (uiFormByAsset == null)
                {
                    return;
                }

                serialId = uiFormByAsset.SerialId;
            }

            Close(serialId, userData);
        }

        public void Close(int serialId, object userData = null)
        {
            if (serialId <= 0 || !GetUIComponent().HasUIForm(serialId))
            {
                return;
            }

            if (userData == null)
            {
                GetUIComponent().CloseUIForm(serialId);
            }
            else
            {
                GetUIComponent().CloseUIForm(serialId, userData);
            }
        }

        public bool IsOpen(string uiName)
        {
            UIForm uiForm = TryGetUIFormByConfig(uiName);
            return uiForm != null;
        }

        public T GetUI<T>(string uiName) where T : BaseUI
        {
            UIForm uiForm = TryGetUIFormByConfig(uiName);
            return uiForm != null ? uiForm.Logic as T : null;
        }

        public UIForm GetUI(string uiName)
        {
            return TryGetUIFormByConfig(uiName);
        }

        public void CloseAll()
        {
            GetUIComponent().CloseAllLoadedUIForms();
            GetUIComponent().CloseAllLoadingUIForms();
            m_SerialIdByUIName.Clear();
            m_UINameBySerialId.Clear();
        }

        public void Register(string uiName, string assetName = null, string groupName = UIGroupName.Normal, bool pauseCoveredUIForm = false, int priority = 0)
        {
            string finalAssetName = string.IsNullOrWhiteSpace(assetName) ? uiName : assetName;
            UIFormDefine.Register(new UIFormConfig(uiName, finalAssetName, groupName, pauseCoveredUIForm, priority));
        }

        public Transform GetLayerRoot(UILayerName layerName)
        {
            switch (layerName)
            {
                case UILayerName.LowLayer:
                    return m_LowLayer;
                case UILayerName.MiddleLayer:
                    return m_MiddleLayer;
                case UILayerName.HighLayer:
                    return m_HighLayer;
                case UILayerName.TopLayer:
                    return m_TopLayer;
                case UILayerName.MainLayer:
                default:
                    return m_MainLayer;
            }
        }

        private void EnsureDefaultGroups()
        {
            IReadOnlyList<UIGroupConfig> groups = UIGroupName.DefaultGroups;
            for (int i = 0; i < groups.Count; i++)
            {
                EnsureGroup(groups[i].Name, groups[i].Depth);
            }
        }

        private void EnsureGroup(string groupName, int depth = 0)
        {
            UIComponent uiComponent = GetUIComponent();
            if (uiComponent.HasUIGroup(groupName))
            {
                return;
            }

            uiComponent.AddUIGroup(groupName, depth);
        }

        private void RegisterFrameworkEvents()
        {
            EventMgr.inst.AddEvent(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            EventMgr.inst.AddEvent(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);
            EventMgr.inst.AddEvent(CloseUIFormCompleteEventArgs.EventId, OnCloseUIFormComplete);
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = e as OpenUIFormSuccessEventArgs;
            if (ne?.UIForm == null)
            {
                return;
            }

            string uiName = ResolveUIName(ne.UIForm.UIFormAssetName, ne.UIForm.SerialId);
            m_SerialIdByUIName[uiName] = ne.UIForm.SerialId;
            m_UINameBySerialId[ne.UIForm.SerialId] = uiName;
        }

        private void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormFailureEventArgs ne = e as OpenUIFormFailureEventArgs;
            if (ne == null)
            {
                return;
            }

            string uiName = ResolveUIName(ne.UIFormAssetName, ne.SerialId);
            m_SerialIdByUIName.Remove(uiName);
            m_UINameBySerialId.Remove(ne.SerialId);
            Debug.LogWarning($"UIMgr open ui failed. uiName = {uiName}, asset = {ne.UIFormAssetName}, error = {ne.ErrorMessage}");
        }

        private void OnCloseUIFormComplete(object sender, GameEventArgs e)
        {
            CloseUIFormCompleteEventArgs ne = e as CloseUIFormCompleteEventArgs;
            if (ne == null)
            {
                return;
            }

            if (m_UINameBySerialId.TryGetValue(ne.SerialId, out string uiName))
            {
                m_UINameBySerialId.Remove(ne.SerialId);
                m_SerialIdByUIName.Remove(uiName);
                return;
            }

            string resolvedUIName = ResolveUIName(ne.UIFormAssetName, ne.SerialId);
            m_SerialIdByUIName.Remove(resolvedUIName);
        }

        private UIForm TryGetUIFormByConfig(string uiName)
        {
            UIFormConfig config = UIFormDefine.Get(uiName);
            if (m_SerialIdByUIName.TryGetValue(config.UIName, out int serialId) && GetUIComponent().HasUIForm(serialId))
            {
                return GetUIComponent().GetUIForm(serialId);
            }

            if (ResMgr.inst.TryGetPrefabPath(config.AssetName, out string resourcePath))
            {
                return GetUIComponent().GetUIForm(resourcePath);
            }

            return GetUIComponent().GetUIForm(config.AssetName);
        }

        private string ResolveUIName(string uiFormAssetName, int serialId)
        {
            if (m_UINameBySerialId.TryGetValue(serialId, out string uiName))
            {
                return uiName;
            }

            foreach (KeyValuePair<string, int> pair in m_SerialIdByUIName)
            {
                if (pair.Value == serialId)
                {
                    return pair.Key;
                }
            }

            return string.IsNullOrWhiteSpace(uiFormAssetName) ? serialId.ToString() : uiFormAssetName;
        }

        private void InitUICanvas()
        {
            if (m_UICanvasInstance != null)
            {
                return;
            }

            GameObject uiCanvasPrefab = ResMgr.inst.LoadPrefab(UICanvasAssetName);
            if (uiCanvasPrefab == null)
            {
                Debug.LogWarning($"UIMgr init ui canvas failed, prefab not found. assetName = {UICanvasAssetName}");
                return;
            }

            m_UICanvasInstance = UnityEngine.Object.Instantiate(uiCanvasPrefab);
            m_UICanvasInstance.name = uiCanvasPrefab.name;
            UnityEngine.Object.DontDestroyOnLoad(m_UICanvasInstance);

            Transform canvasTransform = m_UICanvasInstance.transform;
            m_UIRoot = canvasTransform.Find(UIRootNodeName);
            if (m_UIRoot == null)
            {
                Debug.LogWarning($"UIMgr init ui canvas failed, node not found. nodeName = {UIRootNodeName}");
                return;
            }

            m_LowLayer = m_UIRoot.Find(LowLayerNodeName);
            m_MainLayer = m_UIRoot.Find(MainLayerNodeName);
            m_MiddleLayer = m_UIRoot.Find(MiddleLayerNodeName);
            m_HighLayer = m_UIRoot.Find(HighLayerNodeName);
            m_TopLayer = m_UIRoot.Find(TopLayerNodeName);
        }

        private static UIComponent GetUIComponent()
        {
            UIComponent uiComponent = GameEntry.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                throw new Exception("UIComponent is not found. Please check GameFramework scene setup.");
            }

            return uiComponent;
        }
    }
}
