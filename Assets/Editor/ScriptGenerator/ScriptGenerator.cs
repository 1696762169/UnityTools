using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// �ű�ģ�������� ���ݿɸ��ݽű�ģ�����ɽű�
/// </summary>
public static class ScriptGenerator
{
    /// <summary>
    /// ���ݽű�ģ�����ɽű�
    /// </summary>
    /// <param name="template">ģ��·������ģ���ļ���ScriptTemplates��ʼ�����·����</param>
    /// <param name="target">�ű�����Ŀ��·����Assets��ʼ�����·����</param>
    /// <param name="dict">�ű��еĲ����滻��</param>
    /// <param name="overwrite">����ԭ�ű�</param>
    public static void Generate(string template, string target, Dictionary<string, string> dict, bool overwrite = false)
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
    private static string FillTemplate(string template, Dictionary<string, string> dict)
    {
        string ret = template;
        foreach (string key in dict.Keys)
        {
            ret = ret.Replace($"${key}$", dict[key]);
        }
        return ret;
    }

    // ��Assets��ʼ�����·��תΪ����·��
    public static string GetTargetPath(string path)
    {
        string ret = $"{Application.dataPath}/{path}";
        if (!ret.EndsWith(".cs"))
            ret += ".cs";
        return ret;
    }
    // ��ģ���ļ���ScriptTemplates��ʼ�����·��תΪ����·��
    public static string GetTemplatePath(string path)
    {
        const string TEMPLATE_FOLDER = "ScriptTemplates";
        string[] paths = AssetDatabase.FindAssets(TEMPLATE_FOLDER, new string[] { "Assets/Scripts" });
        if (paths.Length == 0)
            throw new System.Exception("δ�ҵ�ģ���ļ���" + TEMPLATE_FOLDER);
        string folder = AssetDatabase.GUIDToAssetPath(paths[0]);
        return Path.Join(folder, path);
    }
}
