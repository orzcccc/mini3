using UnityEngine;

namespace Mini3
{
    /// <summary>
    /// 可复用 UI 组件基类，用于 Item 级别的节点封装。
    /// </summary>
    public abstract class BaseItem
    {
        protected BaseItem(GameObject root)
        {
            Root = root;
            CachedTransform = root != null ? root.transform : null;
            BindComponents();
            BindEvents();
        }

        protected GameObject Root { get; }

        protected Transform CachedTransform { get; }

        protected virtual void BindComponents()
        {
        }

        protected virtual void BindEvents()
        {
        }

        protected virtual void UnbindEvents()
        {
        }

        protected virtual void RefreshView()
        {
        }

        public virtual void Dispose()
        {
            UnbindEvents();
        }

        protected T FindComponent<T>(string relativePath) where T : Component
        {
            if (CachedTransform == null)
            {
                return null;
            }

            Transform target = CachedTransform.Find(relativePath);
            if (target == null)
            {
                Debug.LogWarning($"BaseItem.FindComponent failed, path = {relativePath}, root = {Root?.name}");
                return null;
            }

            return target.GetComponent<T>();
        }

        protected GameObject FindGameObject(string relativePath)
        {
            if (CachedTransform == null)
            {
                return null;
            }

            Transform target = CachedTransform.Find(relativePath);
            return target != null ? target.gameObject : null;
        }

        protected void Refresh()
        {
            RefreshView();
        }
    }
}
