using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mini3.Editor.UI
{
    /// <summary>
    /// 校验 Resources 下 UI 与图片资源的唯一命名规则。
    /// </summary>
    public static class UIResourceNameValidator
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string ImageRoot = "Assets/Resources/Image";

        public static bool TryCollectResources(out Dictionary<string, string> prefabPaths, out Dictionary<string, string> imagePaths, out string errorMessage)
        {
            prefabPaths = new Dictionary<string, string>(StringComparer.Ordinal);
            imagePaths = new Dictionary<string, string>(StringComparer.Ordinal);
            errorMessage = string.Empty;

            if (!CollectResourceMap("t:Prefab", ResourcesRoot, prefabPaths, out errorMessage))
            {
                return false;
            }

            if (!CollectResourceMap("t:Sprite", ImageRoot, imagePaths, out errorMessage))
            {
                return false;
            }

            return true;
        }

        private static bool CollectResourceMap(string filter, string searchRoot, IDictionary<string, string> result, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!AssetDatabase.IsValidFolder(searchRoot))
            {
                return true;
            }

            string[] guids = AssetDatabase.FindAssets(filter, new[] { searchRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                string assetName = Path.GetFileNameWithoutExtension(assetPath);
                string resourcePath = ToResourcesPath(assetPath);
                if (string.IsNullOrWhiteSpace(resourcePath))
                {
                    continue;
                }

                if (result.TryGetValue(assetName, out string existedPath))
                {
                    errorMessage = $"Resources 资源命名重复：{assetName}\n{existedPath}\n{assetPath}";
                    return false;
                }

                result[assetName] = resourcePath;
            }

            return true;
        }

        public static string ToResourcesPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            string normalizedPath = assetPath.Replace('\\', '/');
            int resourcesIndex = normalizedPath.IndexOf("/Resources/", StringComparison.Ordinal);
            if (resourcesIndex < 0)
            {
                return string.Empty;
            }

            int pathStart = resourcesIndex + "/Resources/".Length;
            string relativePath = normalizedPath.Substring(pathStart);
            return Path.ChangeExtension(relativePath, null)?.Replace('\\', '/');
        }
    }
}
