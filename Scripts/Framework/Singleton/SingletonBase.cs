using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBase<T> where T : SingletonBase<T>, new()
{
    protected static T instance = new T();
    public static T Instance => instance;
}
