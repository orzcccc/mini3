using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UI 预设绑定描述组件，存放编辑器扫描出来的节点信息。
/// </summary>
[DisallowMultipleComponent]
public sealed class UIWidget : MonoBehaviour
{
    private static bool s_IsSyncingBindings;

    [SerializeField]
    private List<UIBindData> m_Bindings = new List<UIBindData>();

    public string ModuleName => ResolveModuleName();

    public string UIName => gameObject.name;

    public List<UIBindData> Bindings => m_Bindings;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || IsGeneratingResourceRegistry() || s_IsSyncingBindings)
        {
            return;
        }

        if (!IsBindableWidgetName(UIName))
        {
            return;
        }

        TryAutoAttachLogicScript();

        if (!TryScanBindings(out List<UIBindData> bindings))
        {
            return;
        }

        if (AreBindingsEqual(m_Bindings, bindings))
        {
            return;
        }

        s_IsSyncingBindings = true;
        try
        {
            m_Bindings.Clear();
            m_Bindings.AddRange(bindings);
            EditorUtility.SetDirty(this);
        }
        finally
        {
            s_IsSyncingBindings = false;
        }
    }

    private static bool AreBindingsEqual(IReadOnlyList<UIBindData> left, IReadOnlyList<UIBindData> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left == null || right == null || left.Count != right.Count)
        {
            return false;
        }

        for (int i = 0; i < left.Count; i++)
        {
            UIBindData leftItem = left[i];
            UIBindData rightItem = right[i];
            if (leftItem == null || rightItem == null)
            {
                if (leftItem != rightItem)
                {
                    return false;
                }

                continue;
            }

            if (leftItem.NodeName != rightItem.NodeName
                || leftItem.RelativePath != rightItem.RelativePath
                || leftItem.FieldName != rightItem.FieldName
                || leftItem.BindType != rightItem.BindType
                || leftItem.ComponentTypeName != rightItem.ComponentTypeName
                || leftItem.WidgetTypeName != rightItem.WidgetTypeName)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsGeneratingResourceRegistry()
    {
        PropertyInfo property = GetEditorType("UICodeGenerator")?.GetProperty("IsGeneratingResourceRegistry", BindingFlags.Public | BindingFlags.Static);
        return property != null && property.GetValue(null) is bool value && value;
    }

    private bool IsBindableWidgetName(string uiName)
    {
        Type scannerType = GetEditorType("UIBindingScanner");
        if (scannerType == null)
        {
            return false;
        }

        MethodInfo isViewMethod = scannerType.GetMethod("IsViewName", BindingFlags.Public | BindingFlags.Static);
        MethodInfo isItemMethod = scannerType.GetMethod("IsItemName", BindingFlags.Public | BindingFlags.Static);
        bool isView = isViewMethod != null && isViewMethod.Invoke(null, new object[] { uiName }) is bool viewResult && viewResult;
        bool isItem = isItemMethod != null && isItemMethod.Invoke(null, new object[] { uiName }) is bool itemResult && itemResult;
        return isView || isItem;
    }

    private bool TryScanBindings(out List<UIBindData> bindings)
    {
        bindings = null;
        Type scannerType = GetEditorType("UIBindingScanner");
        MethodInfo method = scannerType?.GetMethod("TryScan", BindingFlags.Public | BindingFlags.Static);
        if (method == null)
        {
            return false;
        }

        object[] parameters = { this, null, string.Empty };
        if (!(method.Invoke(null, parameters) is bool success) || !success)
        {
            return false;
        }

        bindings = parameters[1] as List<UIBindData>;
        return bindings != null;
    }

    private void TryAutoAttachLogicScript()
    {
        Type codeGeneratorType = GetEditorType("UICodeGenerator");
        MethodInfo method = codeGeneratorType?.GetMethod("AutoAttachLogicScriptIfExists", BindingFlags.Public | BindingFlags.Static);
        method?.Invoke(null, new object[] { this });
    }

    private string ResolveModuleName()
    {
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.name == "Common")
            {
                return "Common";
            }

            current = current.parent;
        }

        return "Common";
    }

    private static Type GetEditorType(string fullName)
    {
        foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(fullName, false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
#endif
}
