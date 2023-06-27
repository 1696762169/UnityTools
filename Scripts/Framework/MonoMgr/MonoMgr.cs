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
    private MonoController controller;
    public MonoMgr()
    {
        GameObject obj = new GameObject("MonoController");
        controller = obj.AddComponent<MonoController>();
    }

    /* 订阅/取消订阅Controller的事件 */
    public void AddUpdateListener(UnityAction update) => controller.updateEvent += update;
    public void RemoveUpdateListener(UnityAction update) => controller.updateEvent += update;

    /* 使用controller间接使用协程 */
    public Coroutine StartCoroutine(string routine) => controller.StartCoroutine(routine);
    public Coroutine StartCoroutine(IEnumerator routine) => controller.StartCoroutine(routine);
    public void StopAllCoroutines() => controller.StopAllCoroutines();
    public void StopCoroutine(IEnumerator routine) => controller.StopCoroutine(routine);
    public void StopCoroutine(Coroutine routine) => controller.StopCoroutine(routine);
    public void StopCoroutine(string routine) => controller.StopCoroutine(routine);

    /// <summary>
    /// 继承Mono的类 能够调用生命周期函数
    /// </summary>
    private class MonoController : MonoBehaviour
    {
        public event UnityAction updateEvent;

        // 作为单例模式的对象 切换场景时也不移除
        protected void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        // Update时调用所有的Update函数
        protected void Update()
        {
            if (updateEvent != null)
                updateEvent();
        }
    }
}
