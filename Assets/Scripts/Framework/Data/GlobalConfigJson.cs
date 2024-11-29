using System;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiniExcelLibs;

/// <summary>
/// Json格式的全局配置类
/// </summary>
public abstract class GlobalConfigJson<T> : ControlledSingleton<GlobalConfigJson<T>>
	where T : GlobalConfigJson<T>, new()
{
    [NonSerializeJson]
    protected virtual string FileName => typeof(T).Name;
    [NonSerializeJson]
    protected virtual string FilePath => $"{Application.streamingAssetsPath}/{FileName}.json";

    public new virtual T InitInstance()
    {
        base.InitInstance();

        try
	    {
		    if (!FilePath.EndsWith("json"))
			    throw new ArgumentException($"全局配置文件【{FilePath}】不是json文件");
		    T instance;
		    if (!File.Exists(FilePath))
		    {
                instance = new T();
				File.WriteAllText(FilePath, JsonMapper.ToJson(instance));
				Debug.LogWarning($"全局配置文件【{FilePath}】不存在，已新建文件");
		    }
		    else
		    {
				instance = JsonMapper.ToObject<T>(File.ReadAllText(FilePath));
				instance.InitExtend();
			}
		    return instance;
	    }
        catch
        {
            HasInstance = false;
            throw new FileNotFoundException($"无法读取全局配置文件：{FilePath}");
        }
    }

    protected virtual void InitExtend() { }
}