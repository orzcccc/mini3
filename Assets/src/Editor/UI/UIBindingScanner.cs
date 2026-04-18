using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 扫描 UIWidget 根节点下的命名规范节点，并生成绑定元数据。
/// </summary>
public static class UIBindingScanner
{
    public static bool IsViewName(string uiName)
    {
        return !string.IsNullOrWhiteSpace(uiName) && uiName.EndsWith("View", StringComparison.Ordinal);
    }

    public static bool IsItemName(string uiName)
    {
        return !string.IsNullOrWhiteSpace(uiName) && uiName.EndsWith("Item", StringComparison.Ordinal);
    }

    public static bool TryScan(UIWidget uiWidget, out List<UIBindData> bindings, out string errorMessage)
    {
        bindings = new List<UIBindData>();
        errorMessage = string.Empty;

        if (uiWidget == null)
        {
            errorMessage = "UIWidget is null.";
            return false;
        }

        if (!IsViewName(uiWidget.UIName) && !IsItemName(uiWidget.UIName))
        {
            errorMessage = $"UIWidget 名称必须以 View 或 Item 结尾，当前名称：{uiWidget.UIName}";
            return false;
        }

        Transform root = uiWidget.transform;
        List<Transform> children = new List<Transform>();
        CollectChildren(root, children);

        HashSet<string> fieldNames = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < children.Count; i++)
        {
            Transform child = children[i];
            if (!TryCreateBindData(root, child, out UIBindData bindData))
            {
                continue;
            }

            if (!fieldNames.Add(bindData.FieldName))
            {
                errorMessage = $"UI 字段名重复：{bindData.FieldName}";
                return false;
            }

            bindings.Add(bindData);
        }

        bindings.Sort((left, right) => string.CompareOrdinal(left.RelativePath, right.RelativePath));
        return true;
    }

    public static string ToMemberName(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return "m_Node";
        }

        StringBuilder builder = new StringBuilder(nodeName.Length + 4);
        bool upperNext = false;
        for (int i = 0; i < nodeName.Length; i++)
        {
            char current = nodeName[i];
            if (!char.IsLetterOrDigit(current))
            {
                upperNext = true;
                continue;
            }

            if (builder.Length == 0)
            {
                if (char.IsDigit(current))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(current));
                continue;
            }

            if (upperNext)
            {
                builder.Append(char.ToUpperInvariant(current));
                upperNext = false;
            }
            else
            {
                builder.Append(current);
            }
        }

        if (builder.Length == 0)
        {
            builder.Append("node");
        }

        return $"m_{builder}";
    }

    public static string GetFieldName(string nodeName, UIBindType bindType)
    {
        string fieldName = ToMemberName(nodeName);
        if (bindType == UIBindType.Item && !fieldName.EndsWith("Go", StringComparison.Ordinal))
        {
            return $"{fieldName}Go";
        }

        return fieldName;
    }

    private static void CollectChildren(Transform root, ICollection<Transform> results)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            results.Add(child);
            if (!ShouldSkipChildScan(child))
            {
                CollectChildren(child, results);
            }
        }
    }

    private static bool ShouldSkipChildScan(Transform target)
    {
        return target != null
            && target.name.EndsWith("Item", StringComparison.Ordinal)
            && target.GetComponent<UIWidget>() != null;
    }

    private static bool TryCreateBindData(Transform root, Transform target, out UIBindData bindData)
    {
        bindData = null;
        if (!TryResolveBindType(target, out UIBindType bindType, out string componentTypeName))
        {
            return false;
        }

        bindData = new UIBindData
        {
            NodeName = target.name,
            RelativePath = GetRelativePath(root, target),
            FieldName = GetFieldName(target.name, bindType),
            BindType = bindType,
            ComponentTypeName = componentTypeName,
            WidgetTypeName = bindType == UIBindType.Item ? target.name : string.Empty
        };

        return true;
    }

    private static bool TryResolveBindType(Transform target, out UIBindType bindType, out string componentTypeName)
    {
        bindType = UIBindType.Unknown;
        componentTypeName = string.Empty;

        string nodeName = target.name;
        if (nodeName.EndsWith("OldTxt", StringComparison.Ordinal))
        {
            Text text = target.GetComponent<Text>();
            if (text == null)
            {
                return false;
            }

            bindType = UIBindType.OldText;
            componentTypeName = typeof(Text).FullName;
            return true;
        }

        if (nodeName.EndsWith("Txt", StringComparison.Ordinal))
        {
            TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                return false;
            }

            bindType = UIBindType.Text;
            componentTypeName = typeof(TextMeshProUGUI).FullName;
            return true;
        }

        if (nodeName.EndsWith("Img", StringComparison.Ordinal))
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                return false;
            }

            bindType = UIBindType.Image;
            componentTypeName = typeof(Image).FullName;
            return true;
        }

        if (nodeName.EndsWith("RawImg", StringComparison.Ordinal))
        {
            RawImage rawImage = target.GetComponent<RawImage>();
            if (rawImage == null)
            {
                return false;
            }

            bindType = UIBindType.RawImage;
            componentTypeName = typeof(RawImage).FullName;
            return true;
        }

        if (nodeName.EndsWith("Btn", StringComparison.Ordinal))
        {
            Button button = target.GetComponent<Button>();
            if (button == null)
            {
                return false;
            }

            bindType = UIBindType.Button;
            componentTypeName = typeof(Button).FullName;
            return true;
        }

        if (nodeName.EndsWith("Item", StringComparison.Ordinal))
        {
            UIWidget childWidget = target.GetComponent<UIWidget>();
            if (childWidget == null)
            {
                return false;
            }

            bindType = UIBindType.Item;
            componentTypeName = typeof(GameObject).FullName;
            return true;
        }

        if (nodeName.EndsWith("Go", StringComparison.Ordinal))
        {
            bindType = UIBindType.GameObject;
            componentTypeName = typeof(GameObject).FullName;
            return true;
        }

        if (nodeName.EndsWith("Trans", StringComparison.Ordinal))
        {
            bindType = UIBindType.Transform;
            componentTypeName = typeof(Transform).FullName;
            return true;
        }

        return false;
    }

    private static string GetRelativePath(Transform root, Transform target)
    {
        if (target == root)
        {
            return string.Empty;
        }

        List<string> segments = new List<string>();
        Transform current = target;
        while (current != null && current != root)
        {
            segments.Add(current.name);
            current = current.parent;
        }

        segments.Reverse();
        return string.Join("/", segments);
    }
}
