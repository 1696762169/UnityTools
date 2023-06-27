using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Monoͳһ������
/// ����ʹ��û�м̳�Mono����Ҳ�ܹ�����ͳһ��Update �Լ�����Э��
/// </summary>
public class MonoMgr : SingletonBase<MonoMgr>
{
    private MonoController controller;
    public MonoMgr()
    {
        GameObject obj = new GameObject("MonoController");
        controller = obj.AddComponent<MonoController>();
    }

    /* ����/ȡ������Controller���¼� */
    public void AddUpdateListener(UnityAction update) => controller.updateEvent += update;
    public void RemoveUpdateListener(UnityAction update) => controller.updateEvent += update;

    /* ʹ��controller���ʹ��Э�� */
    public Coroutine StartCoroutine(string routine) => controller.StartCoroutine(routine);
    public Coroutine StartCoroutine(IEnumerator routine) => controller.StartCoroutine(routine);
    public void StopAllCoroutines() => controller.StopAllCoroutines();
    public void StopCoroutine(IEnumerator routine) => controller.StopCoroutine(routine);
    public void StopCoroutine(Coroutine routine) => controller.StopCoroutine(routine);
    public void StopCoroutine(string routine) => controller.StopCoroutine(routine);

    /// <summary>
    /// �̳�Mono���� �ܹ������������ں���
    /// </summary>
    private class MonoController : MonoBehaviour
    {
        public event UnityAction updateEvent;

        // ��Ϊ����ģʽ�Ķ��� �л�����ʱҲ���Ƴ�
        protected void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        // Updateʱ�������е�Update����
        protected void Update()
        {
            if (updateEvent != null)
                updateEvent();
        }
    }
}
