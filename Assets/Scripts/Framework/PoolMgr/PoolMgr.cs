using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 缓存池类
/// </summary>
public class PoolMgr : SingletonBase<PoolMgr>
{
    private readonly GameObject m_PoolParent;
    private readonly Dictionary<string, Pool> m_Pools = new();
    public PoolMgr()
    {
        m_PoolParent = new GameObject("Pool");
        Object.DontDestroyOnLoad(m_PoolParent);
    }

    /// <summary>
    /// 从缓存池中同步获取对象
    /// </summary>
    /// <param name="resPath">Resources文件夹中的资源路径</param>
    /// <param name="empty">是否加载一个空对象</param>
    public GameObject Fetch(string resPath, bool empty = false)
    {
	    if (m_Pools.ContainsKey(resPath) && m_Pools[resPath].Count > 0)
            return m_Pools[resPath].Pop();
	    if (empty)
		    return new GameObject(resPath);
	    GameObject ret = ResMgr.Instance.Load<GameObject>(resPath);
	    ret.name = resPath;
	    return ret;
    }
    public GameObject Fetch(string resPath, Vector3 position, Quaternion rotation, bool empty = false)
    {
        GameObject obj = Fetch(resPath, empty);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

	/// <summary>
	/// 从缓存池中异步获取对象
	/// </summary>
	/// <param name="resPath">Resources文件夹中的资源路径</param>
	/// <param name="callback">加载完毕后的处理</param>
	public void FetchAsync(string resPath, UnityAction<GameObject> callback = null)
    {
        if (m_Pools.ContainsKey(resPath) && m_Pools[resPath].Count > 0)
        {
	        GameObject obj = m_Pools[resPath].Pop();
	        callback?.Invoke(obj);
        }
        else
        {
            ResMgr.Instance.LoadAsync<GameObject>(resPath, (obj) =>
            {
                // 将新对象名该为本缓存池的名字 方便归还
                obj.name = resPath;
                callback?.Invoke(obj);
            });
        }
    }
    /// <summary>
    /// 从缓存池中异步获取对象 并设置该对象位置与旋转信息
    /// </summary>
    public void FetchAsync(string resPath, Vector3 position, Quaternion rotation, UnityAction<GameObject> callback = null)
    {
        callback += (obj) =>
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        };
        FetchAsync(resPath, callback);
    }

    /// <summary>
    /// 向缓存池归还对象
    /// </summary>
    /// <param name="resPath">Resources文件夹中的资源路径</param>
    /// <param name="obj">待归还的物体</param>
    public void Store(string resPath, GameObject obj)
    {
        if (!m_Pools.ContainsKey(resPath))
        {
            // 创建缓存池和场景上的父对象
            GameObject parent = new(resPath);
            parent.transform.SetParent(m_PoolParent.transform);
            m_Pools.Add(resPath, new Pool(parent.transform));
        }
        m_Pools[resPath].Push(obj);
    }

    /// <summary>
    /// 清除缓存池中的全部内容
    /// </summary>
    public void Clear()
    {
        foreach (string resPath in m_Pools.Keys)
            Clear(resPath);
    }
    /// <summary>
    /// 清除单个缓存池中的内容
    /// </summary>
    public void Clear(string resPath)
    {
	    if (!m_Pools.ContainsKey(resPath)) return;
	    Object.Destroy(m_Pools[resPath].parent.gameObject);
	    m_Pools.Remove(resPath);
    }

    private class Pool
    {
	    private readonly Stack<GameObject> m_Objs = new();
        public int Count => m_Objs.Count;
        public readonly Transform parent;

        public Pool(Transform parent)
        {
            this.parent = parent;
        }
        public void Push(GameObject obj)
        {
            // 将存储后的对象失活
            obj.SetActive(false);
            obj.transform.SetParent(parent);
            m_Objs.Push(obj);
        }
        public GameObject Pop()
        {
            GameObject ret = m_Objs.Pop();
            // 激活取出的对象
            ret.SetActive(true);
            return ret;
        }
    }
}