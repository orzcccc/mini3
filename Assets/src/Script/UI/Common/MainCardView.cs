using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class MainCardView : BaseUI
{
    private GameObject m_commonBackBoxItemGo;
    private CommonBackBoxItem m_commonBackBoxItem;

    protected override void BindComponents()
    {
        base.BindComponents();
        m_commonBackBoxItemGo = FindGameObject("CommonBackBoxItem");
        m_commonBackBoxItem = m_commonBackBoxItemGo != null ? new CommonBackBoxItem(m_commonBackBoxItemGo) : null;
        Debug.Log($"MainCardView.BindComponents commonBackBoxItemGo = {m_commonBackBoxItemGo != null}, item = {m_commonBackBoxItem != null}");
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

    public override UILayerName LayerName => UILayerName.MiddleLayer;

    protected override void Init(object userData)
    {
        base.Init(userData);
        Debug.Log($"MainCardView.Init layer = {LayerName}, userData = {userData ?? "null"}");
    }

    protected override void AfterOpenView(object userData)
    {
        base.AfterOpenView(userData);
        Debug.Log($"MainCardView.AfterOpenView layer = {LayerName}, parent = {transform.parent?.name ?? "null"}");
        Debug.Log($"MainCardView.AfterOpenView itemReady = {m_commonBackBoxItem != null}");
        if (m_commonBackBoxItem != null)
        {
            m_commonBackBoxItem.SetData("背景框标题");
        }
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
