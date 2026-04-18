using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mini3.UI.Common
{
    public sealed class MainCardView : BaseUI
    {
        private GameObject m_commonBackBoxItemGo;
        private CommonBackBoxItem m_commonBackBoxItem;

        protected override void BindComponents()
        {
            base.BindComponents();
            m_commonBackBoxItemGo = FindGameObject("CommonBackBoxItem");
            m_commonBackBoxItem = m_commonBackBoxItemGo != null ? new CommonBackBoxItem(m_commonBackBoxItemGo) : null;
        }

        protected override void BindEvents()
        {
            base.BindEvents();
        }

        protected override void UnbindEvents()
        {
            if (m_commonBackBoxItem != null)
            {
                m_commonBackBoxItem.Dispose();
                m_commonBackBoxItem = null;
            }
            base.UnbindEvents();
        }


        protected override void Init(object userData)
        {
            base.Init(userData);
        }

        protected override void AfterOpenView(object userData)
        {
            base.AfterOpenView(userData);
        }

        protected override void BeforeCloseView(object userData)
        {
            base.BeforeCloseView(userData);
        }

        protected override void Destroy()
        {
            base.Destroy();
        }

    }
}
