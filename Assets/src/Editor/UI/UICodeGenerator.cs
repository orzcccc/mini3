using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

/// <summary>
/// 生成 UI 路径注册表与单类 UI 脚本。
/// </summary>
public static class UICodeGenerator
{
    public enum UIScriptWriteMode
    {
        Create = 0,
        Update = 1
    }

    private const string UIPathRegistryFile = "Assets/src/Script/UI/Config/UIPathRegistry.Generated.cs";
    private const string UIScriptRoot = "Assets/src/Script/UI";
    private static bool s_IsGeneratingResourceRegistry;

    public static bool IsGeneratingResourceRegistry => s_IsGeneratingResourceRegistry;

    public static bool GenerateResourceRegistry(out string errorMessage)
    {
        return GenerateResourceRegistry(true, out errorMessage);
    }

    public static bool GenerateResourceRegistry(bool refreshAssetDatabase, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (s_IsGeneratingResourceRegistry)
        {
            return true;
        }

        if (!UIResourceNameValidator.TryCollectResources(out Dictionary<string, string> prefabPaths, out Dictionary<string, string> imagePaths, out errorMessage))
        {
            return false;
        }

        try
        {
            s_IsGeneratingResourceRegistry = true;
            string content = GenerateRegistryContent(prefabPaths, imagePaths);
            WriteAllText(UIPathRegistryFile, content);
            if (refreshAssetDatabase)
            {
                AssetDatabase.Refresh();
            }

            return true;
        }
        finally
        {
            s_IsGeneratingResourceRegistry = false;
        }
    }

    public static bool GenerateUIScript(UIWidget uiWidget, out string errorMessage)
    {
        return GenerateUIScript(uiWidget, UIScriptWriteMode.Update, out errorMessage);
    }

    public static bool GenerateUIScript(UIWidget uiWidget, UIScriptWriteMode writeMode, out string errorMessage)
    {
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

        string moduleName = ResolveModuleName(uiWidget);
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            errorMessage = $"无法确定 UIWidget 模块名：{uiWidget.name}";
            return false;
        }

        string uiName = uiWidget.UIName;
        bool isView = UIBindingScanner.IsViewName(uiName);
        string targetDirectory = $"{UIScriptRoot}/{moduleName}";
        string targetFilePath = $"{targetDirectory}/{uiName}.cs";

        if (writeMode == UIScriptWriteMode.Create && File.Exists(targetFilePath))
        {
            errorMessage = $"UI 脚本已存在：{targetFilePath}";
            return false;
        }

        if (writeMode == UIScriptWriteMode.Update && !File.Exists(targetFilePath))
        {
            errorMessage = $"UI 脚本不存在，请先创建：{targetFilePath}";
            return false;
        }

        if (isView)
        {
            UIFormDefine.Register(new UIFormConfig(uiName, uiName, UIGroupName.Normal, false, 0));
        }

        EnsureDirectory(targetDirectory);
        string content = GenerateUIScriptContent(moduleName, uiName, bindings, isView);
        WriteAllText(targetFilePath, content);
        AssetDatabase.Refresh();
        if (!AttachLogicScript(uiWidget, moduleName, uiName, out errorMessage))
        {
            return false;
        }

        return true;
    }

    public static void AutoAttachLogicScriptIfExists(UIWidget uiWidget)
    {
        if (uiWidget == null)
        {
            return;
        }

        string moduleName = ResolveModuleName(uiWidget);
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            return;
        }

        string uiName = uiWidget.UIName;
        if (string.IsNullOrWhiteSpace(uiName))
        {
            return;
        }

        AttachLogicScriptIfExists(uiWidget, moduleName, uiName);
    }

    private static string GenerateRegistryContent(IReadOnlyDictionary<string, string> prefabPaths, IReadOnlyDictionary<string, string> imagePaths)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// 编辑器自动生成的 Resources 资源注册表。");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public static partial class UIPathRegistry");
        builder.AppendLine("{");
        builder.AppendLine("    static UIPathRegistry()");
        builder.AppendLine("    {");

        foreach (KeyValuePair<string, string> pair in prefabPaths)
        {
            builder.AppendFormat("        RegisterPrefab(\"{0}\", \"{1}\");", Escape(pair.Key), Escape(pair.Value)).AppendLine();
        }

        foreach (KeyValuePair<string, string> pair in imagePaths)
        {
            builder.AppendFormat("        RegisterImage(\"{0}\", \"{1}\");", Escape(pair.Key), Escape(pair.Value)).AppendLine();
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string GenerateUIScriptContent(string moduleName, string uiName, IReadOnlyList<UIBindData> bindings, bool isView)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("using UnityEngine;");
        builder.AppendLine("using UnityEngine.UI;");
        builder.AppendLine("using TMPro;");
        builder.AppendLine();
        builder.AppendFormat("public sealed class {0} : {1}", uiName, isView ? "BaseUI" : "BaseItem").AppendLine();
        builder.AppendLine("{");

        AppendFieldDeclarations(builder, bindings);

        if (!isView)
        {
            builder.AppendLine();
            builder.AppendFormat("    public {0}(GameObject root) : base(root)", uiName).AppendLine();
            builder.AppendLine("    {");
            builder.AppendLine("    }");
        }

        if (bindings.Count > 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine("    protected override void BindComponents()");
        builder.AppendLine("    {");
        builder.AppendLine("        base.BindComponents();");
        AppendBindStatements(builder, bindings);
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    protected override void BindEvents()");
        builder.AppendLine("    {");
        builder.AppendLine("        base.BindEvents();");
        AppendButtonStatements(builder, bindings, true);
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    protected override void UnbindEvents()");
        builder.AppendLine("    {");
        AppendButtonStatements(builder, bindings, false);
        builder.AppendLine("        base.UnbindEvents();");
        builder.AppendLine("    }");
        builder.AppendLine();

        if (isView)
        {
            builder.AppendLine();
            builder.AppendLine("    protected override void Init(object userData)");
            builder.AppendLine("    {");
            builder.AppendLine("        base.Init(userData);");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    protected override void AfterOpenView(object userData)");
            builder.AppendLine("    {");
            builder.AppendLine("        base.AfterOpenView(userData);");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    protected override void BeforeCloseView(object userData)");
            builder.AppendLine("    {");
            builder.AppendLine("        base.BeforeCloseView(userData);");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    protected override void Destroy()");
            builder.AppendLine("    {");
            AppendItemDisposeStatements(builder, bindings);
            builder.AppendLine("        base.Destroy();");
            builder.AppendLine("    }");
        }

        AppendButtonHandlers(builder, bindings);

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void AppendFieldDeclarations(StringBuilder builder, IReadOnlyList<UIBindData> bindings)
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            UIBindData binding = bindings[i];
            builder.AppendFormat("        private {0} {1};", GetFieldTypeName(binding), binding.FieldName).AppendLine();
            if (binding.BindType == UIBindType.Item)
            {
                builder.AppendFormat("        private {0} {1};", binding.WidgetTypeName, GetItemInstanceFieldName(binding)).AppendLine();
            }
        }
    }

    private static void AppendBindStatements(StringBuilder builder, IReadOnlyList<UIBindData> bindings)
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            UIBindData binding = bindings[i];
            builder.AppendFormat("            {0} = {1};", binding.FieldName, GenerateBindExpression(binding)).AppendLine();
            if (binding.BindType == UIBindType.Item)
            {
                builder.AppendFormat("            {0} = {1} != null ? new {2}({1}) : null;", GetItemInstanceFieldName(binding), binding.FieldName, binding.WidgetTypeName).AppendLine();
            }
        }
    }

    private static void AppendButtonStatements(StringBuilder builder, IReadOnlyList<UIBindData> bindings, bool isAdd)
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            UIBindData binding = bindings[i];
            if (binding.BindType != UIBindType.Button)
            {
                continue;
            }

            builder.AppendFormat("            if ({0} != null)", binding.FieldName).AppendLine();
            builder.AppendLine("            {");
            builder.AppendFormat(
                "                {0}.onClick.{1}(On{2}Click);",
                binding.FieldName,
                isAdd ? "AddListener" : "RemoveListener",
                ToMethodSuffix(binding.NodeName)).AppendLine();
            builder.AppendLine("            }");
        }
    }

    private static void AppendItemDisposeStatements(StringBuilder builder, IReadOnlyList<UIBindData> bindings)
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            UIBindData binding = bindings[i];
            if (binding.BindType != UIBindType.Item)
            {
                continue;
            }

            string instanceFieldName = GetItemInstanceFieldName(binding);
            builder.AppendFormat("            if ({0} != null)", instanceFieldName).AppendLine();
            builder.AppendLine("            {");
            builder.AppendFormat("                {0}.Dispose();", instanceFieldName).AppendLine();
            builder.AppendFormat("                {0} = null;", instanceFieldName).AppendLine();
            builder.AppendLine("            }");
        }
    }

    private static void AppendButtonHandlers(StringBuilder builder, IReadOnlyList<UIBindData> bindings)
    {
        bool hasButton = false;
        for (int i = 0; i < bindings.Count; i++)
        {
            UIBindData binding = bindings[i];
            if (binding.BindType != UIBindType.Button)
            {
                continue;
            }

            hasButton = true;
            builder.AppendLine();
            builder.AppendFormat("        private void On{0}Click()", ToMethodSuffix(binding.NodeName)).AppendLine();
            builder.AppendLine("        {");
            builder.AppendLine("        }");
        }

        if (!hasButton)
        {
            builder.AppendLine();
        }
    }

    private static string GenerateBindExpression(UIBindData binding)
    {
        switch (binding.BindType)
        {
            case UIBindType.GameObject:
                return $"FindGameObject(\"{Escape(binding.RelativePath)}\")";
            case UIBindType.Transform:
                return $"CachedTransform.Find(\"{Escape(binding.RelativePath)}\")";
            case UIBindType.Text:
                return $"FindComponent<TextMeshProUGUI>(\"{Escape(binding.RelativePath)}\")";
            case UIBindType.OldText:
                return $"FindComponent<Text>(\"{Escape(binding.RelativePath)}\")";
            case UIBindType.Image:
                return $"FindComponent<Image>(\"{Escape(binding.RelativePath)}\")";
            case UIBindType.RawImage:
                return $"FindComponent<RawImage>(\"{Escape(binding.RelativePath)}\")";
            case UIBindType.Button:
                return $"FindComponent<Button>(\"{Escape(binding.RelativePath)}\")";
            case UIBindType.Item:
                return $"FindGameObject(\"{Escape(binding.RelativePath)}\")";
            default:
                return "null";
        }
    }

    private static string GetFieldTypeName(UIBindData binding)
    {
        switch (binding.BindType)
        {
            case UIBindType.GameObject:
                return "GameObject";
            case UIBindType.Transform:
                return "Transform";
            case UIBindType.Text:
                return "TextMeshProUGUI";
            case UIBindType.OldText:
                return "Text";
            case UIBindType.Image:
                return "Image";
            case UIBindType.RawImage:
                return "RawImage";
            case UIBindType.Button:
                return "Button";
            case UIBindType.Item:
                return "GameObject";
            default:
                return "Component";
        }
    }

    private static string GetItemInstanceFieldName(UIBindData binding)
    {
        string fieldName = binding.FieldName;
        if (fieldName.EndsWith("Go", StringComparison.Ordinal))
        {
            fieldName = fieldName.Substring(0, fieldName.Length - 2);
        }

        return fieldName;
    }

    private static string ToMethodSuffix(string nodeName)
    {
        string fieldName = UIBindingScanner.ToMemberName(nodeName);
        if (fieldName.StartsWith("m_", StringComparison.Ordinal))
        {
            fieldName = fieldName.Substring(2);
        }

        if (fieldName.Length == 0)
        {
            return "Button";
        }

        return char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
    }

    private static string Escape(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string ResolveModuleName(UIWidget uiWidget)
    {
        string assetPath = ResolveWidgetAssetPath(uiWidget);

        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return string.Empty;
        }

        string folderPath = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return string.Empty;
        }

        folderPath = folderPath.Replace('\\', '/');
        return Path.GetFileName(folderPath);
    }

    private static string ResolveWidgetAssetPath(UIWidget uiWidget)
    {
        if (uiWidget == null)
        {
            return string.Empty;
        }

        string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(uiWidget.gameObject);
        if (!string.IsNullOrWhiteSpace(assetPath))
        {
            return assetPath;
        }

        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null && prefabStage.IsPartOfPrefabContents(uiWidget.gameObject))
        {
            return prefabStage.assetPath;
        }

        return AssetDatabase.GetAssetPath(uiWidget.gameObject);
    }

    private static bool AttachLogicScript(UIWidget uiWidget, string moduleName, string uiName, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (uiWidget == null)
        {
            errorMessage = "UIWidget is null.";
            return false;
        }

        string scriptAssetPath = $"{UIScriptRoot}/{moduleName}/{uiName}.cs";
        MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
        if (monoScript == null)
        {
            errorMessage = $"无法加载生成脚本：{scriptAssetPath}";
            return false;
        }

        Type scriptType = monoScript.GetClass();
        if (scriptType == null)
        {
            errorMessage = $"生成脚本尚未编译完成，请等待 Unity 编译后重试：{scriptAssetPath}";
            return false;
        }

        if (!typeof(Component).IsAssignableFrom(scriptType))
        {
            return true;
        }

        Component existingComponent = uiWidget.GetComponent(scriptType);
        if (existingComponent != null)
        {
            EditorUtility.SetDirty(uiWidget.gameObject);
            AssetDatabase.SaveAssets();
            return true;
        }

        Undo.AddComponent(uiWidget.gameObject, scriptType);
        EditorUtility.SetDirty(uiWidget.gameObject);
        AssetDatabase.SaveAssets();
        return true;
    }

    private static void AttachLogicScriptIfExists(UIWidget uiWidget, string moduleName, string uiName)
    {
        if (uiWidget == null || string.IsNullOrWhiteSpace(moduleName) || string.IsNullOrWhiteSpace(uiName))
        {
            return;
        }

        string scriptAssetPath = $"{UIScriptRoot}/{moduleName}/{uiName}.cs";
        MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
        if (monoScript == null)
        {
            return;
        }

        Type scriptType = monoScript.GetClass();
        if (scriptType == null || !typeof(Component).IsAssignableFrom(scriptType))
        {
            return;
        }

        if (uiWidget.GetComponent(scriptType) != null)
        {
            return;
        }

        Undo.AddComponent(uiWidget.gameObject, scriptType);
        EditorUtility.SetDirty(uiWidget.gameObject);
    }

    private static void EnsureDirectory(string assetDirectoryPath)
    {
        string[] parts = assetDirectoryPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static void WriteAllText(string assetPath, string content)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        string directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content, Encoding.UTF8);
    }
}
