using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class GMView : BaseUI
{
    private GameObject m_commonBackBoxItemGo;
    private CommonBackBoxItem m_commonBackBoxItem;

    protected override void BindComponents()
    {
        base.BindComponents();
        m_commonBackBoxItemGo = FindGameObject("CommonBackBoxItem");
    }

    protected override void BindEvents()
    {
        base.BindEvents();
    }

    protected override void UnbindEvents()
    {
        base.UnbindEvents();
    }

    protected override void Init(object userData)
    {
        base.Init(userData);
        if (m_commonBackBoxItem == null && m_commonBackBoxItemGo != null)
        {
            m_commonBackBoxItem = new CommonBackBoxItem(m_commonBackBoxItemGo);
        }
    }

    protected override void AfterOpenView(object userData)
    {
        base.AfterOpenView(userData);
        if (m_commonBackBoxItem != null)
        {
            m_commonBackBoxItem.SetData("GM窗口");
        }
    }

    protected override void BeforeCloseView(object userData)
    {
        base.BeforeCloseView(userData);
    }

    protected override void Destroy()
    {
        if (m_commonBackBoxItem != null)
        {
            m_commonBackBoxItem.Dispose();
            m_commonBackBoxItem = null;
        }

        base.Destroy();
    }

}
