using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Mono统一管理类
/// 可以使得没有继承Mono的类也能够进行统一的Update 以及开启协程
/// </summary>
public class MonoMgr : SingletonBase<MonoMgr>
{
    private readonly MonoController m_Controller;
    public MonoMgr()
    {
        GameObject obj = new GameObject("MonoController");
        m_Controller = obj.AddComponent<MonoController>();
    }

    /* 订阅/取消订阅Controller的事件 */
    public void AddUpdateListener(UnityAction update) => m_Controller.UpdateEvent += update;
    public void RemoveUpdateListener(UnityAction update) => m_Controller.UpdateEvent -= update;

    /* 使用controller间接使用协程 */
    public Coroutine StartCoroutine(string routine) => m_Controller.StartCoroutine(routine);
    public Coroutine StartCoroutine(IEnumerator routine) => m_Controller.StartCoroutine(routine);
    public void StopAllCoroutines() => m_Controller.StopAllCoroutines();
    public void StopCoroutine(IEnumerator routine) => m_Controller.StopCoroutine(routine);
    public void StopCoroutine(Coroutine routine) => m_Controller.StopCoroutine(routine);
    public void StopCoroutine(string routine) => m_Controller.StopCoroutine(routine);

    /// <summary>
    /// 继承Mono的类 能够调用生命周期函数
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Local
    private class MonoController : MonoBehaviour
    {
        public event UnityAction UpdateEvent;

        // 作为单例模式的对象 切换场景时也不移除
        protected void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        // Update时调用所有的Update函数
        protected void Update()
        {
	        UpdateEvent?.Invoke();
        }
    }
}
