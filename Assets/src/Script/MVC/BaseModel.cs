using UnityEngine;

/// <summary>
/// Model 基类，统一封装注册、初始化、更新和销毁生命周期。
/// </summary>
public abstract class BaseModel
{
    public MVCContext Context { get; private set; }

    internal void Attach(MVCContext context)
    {
        Context = context;
    }

    internal void RegisterInternal()
    {
        OnRegister();
    }

    internal void InitInternal()
    {
        OnInit();
    }

    internal void ReadyInternal()
    {
        OnReady();
    }

    internal void UpdateInternal(float deltaTime)
    {
        OnUpdate(deltaTime);
    }

    internal void LateUpdateInternal(float deltaTime)
    {
        OnLateUpdate(deltaTime);
    }

    internal void ShutdownInternal()
    {
        OnShutdown();
    }

    protected T GetModel<T>() where T : BaseModel
    {
        return Context != null ? Context.GetModel<T>() : null;
    }

    protected T GetController<T>() where T : BaseController
    {
        return Context != null ? Context.GetController<T>() : null;
    }

    protected void LogLifecycle(string stage)
    {
    }

    protected virtual void OnRegister()
    {
    }

    protected virtual void OnInit()
    {
    }

    protected virtual void OnReady()
    {
    }

    protected virtual void OnUpdate(float deltaTime)
    {
    }

    protected virtual void OnLateUpdate(float deltaTime)
    {
    }

    protected virtual void OnShutdown()
    {
    }
}
