using System;

/// <summary>
/// 可控制访问权限的单例对象基类
/// </summary>
public abstract class ControlledSingleton<T> : IDisposable where T : ControlledSingleton<T>
{
	// ReSharper disable once StaticMemberInGenericType
	public static bool HasInstance { get; protected set; }

	public virtual T InitInstance()
	{
		if (HasInstance)
			throw new Exception($"{typeof(T).Name}类的单例对象已经存在");
		HasInstance = true;
		return (T)this;
	}

	public virtual void Dispose()
	{
		HasInstance = false;
	}
}
