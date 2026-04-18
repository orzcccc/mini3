using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityGameFramework.Runtime;

public sealed class CommonBackBoxItem : BaseItem
{
    private Image m_bgImg;
    private Button m_closeBtn;
    private Image m_titleImg;
    private TextMeshProUGUI m_titleTxt;
    private GameObject m_ParentUIRoot;
    private string m_Title;

    public CommonBackBoxItem(GameObject root, GameObject parentUIRoot = null) : base(root)
    {
        m_ParentUIRoot = parentUIRoot;
    }

    protected override void BindComponents()
    {
        base.BindComponents();
        m_bgImg = FindComponent<Image>("bgImg");
        m_closeBtn = FindComponent<Button>("closeBtn");
        m_titleImg = FindComponent<Image>("titleImg");
        m_titleTxt = FindComponent<TextMeshProUGUI>("titleImg/titleTxt");
    }

    protected override void BindEvents()
    {
        base.BindEvents();
        if (m_closeBtn != null)
        {
            m_closeBtn.onClick.AddListener(OnCloseBtnClick);
        }
    }

    protected override void UnbindEvents()
    {
        if (m_closeBtn != null)
        {
            m_closeBtn.onClick.RemoveListener(OnCloseBtnClick);
        }
        base.UnbindEvents();
    }

    protected override void RefreshView()
    {
        base.RefreshView();
        if (m_titleTxt != null)
        {
            m_titleTxt.text = string.IsNullOrEmpty(m_Title) ? string.Empty : m_Title;
        }
    }

    public void SetData(string title, GameObject parentUIRoot = null)
    {
        if (parentUIRoot != null)
        {
            m_ParentUIRoot = parentUIRoot;
        }

        m_Title = title;
        Refresh();
    }

    public override void SetData(object data)
    {
        SetData(data as string);
    }

    private void OnCloseBtnClick()
    {
        GameObject parentUIRoot = m_ParentUIRoot != null ? m_ParentUIRoot : Root;
        if (parentUIRoot == null)
        {
            return;
        }

        UIForm uiForm = parentUIRoot.GetComponentInParent<UIForm>();
        if (uiForm == null)
        {
            return;
        }

        UIMgr.inst.Close(uiForm.SerialId);
    }
}
