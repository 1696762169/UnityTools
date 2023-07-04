using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// �ű�����������
/// </summary>
public class ScriptGeneratorEditorWindow : EditorWindow
{
    // UI ����
    private ScriptTemplateType template;
    private string target = "Scripts/";
    private bool overwrite;

    // ��������ʾ����
    private bool showHelpBox;

    // �ű��滻����
    private string shortClassName;
    private string classComment;

    // ȷ�ϱ���������
    private bool m_Ensure = false;

    // ģ��·��
    private Dictionary<ScriptTemplateType, string> m_TemplatePath = new Dictionary<ScriptTemplateType, string>()
    {
        { ScriptTemplateType.ReadOnlyData, "ReadOnlyData" },
        { ScriptTemplateType.ReadOnlyDB, "ReadOnlyDB" },
        { ScriptTemplateType.RuntimeData, "RuntimeData" },
        { ScriptTemplateType.RuntimeMgr, "RuntimeMgr" },
        { ScriptTemplateType.Panel, "Panel" },
        { ScriptTemplateType.Editor, "Editor" },
        { ScriptTemplateType.GlobalConfigXlsx, "GlobalConfigXlsx" },
        { ScriptTemplateType.GlobalConfigJson, "GlobalConfigJson" },
        { ScriptTemplateType.Enum, "Enum" },
    };

    // ��Ӳ˵����Դ򿪴���
    [MenuItem("Tools/�ű�������")]
    public static void ShowWindow()
    {
        GetWindow<ScriptGeneratorEditorWindow>("�ű�������");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // ���� UI
        template = (ScriptTemplateType)EditorGUILayout.EnumPopup("ģ������", template);
        GUILayout.Label("�ű�����Ŀ��·����Assets��ʼ�����·����");
        target = EditorGUILayout.TextField(target);
        overwrite = EditorGUILayout.Toggle("�Ƿ񸲸�ԭ�����ļ�", overwrite);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("����Ϊģ�����", EditorStyles.boldLabel);
        shortClassName = EditorGUILayout.TextField("������д", shortClassName);
        classComment = EditorGUILayout.TextField("��ע��", classComment);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("���ɽű�") || m_Ensure)
        {
            // ���Ŀ���ļ��Ƿ����
            bool execute = !File.Exists(ScriptGenerator.GetTargetPath(target));

            // ��ʾ���沢Ҫ��ȷ��
            if (!execute)
            {
                if (!overwrite)
                {
                    Debug.LogWarning($"�޷����ɽű���{target}����·������ͬ���ű�");
                    return;
                }
                else
                {
                    m_Ensure = true;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("ȷ��Ҫ��ģ�帲�����еĴ�����", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("ȷ��"))
                    {
                        m_Ensure = false;
                        execute = true;
                    }
                    if (GUILayout.Button("ȡ��"))
                    {
                        m_Ensure = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            // ���ɴ���
            if (execute)
            {
                // ����ģ������滻�ֵ�
                Dictionary<string, string> dict = new Dictionary<string, string>
                {
                    { "ShortClassName", shortClassName },
                    { "ClassComment", classComment },
                };
                Generate(m_TemplatePath[template], target, dict, overwrite);
            }
        }

        // ��������ʾ���ƿ���
        showHelpBox = EditorGUILayout.Toggle("��ʾģ������˵��", showHelpBox);
        // ����������
        if (showHelpBox)
        {
            EditorGUILayout.HelpBox("ReadOnlyData: ֻ��������\n" +
                                    "ReadOnlyDB: ֻ�����ݿ�\n" +
                                    "RuntimeData: ����ʱ����\n" +
                                    "RuntimeMgr: ����ʱ���ݹ�����\n" +
                                    "Panel: UI�����\n" +
                                    "Editor: �༭���ű�\n" +
                                    "GlobalConfigXlsx: �����ȫ��������\n" +
                                    "GlobalConfigJson: Json��ȫ��������\n" +
                                    "Enum: ö��", MessageType.Info);
        }
    }

    /// <summary>
    /// ���ݽű�ģ�����ɽű�
    /// </summary>
    /// <param name="template">ģ��·������ģ���ļ���ScriptTemplates��ʼ�����·����</param>
    /// <param name="target">�ű�����Ŀ��·����Assets��ʼ�����·����</param>
    /// <param name="dict">�ű��еĲ����滻��</param>
    /// <param name="overwrite">����ԭ�ű�</param>
    private void Generate(string template, string target, Dictionary<string, string> dict, bool overwrite = false)
    {
        // ���ģ���Ƿ����
        if (string.IsNullOrWhiteSpace(template))
            return;
        if (!template.EndsWith(".txt"))
            template += ".txt";
        string templatePath = GetTemplatePath(template);
        if (!File.Exists(templatePath))
        {
            Debug.LogWarning($"δ�ҵ��ű�ģ�壺{template}");
            return;
        }

        // ����Ƿ��ܹ�����
        if (string.IsNullOrWhiteSpace(target))
            return;
        if (!target.EndsWith(".cs"))
            target += ".cs";
        string targetPath = GetTargetPath(target);
        if (!overwrite && File.Exists(targetPath))
        {
            Debug.LogWarning($"�޷����ɽű���{target}����·������ͬ���ű�");
            return;
        }

        // ���ɽű�·��
        string directory = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // ���ɽű�
        File.WriteAllText(targetPath, FillTemplate(File.ReadAllText(templatePath), dict));
        Debug.Log($"�ű�{target}�ѳɹ�������{targetPath}��");
    }

    // ��ģ���ַ����еĲ����滻Ϊʵ�ʵ�ֵ������
    private string FillTemplate(string template, Dictionary<string, string> dict)
    {
        string ret = template;
        foreach (string key in dict.Keys)
        {
            ret = ret.Replace($"${key}$", dict[key]);
        }
        return ret;
    }

    // ��Assets��ʼ�����·��תΪ����·��
    private string GetTargetPath(string path)
    {
        string ret = $"{Application.dataPath}/{path}";
        if (!ret.EndsWith(".cs"))
            ret += ".cs";
        return ret;
    }
    // ��ģ���ļ���ScriptTemplates��ʼ�����·��תΪ����·��
    private string GetTemplatePath(string path)
    {
        const string TEMPLATE_FOLDER = "ScriptTemplates";
        string[] paths = AssetDatabase.FindAssets(TEMPLATE_FOLDER, new string[] { "Assets/Scripts" });
        if (paths.Length == 0)
            throw new System.Exception("δ�ҵ�ģ���ļ���" + TEMPLATE_FOLDER);
        string folder = AssetDatabase.GUIDToAssetPath(paths[0]);
        return Path.Join(folder, path);
    }
}
