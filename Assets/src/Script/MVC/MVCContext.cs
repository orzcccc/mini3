using System;
using System.Collections.Generic;

/// <summary>
/// MVC 上下文，负责统一注册、初始化和驱动 Model 与 Controller。
/// </summary>
public sealed class MVCContext
{
    private readonly Dictionary<Type, BaseModel> m_ModelByType = new Dictionary<Type, BaseModel>();
    private readonly Dictionary<Type, BaseController> m_ControllerByType = new Dictionary<Type, BaseController>();
    private readonly List<BaseModel> m_ModelList = new List<BaseModel>();
    private readonly List<BaseController> m_ControllerList = new List<BaseController>();

    public bool IsInitialized { get; private set; }

    public T RegisterModel<T>() where T : BaseModel, new()
    {
        Type type = typeof(T);
        if (m_ModelByType.TryGetValue(type, out BaseModel cachedModel))
        {
            return cachedModel as T;
        }

        T model = new T();
        model.Attach(this);
        m_ModelByType[type] = model;
        m_ModelList.Add(model);
        model.RegisterInternal();
        return model;
    }

    public T RegisterController<T>() where T : BaseController, new()
    {
        Type type = typeof(T);
        if (m_ControllerByType.TryGetValue(type, out BaseController cachedController))
        {
            return cachedController as T;
        }

        T controller = new T();
        controller.Attach(this);
        m_ControllerByType[type] = controller;
        m_ControllerList.Add(controller);
        controller.RegisterInternal();
        return controller;
    }

    public T GetModel<T>() where T : BaseModel
    {
        return m_ModelByType.TryGetValue(typeof(T), out BaseModel model) ? model as T : null;
    }

    public T GetController<T>() where T : BaseController
    {
        return m_ControllerByType.TryGetValue(typeof(T), out BaseController controller) ? controller as T : null;
    }

    public void Init()
    {
        if (IsInitialized)
        {
            return;
        }

        for (int i = 0; i < m_ModelList.Count; i++)
        {
            m_ModelList[i].InitInternal();
        }

        for (int i = 0; i < m_ControllerList.Count; i++)
        {
            m_ControllerList[i].InitInternal();
        }

        for (int i = 0; i < m_ModelList.Count; i++)
        {
            m_ModelList[i].ReadyInternal();
        }

        for (int i = 0; i < m_ControllerList.Count; i++)
        {
            m_ControllerList[i].ReadyInternal();
        }

        IsInitialized = true;
    }

    public void Update(float deltaTime)
    {
        if (!IsInitialized)
        {
            return;
        }

        for (int i = 0; i < m_ModelList.Count; i++)
        {
            m_ModelList[i].UpdateInternal(deltaTime);
        }

        for (int i = 0; i < m_ControllerList.Count; i++)
        {
            m_ControllerList[i].UpdateInternal(deltaTime);
        }
    }

    public void LateUpdate(float deltaTime)
    {
        if (!IsInitialized)
        {
            return;
        }

        for (int i = 0; i < m_ModelList.Count; i++)
        {
            m_ModelList[i].LateUpdateInternal(deltaTime);
        }

        for (int i = 0; i < m_ControllerList.Count; i++)
        {
            m_ControllerList[i].LateUpdateInternal(deltaTime);
        }
    }

    public void Shutdown()
    {
        for (int i = m_ControllerList.Count - 1; i >= 0; i--)
        {
            m_ControllerList[i].ShutdownInternal();
        }

        for (int i = m_ModelList.Count - 1; i >= 0; i--)
        {
            m_ModelList[i].ShutdownInternal();
        }

        m_ControllerByType.Clear();
        m_ModelByType.Clear();
        m_ControllerList.Clear();
        m_ModelList.Clear();
        IsInitialized = false;
    }
}
