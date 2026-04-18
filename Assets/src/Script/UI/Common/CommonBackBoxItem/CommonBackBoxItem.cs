using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mini3.UI.Common
{
    public sealed class CommonBackBoxItem : BaseItem
    {
        private Image m_bgImg;
        private Image m_titleImg;
        private TextMeshProUGUI m_titleTxt;

        public CommonBackBoxItem(GameObject root) : base(root)
        {
        }

        protected override void BindComponents()
        {
            base.BindComponents();
            m_bgImg = FindComponent<Image>("bgImg");
            m_titleImg = FindComponent<Image>("titleImg");
            m_titleTxt = FindComponent<TextMeshProUGUI>("titleImg/titleTxt");
        }

        protected override void BindEvents()
        {
            base.BindEvents();
        }

        protected override void UnbindEvents()
        {
            base.UnbindEvents();
        }

        protected override void RefreshView()
        {
            base.RefreshView();
        }

    }
}
