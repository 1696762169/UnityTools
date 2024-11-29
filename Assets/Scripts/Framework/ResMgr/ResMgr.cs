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
        return res is GameObject ? Object.Instantiate(res) : res;
    }

    // 异步加载
    public void LoadAsync<T>(string resPath, UnityAction<T> callback) where T: Object
    {
        MonoMgr.Instance.StartCoroutine(LoadCoroutine(resPath, callback));
    }
    private IEnumerator LoadCoroutine<T>(string resPath, UnityAction<T> callback) where T: Object
    {
        ResourceRequest rr = Resources.LoadAsync<T>(resPath);
        yield return rr;
        callback((rr.asset is GameObject ? Object.Instantiate(rr.asset) : rr.asset) as T);
    }
}
