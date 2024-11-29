using System;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 运行时管理器接口
/// </summary>
public interface IRuntimeDM
{
	/// <summary>
	/// 保存当前数据
	/// </summary>
	public void SaveData();
	/// <summary>
	/// 保存当前数据到指定编号的存档
	/// </summary>
	public void SaveDataToIndex(int index);
	/// <summary>
	/// 重置数据
	/// </summary>
	public void ResetData();
	public int CurFileIndex { get; }
}

/// <summary>
/// 玩家运行时资源管理器（支持多存档）（非聚合类）
/// </summary>
public abstract class RuntimeDM<TDM> : IDisposable, IRuntimeDM
	where TDM : RuntimeDM<TDM>, new()
{
	public static TDM Instance { get; private set; }
	public int CurFileIndex { get; protected set; } = -1;
	// 文件路径
	protected virtual string FileName => typeof(TDM).Name;
	protected virtual string FileDir => $"{Application.persistentDataPath}/{FileName}";
	[NonSerializeJson]
	public virtual string FilePath => $"{FileDir}/{FileName}_{CurFileIndex}.json";

	// 是否已经初始化过
	public static bool Inited { get; protected set; }

	/// <summary>
	/// 初始化数据
	/// </summary>
	public static void InitInstance(int index)
	{
		Instance = new TDM();
		Instance.CurFileIndex = index;

		if (!Inited)
		{
			Instance.InitOnce();
			Inited = true;
		}

		bool save = false;
#if UNITY_EDITOR
		// 使用测试数据
		if (Instance.UseTestData())
		{
			Instance.InitTestData();
			Instance.CurFileIndex = index;
			return;
		}
		// 使用存档数据
		else
#endif
		{
			try
			{
				Instance = Instance.LoadData();
				Instance.PostProcess();
			}
			catch (Exception)
			{
				//Debug.LogException(e);
				Instance = new TDM();
				Instance.PreProcess();
				Instance.InitData();
				Instance.PostProcess();
				save = true;
			}
		}

		Instance.CurFileIndex = index;
		if (save)
			Instance.SaveData();
	}
	public static void InitInstance() => InitInstance(0);

	/// <summary>
	/// 保存数据
	/// </summary>
	public virtual void SaveData()
	{
		if (!Directory.Exists(FileDir))
			Directory.CreateDirectory(FileDir);
		File.WriteAllText(FilePath, JsonMapper.ToJson(this));
	}
	/// <summary>
	/// 保存数据
	/// </summary>
	public void SaveDataToIndex(int index)
	{
		int temp = CurFileIndex;
		CurFileIndex = index;
		SaveData();
		CurFileIndex = temp;
	}
	/// <summary>
	/// 加载数据
	/// </summary>
	protected virtual TDM LoadData()
	{
		return JsonMapper.ToObject<TDM>(File.ReadAllText(FilePath));
	}
	/// <summary>
	/// 重置数据
	/// </summary>
	public void ResetData()
	{
		PreProcess();
		InitData();
		PostProcess();
	}

	// 仅执行一次的初始化 用于初始化事件
	protected virtual void InitOnce() { }
	// 初始化数据
	protected virtual void InitData() { }
	// 初始化用于测试的数据
	protected virtual void InitTestData() { }
	// 判断是否真的使用了测试数据
	protected virtual bool UseTestData() => false;

	// 初始化数据前进行的处理
	protected virtual void PreProcess() { }
	// 加载/初始化数据后进行的处理
	protected virtual void PostProcess() { }

	public virtual void Dispose()
	{
		Instance = null;
	}
}

public interface IRuntimeDM<TValue> : IRuntimeDM, IGetOriginValue<TValue>
{
	public int DataCount { get; }
	/// <summary>
	/// 添加数据
	/// </summary>
	public void AddValue(TValue value);
	/// <summary>
	/// 移除数据
	/// </summary>
	public bool RemoveValue(TValue value);
}

/// <summary>
/// 玩家运行时资源管理器（支持多存档）（聚合类）
/// </summary>
public abstract class RuntimeDM<TDM, TValue> : RuntimeDM<TDM>, IRuntimeDM<TValue>
	where TDM : RuntimeDM<TDM, TValue>, new()
	where TValue : class, IUnique, new()
{
	// 数据字典
	protected DicTransformer<int, TValue> data;
	[SerializeJson]
	protected readonly Dictionary<string, TValue> dataRef = new();
	[NonSerializeJson]
	public int DataCount => data.Count;

	// 初始化数据
	protected override void InitData()
	{
		dataRef.Clear();
	}
	// 加载/初始化数据后进行的处理
	protected override void PostProcess()
	{
		IDMgr.Instance.InitID(dataRef.Values);
		data = new DicTransformer<int, TValue>(dataRef);
	}

	public virtual void AddValue(TValue value)
	{
		data.Add(value.ID, value);
	}
	public virtual bool RemoveValue(TValue value)
	{
		if (!data.ContainsKey(value.ID))
			return false;
		data.Remove(value.ID);
		return true;
	}

	public TValue GetOriginValue(int id)
	{
		return data.ContainsKey(id) ? data[id] : null;
	}

	public IEnumerable<TValue> GetAllOriginValue()
	{
		return data.Values;
	}
}