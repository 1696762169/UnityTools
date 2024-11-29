using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using LitJson;
using System.Text.RegularExpressions;

/// <summary>
/// 在编辑器中选择对象的脚本
/// </summary>
public class AssetsFilter : ScriptableObject
{
    // 储存字段值
    public string folderPath;
    public AssetType assetType;

    // 控制是否显示帮助框
    public bool showHelpBoxes = false;

    private string selectedJsonFile = "";
    private string jsonFileName = "";
    private bool showAdvance = false;   // 是否显示高级选项

    // 创建枚举，用于表示特定类型的资源
    public enum AssetType
    {
        Prefab, // 预制体
        Texture, // 纹理
        Material, // 材质
    }

    // 名称匹配枚举
    private enum NameConditionType
    {
        All,        // 所有均可
        ExactMatch, // 精确匹配
        StartsWith, // 以...开始
        EndsWith,   // 以...结尾
        RegexMatch  // 正则表达式匹配
    }
    private NameConditionType nameConditionType;    // 名称查找条件类型
    private string conditionValue = ""; // 名称查找条件值

    // 用于存储排除在结果之外的资产名称的字符串列表
    [SerializeField] private List<string> excludedAssetNames = new List<string>();

    public const string ASSETS_FILTER_FOLDER = "Assets/Scripts/Editor/AssetsFilter/";

    // 提供一个按钮以将排除的名称列表存储为JSON文件（使用LitJson库）
    public void SaveExcludedNamesToJson(string filename)
    {
        Directory.CreateDirectory(ASSETS_FILTER_FOLDER);

        string jsonString = JsonMapper.ToJson(excludedAssetNames);
        File.WriteAllText($"{ASSETS_FILTER_FOLDER}/{filename}.json", jsonString);
    }

    // 提供一个按钮以从JSON文件中加载排除的名称列表
    public void LoadExcludedNamesFromJson(string fileName)
    {
        string jsonString = File.ReadAllText($"{ASSETS_FILTER_FOLDER}/{fileName}");
        excludedAssetNames = JsonMapper.ToObject<List<string>>(jsonString);
    }

    // 查找文件夹及其子文件夹中特定类型的所有资产，并以其自身类型返回
    public List<T> FindAssetsInFolder<T>(string folderPath, AssetType assetType) where T : Object
    {
        string assetTypeString = GetAssetTypeString(assetType);
        if (string.IsNullOrEmpty(folderPath) || folderPath.IndexOf("Assets") < 0)
        {
            Debug.LogWarning("选择的文件夹路径不在工程内");
            return new List<T>();
        }

        folderPath = folderPath[folderPath.IndexOf("Assets")..];
        string[] guids = AssetDatabase.FindAssets($"t:{assetTypeString}", new[] { folderPath });
        List<T> assets = new List<T>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (!excludedAssetNames.Contains(asset.name))
            {
                bool nameMatch = true;
                // 检查更名条件是否满足
                switch (nameConditionType)
                {
                case NameConditionType.ExactMatch:
                    nameMatch = asset.name == conditionValue;
                    break;
                case NameConditionType.StartsWith:
                    nameMatch = asset.name.StartsWith(conditionValue);
                    break;
                case NameConditionType.EndsWith:
                    nameMatch = asset.name.EndsWith(conditionValue);
                    break;
                case NameConditionType.RegexMatch:
                    nameMatch = Regex.IsMatch(asset.name, conditionValue);
                    break;
                }

                if (nameMatch)
                    assets.Add(asset);
            }
        }

        return assets;
    }

    // 将自定义枚举转换为Unity可识别的字符串
    private string GetAssetTypeString(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.Prefab => "Prefab",
            AssetType.Texture => "Texture",
            AssetType.Material => "Material",
            _ => "",
        };
    }

    // 在OnGUI中显示此组件的功能
    public void ShowFunction()
    {
        // 选择文件夹
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("文件夹路径:", GUILayout.Width(75));
        folderPath = EditorGUILayout.TextField(folderPath);
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100)))
        {
            folderPath = EditorUtility.OpenFolderPanel("选择文件夹", "Assets", "");
        }
        EditorGUILayout.EndHorizontal();

        showAdvance = EditorGUILayout.Foldout(showAdvance, "高级");
        if (!showAdvance)
        {
            GUILayout.Space(20);
            return;
        }

        // 显示排除的资源名称
        ShowFileSaveHelpBox();
        GUILayout.Label("排除的资源名称:");
        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < excludedAssetNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            excludedAssetNames[i] = EditorGUILayout.TextField(excludedAssetNames[i]);
            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                excludedAssetNames.RemoveAt(i);
                selectedJsonFile = "";
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加", GUILayout.Width(50)))
        {
            excludedAssetNames.Add("");
            selectedJsonFile = "";
        }
        if (GUILayout.Button("清空", GUILayout.Width(50)))
        {
            excludedAssetNames.Clear();
            selectedJsonFile = "";
            jsonFileName = "";
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // 保存排除的资源名称到JSON文件
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("保存排除的资源名称", GUILayout.Width(150)))
        {
            SaveExcludedNamesToJson(jsonFileName);
            selectedJsonFile = "";
        }
        GUILayout.Label("JSON文件名:", GUILayout.Width(75));
        jsonFileName = EditorGUILayout.TextField(string.IsNullOrEmpty(selectedJsonFile) ? jsonFileName : Path.GetFileNameWithoutExtension(selectedJsonFile));
        EditorGUILayout.EndHorizontal();

        // 加载排除的资源名称列表
        if (GUILayout.Button("加载排除的资源名称", GUILayout.Width(150)))
        {
            var info = new DirectoryInfo("Assets/Scripts/Editor/AssetsFilter/");
            FileInfo[] fileInfo = info.GetFiles("*.json");

            GenericMenu menu = new GenericMenu();
            foreach (FileInfo file in fileInfo)
            {
                menu.AddItem(new GUIContent(file.Name), false, OnJsonFileSelected, file.Name);
            }
            menu.ShowAsContext();
        }

        if (!string.IsNullOrEmpty(selectedJsonFile))
        {
            GUILayout.Label($"已选择: {selectedJsonFile}");
        }

        // 选择名称筛选类型
        ShowNameConditionTypeHelpBox();
        nameConditionType = (NameConditionType)EditorGUILayout.EnumPopup("名称筛选条件类型", nameConditionType);
        if (nameConditionType != NameConditionType.All)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("条件值:", GUILayout.Width(75));
            conditionValue = EditorGUILayout.TextField(conditionValue);
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        void OnJsonFileSelected(object userData)
        {
            selectedJsonFile = (string)userData;
            LoadExcludedNamesFromJson(selectedJsonFile);
        }
    }
    private void ShowFileSaveHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("可单独设置操作时要排除的资源名称\n" +
                "设置文件名 即可将当前设置的列表保存\n" +
                "再次打开编辑器时可选择文件加载", MessageType.Info);
        }
    }
    private void ShowNameConditionTypeHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("筛选预制体名称，仅有被筛选到的预制体会被影响:\n" +
                "All: 所有均可\n" +
                "ExactMatch: 精确匹配\n" +
                "StartsWith: 以...开始\n" +
                "EndsWith: 以...结尾\n" +
                "RegexMatch: 正则表达式匹配", MessageType.Info);
        }
    }
}