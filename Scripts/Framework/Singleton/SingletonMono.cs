using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    protected static T instance;
    public static T Instance => instance;
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
            Debug.LogError($"单例模式类{typeof(T).Name}脚本被挂载了多次，挂载对象名：{name}");
    }
}
