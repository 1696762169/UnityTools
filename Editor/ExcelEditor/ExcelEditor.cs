using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs;
using System.Reflection;

/// <summary>
/// Excel工具编辑器脚本
/// </summary>
[CustomEditor(typeof(ExcelToolGUI))]
public class ExcelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ExcelToolGUI excel = target as ExcelToolGUI;

        GUILayout.Space(10);
        if (GUILayout.Button("生成表格文件", GUILayout.Width(150)))
        {
            string dataType = excel.dataType.Replace(".", "+");
            Type type = typeof(GameManager).Assembly.GetType(dataType);
            GenerateFile(type, $"{Application.streamingAssetsPath}/{excel.fileName}.xlsx", excel.overwrite);
            Debug.Log($"文件{excel.fileName}.xlsx生成成功");
        }
    }

	public static void GenerateFile(Type type, string filePath, bool config = false, bool overwrite = false)
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
		var configuration = new OpenXmlConfiguration()
		{
			TableStyles = TableStyles.None,
			DynamicColumns = new DynamicExcelColumn[type.GetProperties().Length + 1]
		};

		// 准备数据
		var value = new List<Dictionary<string, object>>();
		for (int i = 0; i < (config ? 3 : 4); i++)
			value.Add(new Dictionary<string, object>());
		int pCount = 0;
		int ignoreCount = 0;
		foreach (var property in type.GetProperties())
		{
			// 忽略某些列
			if (property.GetCustomAttribute<ExcelIgnoreAttribute>() != null)
			{
				configuration.DynamicColumns[pCount++] = new DynamicExcelColumn(property.Name) { Ignore = true };
				++ignoreCount;
				continue;
			}
			// 写入注释（空行）
			value[0].Add(property.Name, "");
			// 写入类型
			value[1].Add(property.Name, GetShortName(property.PropertyType));
			// 写入列名
			var name = property.GetCustomAttribute<ExcelColumnNameAttribute>();
			string propertyName = name != null ? name.ExcelColumnName : property.Name;
			value[2].Add(property.Name, propertyName);
			// 写入默认值
			if (!config)
				value[3].Add(property.Name, property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null);
			// 设置列宽
			configuration.DynamicColumns[pCount++] = new DynamicExcelColumn(property.Name)
			{
				Width = GetWidth(property),
				Index = pCount - ignoreCount - 1,
				Name = property.Name,
			};
		}

		// 添加注释
		const string COMMENT = "__Comment";
		configuration.DynamicColumns[pCount++] = new DynamicExcelColumn(COMMENT)
		{
			Width = 60,
			Index = pCount - ignoreCount - 1,
			Name = COMMENT,
		};
		value[0].Add(COMMENT, "第一行可以用来给属性写注释，这一列可以用来给每条数据写备注");
		value[1].Add(COMMENT, "第二行是属性数据类型，不会被读取，可以修改");
		value[2].Add(COMMENT, "第三行是属性名称，会被读取，不可更改");
		if (!config)
			value[3].Add(COMMENT, "第四行是属性默认值，可以不填");

		MiniExcel.SaveAs(filePath, value, false, typeName, ExcelType.XLSX, configuration, true);
	}
	public static void GenerateFile(Type type, bool config = false, bool overwrite = false)
	{
		string typeName = type.Name.Replace("Raw", "");
		string filePath = $"{Application.streamingAssetsPath}/{typeName}s.xlsx";
		GenerateFile(type, filePath, config, overwrite);
	}
	public static void GenerateFile<T>(string filePath, bool config = false, bool overwrite = false)
	{
		GenerateFile(typeof(T), filePath, config, overwrite);
	}
	public static void GenerateFile<T>(bool config = false, bool overwrite = false) where T : class, new()
	{
		GenerateFile(typeof(T), config, overwrite);
	}

	private static string GetShortName(Type type)
	{
		return type.Name switch
		{
			nameof(Int32) => "int",
			nameof(Single) => "float",
			nameof(String) => "string",
			nameof(Boolean) => "bool",
			_ => type.Name
		};
	}
	private static double GetWidth(PropertyInfo property)
	{
		var width = property.GetCustomAttribute<ExcelColumnWidthAttribute>();
		if (width != null)
			return width.ExcelColumnWidth;

		var nameAttr = property.GetCustomAttribute<ExcelColumnNameAttribute>();
		string name = nameAttr == null ? property.Name : nameAttr.ExcelColumnName;
		int ret = 2 + name.Sum(c => c < 128 ? 1 : 2);
		return Mathf.Max(8, ret);
	}
}
