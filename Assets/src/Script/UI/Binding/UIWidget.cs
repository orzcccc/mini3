using System.Collections.Generic;
using UnityEngine;

namespace Mini3
{
    /// <summary>
    /// UI 预设绑定描述组件，存放编辑器扫描出来的节点信息。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIWidget : MonoBehaviour
    {
        private string m_ModuleName = "Common";

        private string m_UIName = string.Empty;

        [SerializeField]
        private List<UIBindData> m_Bindings = new List<UIBindData>();

        public string ModuleName
        {
            get => m_ModuleName;
            set => m_ModuleName = value;
        }

        public string UIName
        {
            get => string.IsNullOrWhiteSpace(m_UIName) ? gameObject.name : m_UIName;
            set => m_UIName = value;
        }

        public List<UIBindData> Bindings => m_Bindings;
    }
}
