using UnityEngine;
using UnityEngine.UI;

namespace Mini3.UI.Battle
{
    /// <summary>
    /// BattleMainView 的样例生成脚本，实际项目里可由 UIWidget 工具覆盖生成。
    /// </summary>
    public sealed class BattleMainView : BaseUI
    {
        private Text m_titleTxt;
        private Image m_iconImg;
        private Button m_closeBtn;

        protected override void BindComponents()
        {
            base.BindComponents();
            m_titleTxt = FindComponent<Text>("Root/TitleTxt");
            m_iconImg = FindComponent<Image>("Root/IconImg");
            m_closeBtn = FindComponent<Button>("Root/CloseBtn");
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

        protected override void OnOpenUI(object userData)
        {
            base.OnOpenUI(userData);
        }

        protected override void RefreshView()
        {
            base.RefreshView();
            if (m_titleTxt != null)
            {
                m_titleTxt.text = "BattleMainView";
            }
        }

        private void OnCloseBtnClick()
        {
            CloseSelf();
        }
    }
}
