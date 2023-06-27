using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// �����л�ģ��
/// �������л�����ʱ����һЩ����
/// </summary>
public class SceneMgr : SingletonBase<SceneMgr>
{
    /* ͬ�����س��� */
    public void LoadScene(int sceneBuildID, UnityAction action)
    {
        SceneManager.LoadScene(sceneBuildID);
        action();
    }
    public void LoadScene(string scene, UnityAction action)
    {
        LoadScene(SceneManager.GetSceneByName(scene).buildIndex,action);
    }
    
    /* �첽���س��� */
    public void LoadSceneAsync(int sceneBuildID, UnityAction action)
    {
        MonoMgr.Instance.StartCoroutine(LoadCoroutine(sceneBuildID, action));
    }
    public void LoadSceneAsync(string scene, UnityAction action)
    {
        LoadSceneAsync(SceneManager.GetSceneByName(scene).buildIndex,action);
    }

    // �첽����ʱʹ�õ�Э�̺���
    private IEnumerator LoadCoroutine(int sceneBuildID, UnityAction action)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneBuildID);
        while (!ao.isDone)
        {
            // ����Loading�¼�
            EventMgr.Instance.TriggerEvent("SceneLoading", ao.progress);
            yield return ao.progress;
        }
        action();
    }
}
