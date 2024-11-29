using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using LitJson;
using UnityEngine;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Attributes;
using Unity.VisualScripting;
using UnityEditor;

/// <summary>
/// 根据数据类型生成Excel存储文件的工具类
/// </summary>
public static class ExcelTools
{
	/* 复制基本类型数据到新的对象中 要求变量同名 */
    private static readonly HashSet<Type> supportTypes = new HashSet<Type>()
    {
        typeof(int),
        typeof(float),
        typeof(string),
        typeof(bool),
    };
    public static void BasicCopy<T1, T2>(T1 origin, T2 result) where T1 : class where T2 : class
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        foreach (PropertyInfo prop in origin.GetType().GetProperties(flags))
        {
            if (supportTypes.Contains(prop.PropertyType) || prop.PropertyType.IsEnum)
            {
                PropertyInfo assignProp = result.GetType().GetProperty(prop.Name, flags);
                if (assignProp != null && assignProp.PropertyType == prop.PropertyType && assignProp.CanWrite)
                    assignProp.SetValue(result, prop.GetValue(origin));
            }
        }

        foreach (FieldInfo field in origin.GetType().GetFields(flags))
        {
            if (supportTypes.Contains(field.FieldType) || field.FieldType.IsEnum)
            {
                FieldInfo assignProp = result.GetType().GetField(field.Name, flags);
                if (assignProp != null && assignProp.FieldType == field.FieldType)
                    assignProp.SetValue(result, field.GetValue(origin));
            }
        }
    }
    public static T2 BasicCopy<T1, T2>(T1 origin) where T1 : class where T2 : class
    {
        T2 result = Activator.CreateInstance<T2>();
        BasicCopy(origin, result);
        return result;
    }

    /* 解析基本类型数据到新的对象中 要求变量与列同名 */
    public static void BasicParse<TValue>(TValue origin, DataRow dataRow)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        // 解析属性
        foreach (PropertyInfo property in origin.GetType().GetProperties(flags))
        {
	        if (property.GetCustomAttribute<ExcelIgnoreAttribute>() != null)
		        continue;
#if UNITY_EDITOR
	        if (TestEnvironment.Current.DebugExcelColumnCheck)
	        {
		        if (!dataRow.Table.Columns.Contains(property.Name))
		        {
			        ExcelCommentAttribute attr = property.GetCustomAttribute<ExcelCommentAttribute>();
			        string comment = attr != null ? $"({attr.Comment})" : "";
			        Debug.LogError($"数据类 {origin.GetType()} 的属性 {property.Name}{comment} 没有相应的配置");
			        continue;
		        }
	        }
#endif
			object value = GetValue(property.PropertyType, property.Name, dataRow);
	        if (value != null)
				property.SetValue(origin, value);
        }

		// 解析字段
		foreach (FieldInfo field in origin.GetType().GetFields(flags))
		{
			if (field.GetCustomAttribute<ExcelIgnoreAttribute>() != null)
				continue;
#if UNITY_EDITOR
			if (TestEnvironment.Current.DebugExcelColumnCheck)
			{
				if (!dataRow.Table.Columns.Contains(field.Name))
				{
					ExcelCommentAttribute attr = field.GetCustomAttribute<ExcelCommentAttribute>();
					string comment = attr != null ? $"({attr.Comment})" : "";
					Debug.LogError($"数据类 {origin.GetType()} 的字段 {field.Name}{comment} 没有相应的配置");
					continue;
				}
			}
#endif
			object value = GetValue(field.FieldType, field.Name, dataRow);
			if (value != null)
				field.SetValue(origin, value);
		}

		object GetValue(Type type, string name, DataRow row)
		{
			try
			{
				if (row[name] is DBNull)
					return null;
			}
			catch (ArgumentException)
			{
                return null;
			}

			object value = row[name];
            string strValue = value.ToString();
			if (type == typeof(int))
				return string.IsNullOrEmpty(strValue) ? 0 : Convert.ToInt32(value);
			if (type == typeof(string))
				return strValue;
			if (type == typeof(float))
				return string.IsNullOrEmpty(strValue) ? 0.0f : Convert.ToSingle(value);
			if (type.IsEnum)
				return ToEnum(type, value);
			if (type == typeof(bool))
				return !string.IsNullOrEmpty(strValue) && Convert.ToBoolean(value);
			return null;
		}
	}
    public static TValue BasicParse<TValue>(DataRow dataRow) where TValue : new()
    {
        TValue ret = new TValue();
	    BasicParse(ret, dataRow);
        return ret;
    }

    // 解析枚举
    public static object ToEnum(Type type, object value)
    {
	    return Enum.TryParse(type, value.ToString(), false, out object ret) ? ret : default;
	}

	/* 将字符串解析为List/Dictionary */
	public static List<int> ParseIntList(string origin) => ParseList<int>(origin, int.Parse);
    public static List<float> ParseFloatList(string origin) => ParseList<float>(origin, float.Parse);
    public static List<string> ParseStringList(string origin) => ParseList<string>(origin, (str) => str);
    public static List<T> ParseEnumList<T>(string origin) where T : struct => ParseList<T>(origin, (str) => (T)Enum.Parse(typeof(T), str));
    public static List<T> ParseList<T>(string origin, Func<string, T> parser)
    {
        List<T> list = new List<T>();
        if (string.IsNullOrEmpty(origin))
            return list;
        foreach (string value in origin.Split(';'))
        {
            if (value.Trim() != "")
                list.Add(parser(value.Trim()));
        }
        return list;
    }

    public static Dictionary<TKey, TValue> ParseDict<TKey, TValue>(string origin, Func<string, TKey> keyParser, Func<string, TValue> valueParser)
    {
        Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
        if (string.IsNullOrEmpty(origin))
            return dict;
        foreach (string pair in origin.Split(';'))
        {
            if (pair.Trim() == "")
                continue;
            string key = pair.Split(':')[0];
            string value = pair.Split(':')[1];
            dict.Add(keyParser(key.Trim()), valueParser(value.Trim()));
        }
        return dict;
    }

    /// <summary>
    /// 从DataTable中获取某一类型数据的全部有效默认值
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>name: 默认值属性名 value: 默认值 isPath: 默认值是否为路径</returns>
    public static List<(string name, object value, bool isPath)> GetDefaults<T>(DataTable table)
    {
        DataRow defaultRow = table.Rows[0];
        var ret = new List<(string name, object value, bool isPath)>();
        foreach (PropertyInfo property in typeof(T).GetProperties())
        {
	        try
	        {
		        if (!defaultRow.IsNull(property.Name))
                    ret.Add((property.Name,
	                    defaultRow[property.Name],
	                    defaultRow[property.Name] is string str && str.EndsWith("/*")));
	        }
	        catch (ArgumentException) { }
        }

        return ret;
    }

    /// <summary>
    /// 将数据默认值应用到某一类型的数据行中 需要自行保证数据与默认值类型匹配
    /// </summary>
    /// <param name="origin">原始数据</param>
    /// <param name="defaults">默认值</param>
    public static DataRow ApplyDefaults(DataRow origin, List<(string name, object value, bool isPath)> defaults)
    {
	    foreach ((string propertyName, object value, bool isPath) in defaults)
	    {
		    if (isPath)
			    origin[propertyName] = value.ToString().TrimEnd('*') + origin[propertyName];
		    else if (origin[propertyName] is DBNull)
			    origin[propertyName] = value;
	    }
        return origin;
	}
}

/// <summary>
/// 列注释特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExcelCommentAttribute : Attribute
{
	public string Comment { get; }

	public ExcelCommentAttribute(string comment)
	{
		Comment = comment;
	}
}

/// <summary>
/// 列实际类型特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExcelTypeAttribute : Attribute
{
	public Type Type { get; }

	public ExcelTypeAttribute(Type type)
	{
		Type = type;
	}
}

/// <summary>
/// 列默认值特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExcelDefaultAttribute : Attribute
{
	public object Value { get; }

	public ExcelDefaultAttribute(object value)
	{
		Value = value;
	}
}


/// <summary>
/// 用于属性上的标题特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PropertyHeaderAttribute : Attribute
{
	public string Header { get; }
	public PropertyHeaderAttribute() => Header = null;
	public PropertyHeaderAttribute(string header) => Header = header;
}