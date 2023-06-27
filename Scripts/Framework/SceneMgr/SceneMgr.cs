using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换模块
/// 用于在切换场景时进行一些加载
/// </summary>
public class SceneMgr : SingletonBase<SceneMgr>
{
    /* 同步加载场景 */
    public void LoadScene(int sceneBuildID, UnityAction action)
    {
        SceneManager.LoadScene(sceneBuildID);
        action();
    }
    public void LoadScene(string scene, UnityAction action)
    {
        LoadScene(SceneManager.GetSceneByName(scene).buildIndex,action);
    }
    
    /* 异步加载场景 */
    public void LoadSceneAsync(int sceneBuildID, UnityAction action)
    {
        MonoMgr.Instance.StartCoroutine(LoadCoroutine(sceneBuildID, action));
    }
    public void LoadSceneAsync(string scene, UnityAction action)
    {
        LoadSceneAsync(SceneManager.GetSceneByName(scene).buildIndex,action);
    }

    // 异步加载时使用的协程函数
    private IEnumerator LoadCoroutine(int sceneBuildID, UnityAction action)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneBuildID);
        while (!ao.isDone)
        {
            // 产生Loading事件
            EventMgr.Instance.TriggerEvent("SceneLoading", ao.progress);
            yield return ao.progress;
        }
        action();
    }
}
