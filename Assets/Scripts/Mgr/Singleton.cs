//
// Singleton.cs
//

using UnityEngine;

/// <summary>
/// 单例基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    protected Singleton() { }

    protected static T _inst = new T();
    public static T Instance
    {
        get
        {
            if (null == _inst)
                _inst = new T();
            return _inst;
        }
    }
}

/// <summary>
/// 单例Mono基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoSingleton<T> where T : UnityEngine.Component
{
    protected MonoSingleton() { }

    protected static T _inst = null;
    public static T Instance
    {
        get
        {
            if (null == _inst)
                _inst = new GameObject(typeof(T).Name + "GO").AddComponent<T>();
            return _inst;
        }
    }
}
