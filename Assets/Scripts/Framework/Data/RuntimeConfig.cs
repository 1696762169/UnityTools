using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 运行时读写的单例数据
/// </summary>
public abstract class RuntimeConfig<TConfig> : RuntimeDM<TConfig>
	where TConfig : RuntimeConfig<TConfig>, new()
{
	protected override string FileDir => Application.persistentDataPath;
	public override string FilePath => $"{FileDir}/{FileName}.json";

	/// <summary>
    /// 初始化数据
    /// </summary>
    public new static void InitInstance(int index)
    {
	    RuntimeDM<TConfig>.InitInstance(0);
    }
}
