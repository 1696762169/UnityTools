using LitJson;
using System.IO;
using MiniExcelLibs;
using System;
using System.Linq;
using UnityEngine;
using System.Data;
using MiniExcelLibs.Attributes;

/// <summary>
/// Xlsx格式的全局配置类
/// </summary>
public abstract class GlobalConfigXlsx<T>
	where T : GlobalConfigXlsx<T>, new()
{
	public static T Instance
	{
		get
		{
			if (m_Instance != null)
				return m_Instance;
			m_Instance = new T();
			m_Instance.InitInstance();
			return m_Instance;
		}
	}
	private static T m_Instance;

	[ExcelIgnore]
	public virtual string FileName => typeof(T).Name;
#if UNITY_ANDROID && !UNITY_EDITOR
	[ExcelIgnore]
    public string FilePath => Path.Combine(AndroidConfigInit.ConfigRoot, $"{FileName}.xlsx");
#else
	[ExcelIgnore]
	public string FilePath => $"{Application.streamingAssetsPath}/{FileName}.xlsx";
#endif
	[ExcelIgnore]
	public virtual string SheetName => null;

	private void InitInstance()
	{
		try
        {
			using FileStream fileStream = File.OpenRead(FilePath);
			DataTable table = fileStream.QueryAsDataTable(true, SheetName, ExcelType.XLSX, "A3");
			DataRow data = table.Select().FirstOrDefault() ?? throw new Exception($"全局配置表格 {FileName} 中没有可用数据");

#if UNITY_EDITOR
			// 注入测试数据
			var testDict = TestEnvironment.Current.configModifier;
			if (testDict.TryGetValue(GetType().Name, out var testList))
			{
				foreach (var testData in testList.list)
					data[testData.name] = testData.value;
			}
#endif

			// 解析数据
			ExcelTools.BasicParse(this, data);

			InitExtend();
		}
        catch (Exception ex)
        {
            throw new FileNotFoundException($"无法读取全局配置表格：{FilePath}\n{ex}");
        }
    }

	protected virtual void InitExtend() { }
}