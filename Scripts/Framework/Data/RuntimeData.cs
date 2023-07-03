using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class RuntimeData<T> where T : RuntimeData<T>, new()
{
    private static T instance = new T();
    public static T Instance => instance;
    private bool inited = false;

    /// <summary>
    /// 文件路径
    /// </summary>
    [NonSerializeJson]
    public virtual string FilePath { get; } = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitInstance()
    {
        if (inited)
            return;
        inited = true;
#if UNITY_EDITOR
        if (GameManager.Instance.test)
        {
            if (InitTestData())
            {
                instance.InitExtend();
                return;
            }
        }
#endif
        InitData();
        if (File.Exists(FilePath))
            LoadData();
        else
            SaveData();
        instance.InitExtend();
    }
    /// <summary>
    /// 保存数据
    /// </summary>
    public virtual void SaveData()
    {
#if UNITY_EDITOR
        if (!GameManager.Instance.Initializing && GameManager.Instance.test && !GameManager.Instance.saveTestData)
        {
            return;
        }
#endif
        File.WriteAllText(FilePath, JsonMapper.ToJson(this));
    }
    /// <summary>
    /// 加载数据
    /// </summary>
    protected virtual void LoadData()
    {
        try
        {
            instance = JsonMapper.ToObject<T>(File.ReadAllText(FilePath));
        }
        catch
        {
            SaveData();
        }
    }
    protected virtual void InitExtend() { }
    /// <summary>
    /// 初始化/重置数据
    /// </summary>
    public abstract void InitData();
    /// <summary>
    /// 初始化用于测试的数据
    /// </summary>
    /// <returns>是否真的使用了测试数据</returns>
    protected virtual bool InitTestData()
    {
        return false;
    }
}
