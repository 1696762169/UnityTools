using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Regex = System.Text.RegularExpressions.Regex;
using Config = EasyFrameworkImporter.FrameworkConfig;

public static class EasyFrameworkExporter
{
	private const string PACKAGE_NAME = "EasyFramework";
	private static readonly string ExportPath = Application.dataPath + "/../Build/";
	private static readonly string[] FoldersToInclude = {
		"Assets/Scripts/Core",
		"Assets/Editor",
		"Assets/Plugins",
		"Packages/nuget-packages"
	};

	// 生成依赖配置时 排除以下包
	private static readonly HashSet<string> ExcludedPackages = new HashSet<string>
	{
		"com.unity.modules.*",
		"com.unity.ide.*",
		"com.unity.render-pipelines.*",
		"com.unity.collab-proxy",
		"com.unity.textmeshpro",
		"com.unity.timeline",
		"com.unity.ugui",
		"com.unity.visualscripting",
	};

	[MenuItem("Tools/Export Framework as Package")]
	public static void ExportPackage()
	{
		// 确保导出目录存在
		if (!Directory.Exists(ExportPath))
		{
			Directory.CreateDirectory(ExportPath);
		}

		// 生成依赖配置
		GenerateConfig();

		// 收集所有资产路径
		List<string> assetPaths = new List<string>();
		foreach (string folder in FoldersToInclude)
		{
			if (Directory.Exists(folder))
			{
				// 获取文件夹中的所有文件（不包括meta文件）
				string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
				foreach (string file in files)
				{
					if (!file.EndsWith(".meta"))
					{
						string assetPath = file.Replace("\\", "/");
						assetPaths.Add(assetPath);
					}
				}

				// 添加文件夹本身（确保空文件夹也被包含）
				assetPaths.Add(folder);
			}
			else
			{
				EasyLogger.LogWarning($"文件夹不存在: {folder}");
			}
		}

		// 移除重复项
		assetPaths = new List<string>(new HashSet<string>(assetPaths));

		// 构建完整的导出路径
		string fullExportPath = Path.Combine(ExportPath, PACKAGE_NAME + ".unitypackage");

		try
		{
			EditorUtility.DisplayProgressBar("Exporting Package", "Creating unitypackage...", 0.5f);
			AssetDatabase.ExportPackage(assetPaths.ToArray(), fullExportPath, ExportPackageOptions.Recurse);
			EditorUtility.DisplayProgressBar("Exporting Package", "Package created successfully!", 1.0f);
			EditorUtility.DisplayDialog("Success", $"Package exported successfully to:\n{fullExportPath}", "OK");
			//EditorUtility.RevealInFinder(fullExportPath);
			EasyLogger.Log($"Package exported: {fullExportPath}");
		}
		catch (System.Exception e)
		{
			EasyLogger.LogError("Package export failed: " + e.Message);
			EditorUtility.DisplayDialog("Error", "Package export failed: " + e.Message, "OK");
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	private static void GenerateConfig()
	{
		// 过滤掉不需要的包
		Regex excludeRegex = new Regex(string.Join("|", ExcludedPackages.Select(p => p.Replace(".", "\\.").Replace("*", ".*"))));
		Dictionary<string, string> allDependencies = EasyFrameworkImporter.GetDependencies();
		Dictionary<string, string> dependencies = allDependencies.Where(p =>!excludeRegex.IsMatch(p.Key))
			.ToDictionary(p => p.Key, p => p.Value);

		// 生成配置
		Config config = new Config
		{
			version = Application.version,
			unityPackages = dependencies,
			nugetPackages = new Dictionary<string, string>(),
		};
		string configText = JsonConvert.SerializeObject(config, Formatting.Indented);
		string configPath = Path.Combine(Application.dataPath, EasyFrameworkImporter.CONFIG_PATH.Replace("Assets/", ""));
		File.WriteAllText(configPath, configText);
	}
}