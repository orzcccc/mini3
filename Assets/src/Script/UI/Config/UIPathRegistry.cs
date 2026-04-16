using System;
using System.Collections.Generic;

namespace Mini3
{
    /// <summary>
    /// Resources 资源名到路径的静态注册表，默认由编辑器工具生成。
    /// </summary>
    public static partial class UIPathRegistry
    {
        private static readonly Dictionary<string, string> s_PrefabPathByName = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> s_ImagePathByName = new Dictionary<string, string>(StringComparer.Ordinal);

        public static IReadOnlyDictionary<string, string> PrefabPathByName => s_PrefabPathByName;

        public static IReadOnlyDictionary<string, string> ImagePathByName => s_ImagePathByName;

        public static void RegisterPrefab(string assetName, string resourcePath)
        {
            RegisterInternal(s_PrefabPathByName, assetName, resourcePath);
        }

        public static void RegisterImage(string assetName, string resourcePath)
        {
            RegisterInternal(s_ImagePathByName, assetName, resourcePath);
        }

        public static void Clear()
        {
            s_PrefabPathByName.Clear();
            s_ImagePathByName.Clear();
        }

        private static void RegisterInternal(IDictionary<string, string> registry, string assetName, string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(assetName) || string.IsNullOrWhiteSpace(resourcePath))
            {
                return;
            }

            registry[assetName] = resourcePath;
        }
    }
}
