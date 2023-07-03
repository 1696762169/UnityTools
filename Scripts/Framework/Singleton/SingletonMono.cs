using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局单例Mono对象基类
/// </summary>
public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    public static T Instance { get; protected set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
            Debug.LogError($"单例模式类{typeof(T).Name}脚本被挂载了多次，挂载对象名：{name}");
    }
}
