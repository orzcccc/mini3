using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mini3
{
    /// <summary>
    /// Resources 资源统一入口，基于唯一命名注册表自动寻址。
    /// </summary>
    public sealed class ResMgr : Singleton<ResMgr>
    {
        private readonly Dictionary<string, UnityEngine.Object> m_ObjectCache = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> m_PrefabPathByName = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> m_SpritePathByName = new Dictionary<string, string>(StringComparer.Ordinal);

        protected override void OnInit()
        {
            base.OnInit();
            RebuildRegistry();
        }

        public void RebuildRegistry()
        {
            m_PrefabPathByName.Clear();
            m_SpritePathByName.Clear();

            foreach (KeyValuePair<string, string> pair in UIPathRegistry.UIPathByName)
            {
                m_PrefabPathByName[pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<string, string> pair in UIPathRegistry.ImagePathByName)
            {
                m_SpritePathByName[pair.Key] = pair.Value;
            }
        }

        public GameObject LoadPrefab(string assetName)
        {
            return Load<GameObject>(assetName, m_PrefabPathByName);
        }

        public Sprite LoadSprite(string assetName)
        {
            return Load<Sprite>(assetName, m_SpritePathByName);
        }

        public T Load<T>(string assetName) where T : UnityEngine.Object
        {
            Type targetType = typeof(T);
            if (targetType == typeof(GameObject))
            {
                return LoadPrefab(assetName) as T;
            }

            if (targetType == typeof(Sprite))
            {
                return LoadSprite(assetName) as T;
            }

            if (TryLoad<T>(assetName, m_PrefabPathByName, out T prefabResult))
            {
                return prefabResult;
            }

            if (TryLoad<T>(assetName, m_SpritePathByName, out T spriteResult))
            {
                return spriteResult;
            }

            Debug.LogWarning($"ResMgr.Load failed, asset is not registered. assetName = {assetName}, type = {targetType.Name}");
            return null;
        }

        public bool TryGetPath(string assetName, out string resourcePath)
        {
            if (m_PrefabPathByName.TryGetValue(assetName, out resourcePath))
            {
                return true;
            }

            return m_SpritePathByName.TryGetValue(assetName, out resourcePath);
        }

        public bool TryGetUIPrefabPath(string assetName, out string resourcePath)
        {
            return m_PrefabPathByName.TryGetValue(assetName, out resourcePath);
        }

        public bool TryGetImagePath(string assetName, out string resourcePath)
        {
            return m_SpritePathByName.TryGetValue(assetName, out resourcePath);
        }

        private T Load<T>(string assetName, IDictionary<string, string> registry) where T : UnityEngine.Object
        {
            if (!registry.TryGetValue(assetName, out string resourcePath))
            {
                Debug.LogWarning($"ResMgr.Load failed, asset is not registered. assetName = {assetName}, type = {typeof(T).Name}");
                return null;
            }

            if (m_ObjectCache.TryGetValue(resourcePath, out UnityEngine.Object cachedObject))
            {
                return cachedObject as T;
            }

            T asset = Resources.Load<T>(resourcePath);
            if (asset == null)
            {
                Debug.LogWarning($"ResMgr.Load failed, can not load asset. assetName = {assetName}, path = {resourcePath}, type = {typeof(T).Name}");
                return null;
            }

            m_ObjectCache[resourcePath] = asset;
            return asset;
        }

        private bool TryLoad<T>(string assetName, IDictionary<string, string> registry, out T asset) where T : UnityEngine.Object
        {
            asset = Load<T>(assetName, registry);
            return asset != null;
        }
    }
}
