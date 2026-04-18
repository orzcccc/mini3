using UnityEngine;
using UnityGameFramework.Runtime;

namespace Mini3
{
    /// <summary>
    /// 业务 UI 基类，统一封装 UGF 的窗体生命周期。
    /// </summary>
    public abstract class BaseUI : UIFormLogic
    {
        private bool m_IsBound;
        private object m_OpenUserData;
        private bool m_IsDestroyed;

        public string UIName => UIForm != null ? UIForm.UIFormAssetName : gameObject.name;

        public int SerialId => UIForm != null ? UIForm.SerialId : 0;

        public object OpenUserData => m_OpenUserData;

        public virtual UILayerName LayerName => UILayerName.MainLayer;

        protected sealed override void OnInit(object userData)
        {
            base.OnInit(userData);
            BindComponents();
            m_IsBound = true;
            Init(userData);
        }

        protected sealed override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_OpenUserData = userData;
            if (!m_IsBound)
            {
                BindComponents();
                m_IsBound = true;
            }

            BindEvents();
            AfterOpenView(userData);
        }

        protected sealed override void OnClose(bool isShutdown, object userData)
        {
            BeforeCloseView(userData);
            UnbindEvents();
            m_OpenUserData = null;
            base.OnClose(isShutdown, userData);
        }

        protected sealed override void OnRecycle()
        {
            if (!m_IsDestroyed)
            {
                m_IsDestroyed = true;
                Destroy();
            }

            base.OnRecycle();
        }

        protected virtual void BindComponents()
        {
        }

        protected virtual void BindEvents()
        {
        }

        protected virtual void UnbindEvents()
        {
        }

        protected virtual void Init(object userData)
        {
        }

        protected virtual void AfterOpenView(object userData)
        {
        }

        protected virtual void BeforeCloseView(object userData)
        {
        }

        protected virtual void Destroy()
        {
        }

        protected T FindComponent<T>(string relativePath) where T : Component
        {
            Transform target = CachedTransform.Find(relativePath);
            if (target == null)
            {
                Debug.LogWarning($"BaseUI.FindComponent failed, path = {relativePath}, ui = {UIName}");
                return null;
            }

            return target.GetComponent<T>();
        }

        protected GameObject FindGameObject(string relativePath)
        {
            Transform target = CachedTransform.Find(relativePath);
            return target != null ? target.gameObject : null;
        }

        protected void CloseSelf(object userData = null)
        {
            UIMgr.inst.Close(SerialId, userData);
        }
    }
}
