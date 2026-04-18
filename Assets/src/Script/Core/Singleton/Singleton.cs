using System;

/// <summary>
/// 纯 C# 泛型单例基类，适合 TableMgr、ModelMgr 这类非 MonoBehaviour 管理器。
/// </summary>
/// <typeparam name="T">单例类型。</typeparam>
public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static readonly Lazy<T> s_Instance = new Lazy<T>(CreateInstance);

    public static T Inst => s_Instance.Value;

    // 保留小写入口，兼容常见的 inst 调用习惯。
    public static T inst => s_Instance.Value;

    protected Singleton()
    {
    }

    protected virtual void OnInit()
    {
    }

    private static T CreateInstance()
    {
        T instance = new T();
        instance.OnInit();
        return instance;
    }
}
