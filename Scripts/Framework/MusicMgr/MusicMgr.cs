using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 音频管理类
/// </summary>
public class MusicMgr : SingletonBase<MusicMgr>
{
    // 背景音乐大小
    public float MusicVolume
    {
        get => BGM.volume;
        set => BGM.volume = value;
    }
    // 音效大小
    public float SoundVolume
    {
        get => m_SoundVolume;
        set
        {
            m_SoundVolume = value;
            foreach (GameObject sound in m_SoundList)
                sound.GetComponent<AudioSource>().volume = value;
        }
    }
    private float m_SoundVolume;

    // 背景音乐对象
    private readonly AudioSource BGM;
    // 音效列表
    private readonly List<GameObject> m_SoundList = new();
    // 音效在缓存池里的名称
    private const string SOUND_POOL = "__Sound";

    public MusicMgr()
    {
        // 创建背景音乐对象
        GameObject obj = new GameObject("BGM");
        BGM = obj.AddComponent<AudioSource>();
        BGM.loop = true;
        Object.DontDestroyOnLoad(obj);

        // 检测音效对象是否播放完 向缓存池归还音效对象
        MonoMgr.Instance.AddUpdateListener(() =>
        {
            for (int i = m_SoundList.Count - 1; i >= 0; i--)
            {
                if (!m_SoundList[i].GetComponent<AudioSource>().isPlaying)
                {
                    PoolMgr.Instance.Store(SOUND_POOL, m_SoundList[i]);
                    m_SoundList.RemoveAt(i);
                }
            }
        });
    }

    #region 背景音乐方法
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="resPath">背景音乐名</param>
    public void PlayMusic(string resPath)
    {
        AudioClip clip = Resources.Load<AudioClip>(resPath);
        BGM.clip = clip;
        BGM.Play();
    }
    /// <summary>
    /// 播放背景音乐(异步加载音频)
    /// </summary>
    /// <param name="resPath">背景音乐名</param>
    public void PlayMusicAsync(string resPath)
    {
        ResMgr.Instance.LoadAsync<AudioClip>(resPath, (clip) =>
        {
            BGM.clip = clip;
            BGM.Play();
        });
    }
    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopMusic() => BGM.Stop();
    /// <summary>
    /// 暂停播放背景音乐
    /// </summary>
    public void PauseMusic() => BGM.Pause();
    #endregion

    #region 音效方法
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="resPath">音效资源路径</param>
    /// <param name="isLoop">是否循环播放</param>
    public GameObject PlaySound(string resPath, bool isLoop = false)
    {
        GameObject obj = ProcessSound(Resources.Load<AudioClip>(resPath), isLoop);
        return obj;
    }
    public GameObject PlaySound(string resPath, Vector3 position, bool isLoop = false)
    {
        GameObject obj = ProcessSound(Resources.Load<AudioClip>(resPath), isLoop);
        obj.transform.position = position;
        return obj;
    }

    /// <summary>
    /// 播放音效(异步加载音频)
    /// </summary>
    /// <param name="resPath">音效资源路径</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="callback">资源加载后的回调函数</param>
    public void PlaySoundAsync(string resPath, bool isLoop = false, UnityAction<GameObject> callback = null)
    {
        ResMgr.Instance.LoadAsync<AudioClip>(resPath, (clip) =>
        {
            GameObject obj = ProcessSound(clip, isLoop);
            // 对音效的其它处理
            callback?.Invoke(obj);
        });
    }
    public void PlaySoundAsync(string resPath, Vector3 position, bool isLoop = false, UnityAction<GameObject> callback = null)
    {
        callback += (obj) => obj.transform.position = position;
        PlaySoundAsync(resPath, isLoop, callback);
    }
    /// <summary>
    /// 停止播放音效 向缓存池归还音效组件
    /// </summary>
    /// <param name="sound">要停止播放的音效对象</param>
    public void StopSound(GameObject sound)
    {
	    if (!m_SoundList.Contains(sound))
		    return;
	    sound.GetComponent<AudioSource>().Stop();
	    m_SoundList.Remove(sound);
	    PoolMgr.Instance.Store(SOUND_POOL, sound);
    }

    protected GameObject ProcessSound(AudioClip clip, bool isLoop)
    {
        // 从缓存池中加载对象
        GameObject obj = PoolMgr.Instance.Fetch(SOUND_POOL, true);
        AudioSource sound = obj.GetOrAddComponent<AudioSource>();
        
        // 设置新的音效
        sound.clip = clip;
        sound.loop = isLoop;
        sound.volume = m_SoundVolume;
        sound.Play();
        m_SoundList.Add(obj);
        return obj;
    }
    #endregion
}
