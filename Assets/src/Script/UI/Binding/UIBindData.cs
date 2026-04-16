using System;
using UnityEngine;

namespace Mini3
{
    /// <summary>
    /// 记录单个 UI 节点的绑定信息，供编辑器生成代码时使用。
    /// </summary>
    [Serializable]
    public sealed class UIBindData
    {
        [SerializeField]
        private string m_NodeName;

        [SerializeField]
        private string m_RelativePath;

        [SerializeField]
        private string m_FieldName;

        [SerializeField]
        private UIBindType m_BindType;

        [SerializeField]
        private string m_ComponentTypeName;

        [SerializeField]
        private string m_WidgetTypeName;

        public string NodeName
        {
            get => m_NodeName;
            set => m_NodeName = value;
        }

        public string RelativePath
        {
            get => m_RelativePath;
            set => m_RelativePath = value;
        }

        public string FieldName
        {
            get => m_FieldName;
            set => m_FieldName = value;
        }

        public UIBindType BindType
        {
            get => m_BindType;
            set => m_BindType = value;
        }

        public string ComponentTypeName
        {
            get => m_ComponentTypeName;
            set => m_ComponentTypeName = value;
        }

        public string WidgetTypeName
        {
            get => m_WidgetTypeName;
            set => m_WidgetTypeName = value;
        }
    }

    /// <summary>
    /// UI 节点绑定类型。
    /// </summary>
    public enum UIBindType
    {
        Unknown = 0,
        GameObject = 1,
        Transform = 2,
        Text = 3,
        Image = 4,
        RawImage = 5,
        Button = 6,
        Item = 7
    }
}
