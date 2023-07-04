using LitJson;
using System.IO;
using MiniExcelLibs;
using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Xlsx格式的全局配置类
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TRaw"></typeparam>
public abstract class GlobalConfigXlsx<T, TRaw> : ControlledSingleton<GlobalConfigXlsx<T, TRaw>>
	where T : GlobalConfigXlsx<T, TRaw>, new()
	where TRaw : class, new()
{
	public virtual string FileName => typeof(T).Name;
	public virtual string FilePath => $"{Application.streamingAssetsPath}/{FileName}.xlsx";
	public virtual string SheetName => null;

	public new virtual T InitInstance()
	{
		base.InitInstance();

        try
        {
            if (!FilePath.EndsWith("xlsx"))
	            throw new ArgumentException($"全局配置文件【{FilePath}】不是xlsx文件");
            if (!File.Exists(FilePath))
				throw new FileNotFoundException($"全局配置文件【{FilePath}】不存在");
			
            // 只读取第一行有效数据
            return MiniExcel.Query<TRaw>(FilePath, SheetName, ExcelType.XLSX, "A3")
	            .Select(raw => Activator.CreateInstance(typeof(T), raw) as T)
	            .FirstOrDefault();
        }
        catch
        {
            throw new FileNotFoundException($"无法读取全局配置文件：{FilePath}");
        }
    }
}