using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniExcelLibs;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

// 保证只读数据有一个ID字段
public interface IUnique
{
    public int ID { get; }
}
// 保证只读数据可复制出副本给外部使用
public interface ICopy<T>
{
    public T Copy();
}

// 读取原数据接口
public interface IGetOriginValue<T>
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
public interface IGetCopyValue<T>
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
public interface IReadonlyDB<T> : IGetOriginValue<T>
{
    /// <summary>
    /// 判断是否包含特定ID的数据
    /// </summary>
    public bool Contains(int id);
}

/// <summary>
/// 需要从Excel表格中读取数据的的只读数据管理类
/// </summary>
/// <typeparam name="TDB">本类型</typeparam>
/// <typeparam name="TValue">数据类型</typeparam>
/// <typeparam name="TRaw">从Excel中读取到的原始数据类型</typeparam>
public abstract class ReadonlyDB<TDB, TValue, TRaw> : IReadonlyDB<TValue>
    where TDB : ReadonlyDB<TDB, TValue, TRaw>, new()
    where TValue : class, IUnique
    where TRaw : class, IUnique, new()
{
    private static TDB instance = new TDB();
    public static TDB Instance => instance;
    private bool inited = false;

    /// <summary>
    /// 文件路径
    /// </summary>
    [NonSerializedField]
    public abstract string FilePath { get; }
    /// <summary>
    /// 表格名
    /// </summary>
    [NonSerializedField]
    public virtual string SheetName { get; } = null;

    // 存储数据的字典
    protected Dictionary<int, TValue> m_Data = new Dictionary<int, TValue>();
    public TValue this[int id] => m_Data[id];
    /// <summary>
    /// 初始化数据
    /// </summary>
    public virtual void InitInstance()
    {
        if (inited)
            return;
        inited = true;
        foreach (TRaw line in MiniExcel.Query<TRaw>(FilePath, SheetName, ExcelType.XLSX, "A3"))
        {
            if (line.ID == 0)
                continue;
            m_Data.Add(line.ID, ProcessRaw(line));
        }
        InitExtend();
    }

    public virtual void InitExtend() { }

    //public TValue GetCopyValue(int id)
    //{
    //    if (!m_Data.ContainsKey(id))
    //        return null;
    //    return m_Data[id].Copy();
    //}
    //public IEnumerable<TValue> GetAllCopyValue()
    //{
    //    foreach (TValue value in m_Data.Values)
    //        yield return value.Copy();
    //}

    public TValue GetOriginValue(int id)
    {
        if (!m_Data.ContainsKey(id))
            return null;
        return m_Data[id];
    }

    public IEnumerable<TValue> GetAllOriginValue()
    {
        foreach (TValue value in m_Data.Values)
            yield return value;
    }

    public bool Contains(int id) => m_Data.ContainsKey(id);

    // 将从Excel表格中读取到的原始数据转换为实际使用的数据
    protected virtual TValue ProcessRaw(TRaw raw) => Activator.CreateInstance(typeof(TValue), raw) as TValue;
}