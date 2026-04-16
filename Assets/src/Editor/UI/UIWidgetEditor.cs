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

        [MenuItem("CONTEXT/UIWidget/Rescan Bindings")]
        private static void RescanBindings(MenuCommand command)
        {
            UIWidget uiWidget = command.context as UIWidget;
            if (uiWidget == null)
            {
                return;
            }

            if (!UIBindingScanner.TryScan(uiWidget, out List<UIBindData> bindings, out string errorMessage))
            {
                Debug.LogError($"[UI] 扫描绑定失败：{errorMessage}");
                return;
            }

            Undo.RecordObject(uiWidget, "Rescan UI Bindings");
            uiWidget.Bindings.Clear();
            uiWidget.Bindings.AddRange(bindings);
            if (string.IsNullOrWhiteSpace(uiWidget.UIName))
            {
                uiWidget.UIName = uiWidget.gameObject.name;
            }

            EditorUtility.SetDirty(uiWidget);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UI] 扫描完成，绑定数量: {bindings.Count}");
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

        [MenuItem("CONTEXT/UIWidget/Generate UI Script")]
        private static void GenerateUIScript(MenuCommand command)
        {
            UIWidget uiWidget = command.context as UIWidget;
            if (uiWidget == null)
            {
                return;
            }

            if (!UICodeGenerator.GenerateUIScript(uiWidget, out string errorMessage))
            {
                Debug.LogError($"[UI] 生成 UI 脚本失败：{errorMessage}");
                return;
            }

            Debug.Log($"[UI] 已生成 UI 脚本：{uiWidget.UIName}");
        }

        [MenuItem("CONTEXT/UIWidget/Open Script Folder")]
        private static void OpenScriptFolder(MenuCommand command)
        {
            UIWidget uiWidget = command.context as UIWidget;
            if (uiWidget == null)
            {
                return;
            }

            string moduleName = string.IsNullOrWhiteSpace(uiWidget.ModuleName) ? "Common" : uiWidget.ModuleName;
            string uiName = string.IsNullOrWhiteSpace(uiWidget.UIName) ? uiWidget.gameObject.name : uiWidget.UIName;
            string folderPath = $"Assets/src/Script/UI/{moduleName}/{uiName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"[UI] 脚本目录尚未创建：{folderPath}");
                return;
            }

            Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folder;
        }
    }
}
