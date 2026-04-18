using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mini3.Editor.UI
{
    /// <summary>
    /// UIWidget 的右键工具入口。
    /// </summary>
    public static class UIWidgetEditor
    {
        [MenuItem("Tools/UI/Generate Resource Registry")]
        public static void GenerateResourceRegistry()
        {
            if (!UICodeGenerator.GenerateResourceRegistry(out string errorMessage))
            {
                Debug.LogError($"[UI] 生成资源注册表失败：{errorMessage}");
                return;
            }

            Debug.Log("[UI] 资源注册表已生成。");
        }

        [MenuItem("CONTEXT/UIWidget/Create Script")]
        private static void CreateScript(MenuCommand command)
        {
            GenerateUIScript(command, UICodeGenerator.UIScriptWriteMode.Create, "创建");
        }

        [MenuItem("CONTEXT/UIWidget/Update Script")]
        private static void UpdateScript(MenuCommand command)
        {
            GenerateUIScript(command, UICodeGenerator.UIScriptWriteMode.Update, "更新");
        }

        [MenuItem("CONTEXT/UIWidget/Validate Resource Names")]
        private static void ValidateResourceNames(MenuCommand command)
        {
            if (UIResourceNameValidator.TryCollectResources(out _, out _, out string errorMessage))
            {
                Debug.Log("[UI] Resources 命名校验通过。");
                return;
            }

            Debug.LogError($"[UI] Resources 命名校验失败：{errorMessage}");
        }

        [MenuItem("CONTEXT/UIWidget/Open Script Folder")]
        private static void OpenScriptFolder(MenuCommand command)
        {
            UIWidget uiWidget = command.context as UIWidget;
            if (uiWidget == null)
            {
                return;
            }

            string moduleName = ResolveModuleName(uiWidget);
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                Debug.LogWarning($"[UI] 无法定位脚本目录，未能推断模块名：{uiWidget.name}");
                return;
            }

            string uiName = string.IsNullOrWhiteSpace(uiWidget.UIName) ? uiWidget.gameObject.name : uiWidget.UIName;
            string folderPath = $"Assets/src/Script/UI/{moduleName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"[UI] 脚本目录尚未创建：{folderPath}");
                return;
            }

            Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folder;
        }

        private static void GenerateUIScript(MenuCommand command, UICodeGenerator.UIScriptWriteMode writeMode, string actionName)
        {
            UIWidget uiWidget = command.context as UIWidget;
            if (uiWidget == null)
            {
                return;
            }

            if (!RescanBindings(uiWidget, out int bindingCount, out string errorMessage))
            {
                Debug.LogError($"[UI] 扫描绑定失败：{errorMessage}");
                return;
            }

            if (!UICodeGenerator.GenerateUIScript(uiWidget, writeMode, out errorMessage))
            {
                Debug.LogError($"[UI] {actionName} UI 脚本失败：{errorMessage}");
                return;
            }

            Debug.Log($"[UI] 已{actionName} UI 脚本：{uiWidget.UIName}，绑定数量: {bindingCount}");
        }

        private static bool RescanBindings(UIWidget uiWidget, out int bindingCount, out string errorMessage)
        {
            bindingCount = 0;
            errorMessage = string.Empty;
            if (uiWidget == null)
            {
                errorMessage = "UIWidget is null.";
                return false;
            }

            if (!UIBindingScanner.TryScan(uiWidget, out List<UIBindData> bindings, out errorMessage))
            {
                return false;
            }

            Undo.RecordObject(uiWidget, "Rescan UI Bindings");
            uiWidget.Bindings.Clear();
            uiWidget.Bindings.AddRange(bindings);
            if (string.IsNullOrWhiteSpace(uiWidget.UIName))
            {
                uiWidget.UIName = uiWidget.gameObject.name;
            }

            bindingCount = bindings.Count;
            EditorUtility.SetDirty(uiWidget);
            AssetDatabase.SaveAssets();
            return true;
        }

        private static string ResolveModuleName(UIWidget uiWidget)
        {
            if (!string.IsNullOrWhiteSpace(uiWidget.ModuleName))
            {
                return uiWidget.ModuleName;
            }

            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(uiWidget.gameObject);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                assetPath = AssetDatabase.GetAssetPath(uiWidget.gameObject);
            }

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            string folderPath = System.IO.Path.GetDirectoryName(assetPath);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return string.Empty;
            }

            folderPath = folderPath.Replace('\\', '/');
            return System.IO.Path.GetFileName(folderPath);
        }
    }
}
