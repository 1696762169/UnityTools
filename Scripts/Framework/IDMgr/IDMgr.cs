using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDMgr : SingletonBase<IDMgr>
{
    protected Dictionary<Type, int> m_IDs = new Dictionary<Type, int>();

    /// <summary>
    /// 初始化数组容器中的ID记录
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="collection">数组容器</param>
    public void InitID<T>(Array collection) where T : IUnique
    {
        Init<T>(collection);
    }
    /// <summary>
    /// 初始化泛型容器中的ID记录
    /// </summary>
    /// <typeparam name="T">泛型元素类型</typeparam>
    /// <param name="collection">泛型容器</param>
    public void InitID<T>(IEnumerable<T> collection) where T : IUnique
    {
        Init<T>(collection);
    }
    // 初始化ID记录
    protected void Init<T>(IEnumerable collection) where T : IUnique
    {
        int max = 1;
        foreach (T item in collection)
        {
            if (item.ID == int.MaxValue)
            {
                max = 1;
                break;
            }
            if (item.ID >= max)
            {
                max = item.ID + 1;
            }
        }
        m_IDs[typeof(T)] = max;
    }

    /// <summary>
    /// 获取一个新ID
    /// </summary>
    /// <typeparam name="T">需要ID的类型</typeparam>
    public int GetNewID<T>() where T : IUnique
    {
        if (!m_IDs.ContainsKey(typeof(T)) || m_IDs[typeof(T)] == int.MaxValue)
            m_IDs[typeof(T)] = 1;
        return m_IDs[typeof(T)]++;
    }
}
