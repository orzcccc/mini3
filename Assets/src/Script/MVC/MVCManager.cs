/// <summary>
/// MVC 统一入口，负责持有上下文并暴露统一访问点。
/// </summary>
public sealed class MVCManager : Singleton<MVCManager>
{
    public MVCContext Context { get; private set; }

    protected override void OnInit()
    {
        base.OnInit();
        Context = new MVCContext();
    }

    public T RegisterModel<T>() where T : BaseModel, new()
    {
        return Context.RegisterModel<T>();
    }

    public T RegisterController<T>() where T : BaseController, new()
    {
        return Context.RegisterController<T>();
    }

    public T GetModel<T>() where T : BaseModel
    {
        return Context.GetModel<T>();
    }

    public T GetController<T>() where T : BaseController
    {
        return Context.GetController<T>();
    }

    public void Init()
    {
        Context.Init();
    }

    public void Update(float deltaTime)
    {
        Context.Update(deltaTime);
    }

    public void LateUpdate(float deltaTime)
    {
        Context.LateUpdate(deltaTime);
    }

    public void Shutdown()
    {
        Context.Shutdown();
    }
}
