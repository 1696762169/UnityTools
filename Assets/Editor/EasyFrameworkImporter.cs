using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.PackageManager;
using UnityEngine;
using Regex = System.Text.RegularExpressions.Regex;

/// <summary>
/// 在框架文件更新时自动处理依赖导入
/// </summary>
[ScriptedImporter(1, "frameconfig")]
public class EasyFrameworkImporter : ScriptedImporter
{
	public const string CONFIG_PATH = "Assets/Scripts/Core/EasyFramework.frameconfig";
	public static readonly string ManifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

	public override void OnImportAsset(AssetImportContext ctx)
	{
		if (ctx.assetPath != CONFIG_PATH)
		{
			Debug.LogError("An Unexpected Framework Config File Imported: \n" + ctx.assetPath);
			return;
		}
		string configPath = Path.Combine(Application.dataPath, ctx.assetPath.Replace("Assets/", ""));
		FrameworkConfig obj = JsonConvert.DeserializeObject<FrameworkConfig>(File.ReadAllText(configPath));

		// 更新Unity依赖包
		Regex versionRegex = new Regex(@"^(\d+\.){2}\d+$");
		Dictionary<string, string> currentPackages = GetDependencies();
		List<string> packages = new List<string>();
		foreach (var package in obj.unityPackages)
		{
			if (versionRegex.IsMatch(package.Value))
			{
				if (currentPackages.TryGetValue(package.Value, out string curVersion) &&
				    (versionRegex.IsMatch(curVersion) && !IsVersionHigher(package.Value, curVersion) || 
				     curVersion.StartsWith(">")))
					continue;
				packages.Add($"{package.Key}@{package.Value}");
			}
			else
			{
				packages.Add(package.Value);
			}
		}

		for (int i = 0; i < packages.Count; i++)
		{
			string package = packages[i];
			var req = Client.Add(package);
			EditorUtility.DisplayProgressBar("Import Package", $"Importing package {package}...", i / (float)packages.Count);
			while (!req.IsCompleted)
				;
			if (req.Status == StatusCode.Failure)
				Debug.LogError(req.Error.message);
			if (req.Status == StatusCode.Success)
				Debug.Log($"Package {package} imported successfully");
		}
		Client.AddAndRemove(packagesToAdd:packages.ToArray());
	}

	public static Dictionary<string, string> GetDependencies()
	{
		var request = Client.List(true);
		while (!request.IsCompleted)
			;
		if (request.Error!= null)
			Debug.LogError(request.Error.message);
		if (request.Status == StatusCode.Failure)
		{
			Debug.LogError("Failed to retrieve package list");
			return new Dictionary<string, string>();
		}

		return request.Result.Where(package => package.source != PackageSource.Embedded)
			.ToDictionary(package => package.name, package => package.source == PackageSource.Git ? package.packageId.Split("@").Last() : package.version);
	}

	public static bool IsVersionHigher(string v1, string v2)
	{
		int[] v1Arr = v1.Split('.').Select(int.Parse).ToArray();
		int[] v2Arr = v2.Split('.').Select(int.Parse).ToArray();
		for (int i = 0; i < v1Arr.Length; i++)
		{
			if (v1Arr[i] > v2Arr[i])
				return true;
			if (v1Arr[i] < v2Arr[i])
				return false;
		}
		return false;
	}


	public class FrameworkConfig
	{
		public string version;
		public Dictionary<string, string> unityPackages;
		public Dictionary<string, string> nugetPackages;
	}
}
