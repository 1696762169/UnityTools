using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using LitJson;
using System.Text.RegularExpressions;

/// <summary>
/// �ڱ༭����ѡ�����Ľű�
/// </summary>
public class AssetsFilter : ScriptableObject
{
    // �����ֶ�ֵ
    public string folderPath;
    public AssetType assetType;

    // �����Ƿ���ʾ������
    public bool showHelpBoxes = false;

    private string selectedJsonFile = "";
    private string jsonFileName = "";
    private bool showAdvance = false;   // �Ƿ���ʾ�߼�ѡ��

    // ����ö�٣����ڱ�ʾ�ض����͵���Դ
    public enum AssetType
    {
        Prefab, // Ԥ����
        Texture, // ����
        Material, // ����
    }

    // ����ƥ��ö��
    private enum NameConditionType
    {
        All,        // ���о���
        ExactMatch, // ��ȷƥ��
        StartsWith, // ��...��ʼ
        EndsWith,   // ��...��β
        RegexMatch  // ������ʽƥ��
    }
    private NameConditionType nameConditionType;    // ���Ʋ�����������
    private string conditionValue = ""; // ���Ʋ�������ֵ

    // ���ڴ洢�ų��ڽ��֮����ʲ����Ƶ��ַ����б�
    [SerializeField] private List<string> excludedAssetNames = new List<string>();

    public const string ASSETS_FILTER_FOLDER = "Assets/Scripts/Editor/AssetsFilter/";

    // �ṩһ����ť�Խ��ų��������б�洢ΪJSON�ļ���ʹ��LitJson�⣩
    public void SaveExcludedNamesToJson(string filename)
    {
        Directory.CreateDirectory(ASSETS_FILTER_FOLDER);

        string jsonString = JsonMapper.ToJson(excludedAssetNames);
        File.WriteAllText($"{ASSETS_FILTER_FOLDER}/{filename}.json", jsonString);
    }

    // �ṩһ����ť�Դ�JSON�ļ��м����ų��������б�
    public void LoadExcludedNamesFromJson(string fileName)
    {
        string jsonString = File.ReadAllText($"{ASSETS_FILTER_FOLDER}/{fileName}");
        excludedAssetNames = JsonMapper.ToObject<List<string>>(jsonString);
    }

    // �����ļ��м������ļ������ض����͵������ʲ����������������ͷ���
    public List<T> FindAssetsInFolder<T>(string folderPath, AssetType assetType) where T : Object
    {
        string assetTypeString = GetAssetTypeString(assetType);
        if (string.IsNullOrEmpty(folderPath) || folderPath.IndexOf("Assets") < 0)
        {
            Debug.LogWarning("ѡ����ļ���·�����ڹ�����");
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
                // �����������Ƿ�����
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

    // ���Զ���ö��ת��ΪUnity��ʶ����ַ���
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

    // ��OnGUI����ʾ������Ĺ���
    public void ShowFunction()
    {
        // ѡ���ļ���
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("�ļ���·��:", GUILayout.Width(75));
        folderPath = EditorGUILayout.TextField(folderPath);
        if (GUILayout.Button("ѡ���ļ���", GUILayout.Width(100)))
        {
            folderPath = EditorUtility.OpenFolderPanel("ѡ���ļ���", "Assets", "");
        }
        EditorGUILayout.EndHorizontal();

        showAdvance = EditorGUILayout.Foldout(showAdvance, "�߼�");
        if (!showAdvance)
        {
            GUILayout.Space(20);
            return;
        }

        // ��ʾ�ų�����Դ����
        ShowFileSaveHelpBox();
        GUILayout.Label("�ų�����Դ����:");
        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < excludedAssetNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            excludedAssetNames[i] = EditorGUILayout.TextField(excludedAssetNames[i]);
            if (GUILayout.Button("ɾ��", GUILayout.Width(50)))
            {
                excludedAssetNames.RemoveAt(i);
                selectedJsonFile = "";
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("���", GUILayout.Width(50)))
        {
            excludedAssetNames.Add("");
            selectedJsonFile = "";
        }
        if (GUILayout.Button("���", GUILayout.Width(50)))
        {
            excludedAssetNames.Clear();
            selectedJsonFile = "";
            jsonFileName = "";
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // �����ų�����Դ���Ƶ�JSON�ļ�
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("�����ų�����Դ����", GUILayout.Width(150)))
        {
            SaveExcludedNamesToJson(jsonFileName);
            selectedJsonFile = "";
        }
        GUILayout.Label("JSON�ļ���:", GUILayout.Width(75));
        jsonFileName = EditorGUILayout.TextField(string.IsNullOrEmpty(selectedJsonFile) ? jsonFileName : Path.GetFileNameWithoutExtension(selectedJsonFile));
        EditorGUILayout.EndHorizontal();

        // �����ų�����Դ�����б�
        if (GUILayout.Button("�����ų�����Դ����", GUILayout.Width(150)))
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
            GUILayout.Label($"��ѡ��: {selectedJsonFile}");
        }

        // ѡ������ɸѡ����
        ShowNameConditionTypeHelpBox();
        nameConditionType = (NameConditionType)EditorGUILayout.EnumPopup("����ɸѡ��������", nameConditionType);
        if (nameConditionType != NameConditionType.All)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("����ֵ:", GUILayout.Width(75));
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
            EditorGUILayout.HelpBox("�ɵ������ò���ʱҪ�ų�����Դ����\n" +
                "�����ļ��� ���ɽ���ǰ���õ��б���\n" +
                "�ٴδ򿪱༭��ʱ��ѡ���ļ�����", MessageType.Info);
        }
    }
    private void ShowNameConditionTypeHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("ɸѡԤ�������ƣ����б�ɸѡ����Ԥ����ᱻӰ��:\n" +
                "All: ���о���\n" +
                "ExactMatch: ��ȷƥ��\n" +
                "StartsWith: ��...��ʼ\n" +
                "EndsWith: ��...��β\n" +
                "RegexMatch: ������ʽƥ��", MessageType.Info);
        }
    }
}