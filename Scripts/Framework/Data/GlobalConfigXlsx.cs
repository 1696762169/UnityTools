using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiniExcelLibs;
using System;

public abstract class GlobalConfigXlsx<T, TRaw> where T : GlobalConfigXlsx<T, TRaw>, new() where TRaw : class, new()
{
    [NonSerializeJson]
    public static T Instance { get; protected set; } = new T();
    [NonSerializeJson]
    public abstract string FilePath { get; }

    private bool inited = false;

    public void InitInstance()
    {
        if (inited)
            return;
        inited = true;

        try
        {
            if (FilePath.EndsWith("xlsx"))
            {
                foreach (TRaw raw in MiniExcel.Query<TRaw>(FilePath, startCell: "A3"))
                {
                    Instance = Activator.CreateInstance(typeof(T), raw) as T;
                    break;
                }
            }
            else
            {
                throw new System.ArgumentException($"全局配置文件【{FilePath}】不是xlsx文件");
            }
        }
        catch
        {
            throw new FileNotFoundException($"无法读取全局配置文件：{FilePath}");
        }
    }
}