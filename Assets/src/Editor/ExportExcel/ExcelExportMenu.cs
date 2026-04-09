using UnityEditor;
using UnityEngine;

namespace Mini3.Editor.ExportExcel
{
    public static class ExcelExportMenu
    {
        [MenuItem("Tools/Excel/Create Settings Asset")]
        public static void CreateSettingsAsset()
        {
            ExcelExportSettings asset = ScriptableObject.CreateInstance<ExcelExportSettings>();
            AssetDatabase.CreateAsset(asset, ExcelExportSettings.DefaultAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        [MenuItem("Tools/Excel/Export All")]
        public static void ExportAll()
        {
            ExcelExportSettings settings = LoadSettings();
            if (settings == null)
            {
                Debug.LogError("[Excel导表] 请先通过 Tools/Excel/Create Settings Asset 创建配置文件。");
                return;
            }

            ExcelExportPipeline.ExportAll(settings);
        }

        private static ExcelExportSettings LoadSettings()
        {
            return AssetDatabase.LoadAssetAtPath<ExcelExportSettings>(ExcelExportSettings.DefaultAssetPath);
        }
    }
}
