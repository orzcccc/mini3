using UnityEditor;

/// <summary>
/// 监听 Resources 资源变化，自动重建资源注册表。
/// </summary>
public sealed class UIResourceRegistryPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (UICodeGenerator.IsGeneratingResourceRegistry)
        {
            return;
        }

        if (!HasRelevantAssetChange(importedAssets)
            && !HasRelevantAssetChange(deletedAssets)
            && !HasRelevantAssetChange(movedAssets)
            && !HasRelevantAssetChange(movedFromAssetPaths))
        {
            return;
        }

        if (!UICodeGenerator.GenerateResourceRegistry(false, out string errorMessage))
        {
            UnityEngine.Debug.LogError($"[UI] 自动生成资源注册表失败：{errorMessage}");
            return;
        }

        AssetDatabase.ImportAsset("Assets/src/Script/UI/Config/UIPathRegistry.Generated.cs");
    }

    private static bool HasRelevantAssetChange(string[] assetPaths)
    {
        if (assetPaths == null)
        {
            return false;
        }

        for (int i = 0; i < assetPaths.Length; i++)
        {
            string assetPath = assetPaths[i];
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                continue;
            }

            string normalizedPath = assetPath.Replace('\\', '/');
            if (!normalizedPath.StartsWith("Assets/Resources/", System.StringComparison.Ordinal))
            {
                continue;
            }

            if (normalizedPath.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)
                || normalizedPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                || normalizedPath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase)
                || normalizedPath.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase)
                || normalizedPath.EndsWith(".tga", System.StringComparison.OrdinalIgnoreCase)
                || normalizedPath.EndsWith(".psd", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
