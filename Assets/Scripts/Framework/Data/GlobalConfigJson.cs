using System;
using LitJson;
using System.IO;
using UnityEngine;

/// <summary>
/// Json格式的全局配置类
/// </summary>
public class GlobalConfigJson<T> : IDisposable where T : GlobalConfigJson<T>, new()
{
	public static T Instance
	{
		get
		{
			if (m_Instance != null)
				return m_Instance;
			m_Instance = new T().InitInstance();
			return m_Instance;
		}
	}
	private static T m_Instance;

    [NonSerializeJson]
    public virtual string FileName => typeof(T).Name;
    [NonSerializeJson]
    public virtual string FileDir => "";
    [NonSerializeJson]
#if UNITY_ANDROID && !UNITY_EDITOR
    public string FilePath => Path.Combine(AndroidConfigInit.ConfigRoot, $"{FileDir}{FileName}.json");
#else
    public string FilePath => $"{Application.streamingAssetsPath}/{FileDir}{FileName}.json";
#endif

	public T InitInstance()
	{
		try
		{
			T instance = JsonMapper.ToObject<T>(File.ReadAllText(FilePath));
			instance.InitExtend();
			return instance;
		}
		catch
		{
			if (!File.Exists(FilePath))
			{
				// 创建文件
				T instance = new T();
				instance.InitExtend();
				File.WriteAllText(FilePath, JsonMapper.ToJson(instance));
				Debug.LogWarning($"全局配置文件【{FileName}】不存在，已新建文件");
				return instance;
			}
			else
			{
				throw new FileNotFoundException($"无法读取全局配置文件：{FileName}");
			}
		}
	}

#if UNITY_EDITOR
	public virtual void SaveDataEditor() => File.WriteAllText(FilePath, JsonMapper.ToJson(this));
#endif

    protected virtual void InitExtend() { }

    public virtual void Dispose()
    {
	    m_Instance = null;
    }
}