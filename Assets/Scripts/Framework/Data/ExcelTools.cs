using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.Attributes;
using Unity.VisualScripting;

/// <summary>
/// 根据数据类型生成Excel存储文件的工具类
/// </summary>
public static class ExcelTools
{
	/* 复制基本类型数据到新的对象中 要求变量同名 */
    private static readonly HashSet<Type> supportTypes = new()
    {
        typeof(int),
        typeof(float),
        typeof(string),
        typeof(bool),
    };
    public static void BasicCopy<T1, T2>(T1 origin, T2 result) where T1 : class where T2 : class
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        foreach (PropertyInfo prop in typeof(T1).GetProperties(flags))
        {
            if (supportTypes.Contains(prop.PropertyType) || prop.PropertyType.IsEnum)
            {
                PropertyInfo assginProp = typeof(T2).GetProperty(prop.Name, flags);
                if (assginProp != null && assginProp.PropertyType == prop.PropertyType && assginProp.CanWrite)
                    assginProp.SetValue(result, prop.GetValue(origin));
            }
        }

        foreach (FieldInfo field in typeof(T1).GetFields(flags))
        {
            if (supportTypes.Contains(field.FieldType) || field.FieldType.IsEnum)
            {
                FieldInfo assginProp = typeof(T2).GetField(field.Name, flags);
                if (assginProp != null && assginProp.FieldType == field.FieldType)
                    assginProp.SetValue(result, field.GetValue(origin));
            }
        }
    }
    public static T2 BasicCopy<T1, T2>(T1 origin) where T1 : class where T2 : class
    {
        T2 result = Activator.CreateInstance<T2>();
        BasicCopy(origin, result);
        return result;
    }

    /* 将字符串解析为List/Dictionary */
    public static List<int> ParseIntList(string origin) => ParseList<int>(origin, (str) => int.Parse(str));
    public static List<float> ParseFloatList(string origin) => ParseList<float>(origin, (str) => float.Parse(str));
    public static List<string> ParseStringList(string origin) => ParseList<string>(origin, (str) => str);
    public static List<T> ParseEnumList<T>(string origin) where T : struct => ParseList<T>(origin, (str) => Enum.Parse<T>(str));
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
}
