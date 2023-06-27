using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 缓存池类
/// </summary>
public class PoolMgr : SingletonBase<PoolMgr>
{
    private GameObject poolParent;
    private Dictionary<string, Pool> pools = new Dictionary<string, Pool>();
    public PoolMgr()
    {
        poolParent = new GameObject("Pool");
        GameObject.DontDestroyOnLoad(poolParent);
    }

    /// <summary>
    /// 从缓存池中同步获取对象
    /// </summary>
    /// <param name="resPath">Resources文件夹中的资源路径</param>
    /// <param name="empty">是否加载一个空对象</param>
    public GameObject Fetch(string resPath, bool empty = false)
    {
        if (pools.ContainsKey(resPath) && pools[resPath].Count > 0)
            return pools[resPath].Pop();
        else if (empty)
            return new GameObject(resPath);
        else
        {
            GameObject ret = ResMgr.Instance.Load<GameObject>(resPath);
            ret.name = resPath;
            return ret;
        }
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
    /// <param name="empty">是否创建一个空对象 无需从Resources里加载</param>
    public void FetchAsync(string resPath, UnityAction<GameObject> callback = null)
    {
        if (pools.ContainsKey(resPath) && pools[resPath].Count > 0)
        {
            if (callback != null)
                callback(pools[resPath].Pop());
            else
                pools[resPath].Pop();
        }
        else
        {
            ResMgr.Instance.LoadAsync<GameObject>(resPath, (obj) =>
            {
                // 将新对象名该为本缓存池的名字 方便归还
                obj.name = resPath;
                if (callback != null)
                    callback(obj);
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
        if (!pools.ContainsKey(resPath))
        {
            // 创建缓存池和场景上的父对象
            GameObject parent = new GameObject(resPath);
            parent.transform.parent = poolParent.transform;
            pools.Add(resPath, new Pool(parent.transform));
        }
        pools[resPath].Push(obj);
    }

    /// <summary>
    /// 清除缓存池中的全部内容
    /// </summary>
    public void Clear()
    {
        foreach (Pool pool in pools.Values)
            GameObject.Destroy(pool.parent);
        pools.Clear();
    }
    /// <summary>
    /// 清除单个缓存池中的内容
    /// </summary>
    public void Clear(string resPath)
    {
        if (pools.ContainsKey(resPath))
        {
            GameObject.Destroy(pools[resPath].parent);
            pools.Remove(resPath);
        }
    }

    private class Pool
    {
        private Stack<GameObject> objs;
        public int Count => objs.Count;
        public Transform parent;

        public Pool(Transform parent)
        {
            this.parent = parent;
            objs = new Stack<GameObject>();
        }
        public void Push(GameObject obj)
        {
            // 将存储后的对象失活
            obj.SetActive(false);
            obj.transform.parent = parent;
            objs.Push(obj);
        }
        public GameObject Pop()
        {
            GameObject ret = objs.Pop();
            // 激活取出的对象
            ret.SetActive(true);
            return ret;
        }
    }
}