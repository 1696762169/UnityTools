using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源加载器
/// 加载出的GameObject会直接实例化
/// </summary>
public class ResMgr : SingletonBase<ResMgr>
{
    // 同步加载
    public T Load<T>(string resPath) where T : Object
    {
        T res = Resources.Load<T>(resPath);
        if (res is GameObject)
            return GameObject.Instantiate(res) as T;
        else
            return res;
    }

    // 异步加载
    public void LoadAsync<T>(string resPath, UnityAction<T> callback) where T: Object
    {
        MonoMgr.Instance.StartCoroutine(LoadCoroutine<T>(resPath, callback));
    }
    private IEnumerator LoadCoroutine<T>(string resPath, UnityAction<T> callback) where T: Object
    {
        ResourceRequest rr = Resources.LoadAsync<T>(resPath);
        yield return rr;

        if (rr.asset is GameObject)
            callback(GameObject.Instantiate(rr.asset) as T);
        else
            callback(rr.asset as T);
    }
}
