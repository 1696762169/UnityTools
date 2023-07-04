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
	protected override string FilePath => $"{FileDir}/{FileName}.json";

	/// <summary>
    /// 初始化数据
    /// </summary>
    public override TConfig InitInstance(int index)
    {
	    return base.InitInstance(0);
    }
	public new TConfig InitInstance()
	{
		return base.InitInstance(0);
	}
}
