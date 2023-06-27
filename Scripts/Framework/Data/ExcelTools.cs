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
    public static void GenerateFile(Type type, string filePath, bool overwrite = false)
    {
        string typeName = type.Name.Replace("Raw", "");
        // 检查文件是否存在
        if (!overwrite)
        {
            int copyCount = 1;
            while (File.Exists(filePath))
                filePath = $"{Application.streamingAssetsPath}/{typeName}s - 副本{copyCount++}.xlsx";
            if (copyCount > 1)
                Debug.LogWarning($"类型{typeName}的Excel配置表已存在，已创建副本");
        }

        // 设置样式
        var config = new OpenXmlConfiguration()
        {
            TableStyles = TableStyles.None,
            DynamicColumns = new DynamicExcelColumn[type.GetProperties().Length + 1]
        };

        // 准备数据
        var value = new List<Dictionary<string, string>>();
        for (int i = 0; i < 3; i++)
            value.Add(new Dictionary<string, string>());
        int pCount = 0;
        int ignoreCount = 0;
        foreach (var property in type.GetProperties())
        {
            // 忽略某些列
            if (property.GetCustomAttribute<ExcelIgnoreAttribute>() != null)
            {
                config.DynamicColumns[pCount++] = new DynamicExcelColumn(property.Name) { Ignore = true };
                ++ignoreCount;
                continue;
            }
            // 填充空行
            value[0].Add(property.Name, "");
            // 写入类型
            value[1].Add(property.Name, GetShortName(property.PropertyType));
            // 写入列名
            var name = property.GetCustomAttribute<ExcelColumnNameAttribute>();
            if (name != null)
                value[2].Add(property.Name, name.ExcelColumnName);
            else
                value[2].Add(property.Name, property.Name);
            // 设置列宽
            config.DynamicColumns[pCount++] = new DynamicExcelColumn(property.Name)
            {
                Width = GetWidth(property),
                Index = pCount - ignoreCount - 1,
                Name = property.Name,
            };
        }

        // 添加注释
        const string comment = "__Comment";
        config.DynamicColumns[pCount++] = new DynamicExcelColumn(comment)
        {
            Width = 60,
            Index = pCount - ignoreCount - 1,
            Name = comment,
        };
        value[0].Add(comment, "第一行可以用来给属性写注释，这一列可以用来给每条数据写备注");
        value[1].Add(comment, "第二行是属性数据类型，不会被读取，可以修改");
        value[2].Add(comment, "第三行是属性名称，会被读取，不可更改");

        MiniExcel.SaveAs(filePath, value, false, typeName, ExcelType.XLSX, config, true);
    }
    public static void GenerateFile(Type type, bool overwrite = false)
    {
        string typeName = type.Name.Replace("Raw", "");
        string filePath = $"{Application.streamingAssetsPath}/{typeName}s.xlsx";
        GenerateFile(type, filePath, overwrite);
    }
    public static void GenerateFile<T>(string filePath, bool overwrite = false)
    {
        GenerateFile(typeof(T), filePath, overwrite);
    }
    public static void GenerateFile<T>(bool overwrite = false) where T : class, new()
    {
        GenerateFile(typeof(T), overwrite);
    }
    
    private static string GetShortName(Type type)
    {
        if (type.Name == typeof(int).Name)
            return "int";
        if (type.Name == typeof(float).Name)
            return "float";
        if (type.Name == typeof(string).Name)
            return "string";
        if (type.Name == typeof(bool).Name)
            return "bool";
        return type.Name;
    }
    private static double GetWidth(PropertyInfo property)
    {
        var width = property.GetCustomAttribute<ExcelColumnWidthAttribute>();
        if (width != null)
            return width.ExcelColumnWidth;

        var nameAttr = property.GetCustomAttribute<ExcelColumnNameAttribute>();
        string name = nameAttr == null ? property.Name : nameAttr.ExcelColumnName;
        int ret = 2;
        foreach (char c in name)
            ret += c < 128 ? 1 : 2;
        return Mathf.Max(8, ret);
    }

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
