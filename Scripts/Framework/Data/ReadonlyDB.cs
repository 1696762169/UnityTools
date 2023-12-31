using System.Collections.Generic;
using UnityEngine;
using MiniExcelLibs;
using System;
using System.Linq;
using System.Reflection;
using static UnityEditorInternal.ReorderableList;

// 保证只读数据有一个ID字段
public interface IUnique
{
    public int ID { get; }
}
// 保证只读数据可复制出副本给外部使用
public interface ICopy<out T>
{
    public T Copy();
}

// 读取原数据接口
public interface IGetOriginValue<out T>
{
    /// <summary>
    /// 根据ID获取原始数据对象
    /// </summary>
    public T GetOriginValue(int id);
    /// <summary>
    /// 获取全部原始数据对象
    /// </summary>
    public IEnumerable<T> GetAllOriginValue();
}
// 获取副本数据接口
public interface IGetCopyValue<out T>
{
    /// <summary>
    /// 根据ID获取数据的副本
    /// </summary>
    public T GetCopyValue(int id);
    /// <summary>
    /// 获取全部数据的副本
    /// </summary>
    public IEnumerable<T> GetAllCopyValue();
}

// 完整的只读数据管理类访问接口
public interface IReadonlyDB<out T> : IGetOriginValue<T>
{
    /// <summary>
    /// 判断是否包含特定ID的数据
    /// </summary>
    public bool Contains(int id);
}

/// <summary>
/// 需要从Excel表格中读取数据的只读数据管理类
/// </summary>
/// <typeparam name="TValue">数据类型</typeparam>
/// <typeparam name="TRaw">从Excel中读取到的原始数据类型</typeparam>
public abstract class ReadonlyDB<TValue, TRaw> : ControlledSingleton<ReadonlyDB<TValue, TRaw>> ,IReadonlyDB<TValue>
    where TValue : class, IUnique
    where TRaw : class, IUnique, new()
{
	/// <summary>
	/// 文件路径
	/// </summary>
	protected virtual string FileName => typeof(TValue).Name + "s";
	protected virtual string FilePath => $"{Application.streamingAssetsPath}/{FileName}.xlsx";
	/// <summary>
	/// 表格名
	/// </summary>
	protected virtual string SheetName => null;

    // 存储数据的字典
    protected Dictionary<int, TValue> Data { get; } = new();
    public TValue this[int id] => Data[id];

	/// <summary>
	/// 初始化数据
	/// </summary>
	public override ReadonlyDB<TValue, TRaw> InitInstance()
    {
	    base.InitInstance();

	    TRaw @default = MiniExcel.Query<TRaw>(FilePath, SheetName, ExcelType.XLSX, "A3").First();
        List<(PropertyInfo, object, bool)> defaults = (from property in typeof(TRaw).GetProperties()
	        where !IsDefault(property,
		        property.GetValue(@default))
	        select (property, property.GetValue(@default), property.GetValue(@default) is string str && str.EndsWith("/*"))).ToList();

        foreach (TRaw line in MiniExcel.Query<TRaw>(FilePath, SheetName, ExcelType.XLSX, "A3"))
        {
            if (line.ID == 0)
                continue;
            Data.Add(line.ID, ProcessRaw(line, defaults));
        }
        InitExtend();
        return this;
    }

    protected virtual void InitExtend() { }
    
    public TValue GetOriginValue(int id)
    {
		return Data.TryGetValue(id, out var value) ? value : null;
	}

    public IEnumerable<TValue> GetAllOriginValue()
    {
	    return Data.Values;
    }

    public bool Contains(int id) => Data.ContainsKey(id);

    // 将从Excel表格中读取到的原始数据转换为实际使用的数据
    protected virtual TValue ProcessRaw(TRaw raw, IEnumerable<(PropertyInfo, object, bool)> defaults)
    {
	    foreach ((PropertyInfo property, object value, bool isPath) in defaults)
		{
			if (isPath)
				property.SetValue(raw, (value as string).TrimEnd('*') + property.GetValue(raw));
			else if (IsDefault(property, property.GetValue(raw)))
                property.SetValue(raw, value);
	    }
	    return Activator.CreateInstance(typeof(TValue), raw) as TValue;
    }

    private static bool IsDefault(PropertyInfo property, object value)
    {
	    return Equals(value, property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null);
	}
}

/// <summary>
/// 可复制项目的只读数据管理类
/// </summary>
/// <typeparam name="TValue">数据类型</typeparam>
/// <typeparam name="TRaw">从Excel中读取到的原始数据类型</typeparam>
public abstract class CopyableDB<TValue, TRaw> : ReadonlyDB<TValue, TRaw>, IGetCopyValue<TValue>
	where TValue : class, IUnique, ICopy<TValue>
	where TRaw : class, IUnique, new()
{
    public TValue GetCopyValue(int id)
    {
	    return Data.TryGetValue(id, out var value) ? value.Copy() : null;
    }
    public IEnumerable<TValue> GetAllCopyValue()
    {
	    return Data.Values.Select(value => value.Copy());
    }
}
