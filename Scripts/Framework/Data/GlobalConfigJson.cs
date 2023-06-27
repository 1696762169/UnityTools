using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiniExcelLibs;

public abstract class GlobalConfigJson<T> where T : GlobalConfigJson<T>, new()
{
    [NonSerializedField]
    public static T Instance { get; protected set; } = new T();
    [NonSerializedField]
    public abstract string FilePath { get; }

    private bool inited = false;

    public void InitInstance()
    {
        if (inited)
            return;
        inited = true;

        try
        {
            if (FilePath.EndsWith("json"))
            {
                Instance = JsonMapper.ToObject<T>(File.ReadAllText(FilePath));
            }
            else
            {
                throw new System.ArgumentException($"全局配置文件【{FilePath}】不是json文件");
            }
        }
        catch
        {
            throw new FileNotFoundException($"无法读取全局配置文件：{FilePath}");
        }
        Instance.InitExtend();
    }
    protected virtual void InitExtend() { }
}
