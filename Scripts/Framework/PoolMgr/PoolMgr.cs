using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �������
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
    /// �ӻ������ͬ����ȡ����
    /// </summary>
    /// <param name="resPath">Resources�ļ����е���Դ·��</param>
    /// <param name="empty">�Ƿ����һ���ն���</param>
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
    /// �ӻ�������첽��ȡ����
    /// </summary>
    /// <param name="resPath">Resources�ļ����е���Դ·��</param>
    /// <param name="empty">�Ƿ񴴽�һ���ն��� �����Resources�����</param>
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
                // ���¶�������Ϊ������ص����� ����黹
                obj.name = resPath;
                if (callback != null)
                    callback(obj);
            });
        }
    }
    /// <summary>
    /// �ӻ�������첽��ȡ���� �����øö���λ������ת��Ϣ
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
    /// �򻺴�ع黹����
    /// </summary>
    /// <param name="resPath">Resources�ļ����е���Դ·��</param>
    /// <param name="obj">���黹������</param>
    public void Store(string resPath, GameObject obj)
    {
        if (!pools.ContainsKey(resPath))
        {
            // ��������غͳ����ϵĸ�����
            GameObject parent = new GameObject(resPath);
            parent.transform.parent = poolParent.transform;
            pools.Add(resPath, new Pool(parent.transform));
        }
        pools[resPath].Push(obj);
    }

    /// <summary>
    /// ���������е�ȫ������
    /// </summary>
    public void Clear()
    {
        foreach (Pool pool in pools.Values)
            GameObject.Destroy(pool.parent);
        pools.Clear();
    }
    /// <summary>
    /// �������������е�����
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
            // ���洢��Ķ���ʧ��
            obj.SetActive(false);
            obj.transform.parent = parent;
            objs.Push(obj);
        }
        public GameObject Pop()
        {
            GameObject ret = objs.Pop();
            // ����ȡ���Ķ���
            ret.SetActive(true);
            return ret;
        }
    }
}