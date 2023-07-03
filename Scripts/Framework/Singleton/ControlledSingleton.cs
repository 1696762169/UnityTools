using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可控制访问权限的单例对象基类
/// </summary>
public abstract class ControlledSingleton : IDisposable
{
	public bool HasInstance { get; private set; }

	public virtual void InitInstance()
	{
		if (HasInstance)
			return;
		HasInstance = true;
	}

	public virtual void Dispose()
	{
		HasInstance = false;
	}
}
