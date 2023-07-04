using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 脚本模板生成器 根据可根据脚本模板生成脚本
/// </summary>
public static class ScriptGenerator
{
    /// <summary>
    /// 根据脚本模板生成脚本
    /// </summary>
    /// <param name="template">模板路径（从模板文件夹ScriptTemplates开始的相对路径）</param>
    /// <param name="target">脚本生成目标路径（Assets开始的相对路径）</param>
    /// <param name="dict">脚本中的参数替换表</param>
    /// <param name="overwrite">覆盖原脚本</param>
    public static void Generate(string template, string target, Dictionary<string, string> dict, bool overwrite = false)
    {
        // 检查模板是否存在
        if (string.IsNullOrWhiteSpace(template))
            return;
        if (!template.EndsWith(".txt"))
            template += ".txt";
        string templatePath = GetTemplatePath(template);
        if (!File.Exists(templatePath))
        {
            Debug.LogWarning($"未找到脚本模板：{template}");
            return;
        }

        // 检查是否能够生成
        if (string.IsNullOrWhiteSpace(target))
            return;
        if (!target.EndsWith(".cs"))
            target += ".cs";
        string targetPath = GetTargetPath(target);
        if (!overwrite && File.Exists(targetPath))
        {
            Debug.LogWarning($"无法生成脚本：{target}，该路径已有同名脚本");
            return;
        }

        // 生成脚本路径
        string directory = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // 生成脚本
        File.WriteAllText(targetPath, FillTemplate(File.ReadAllText(templatePath), dict));
        Debug.Log($"脚本{target}已成功生成于{targetPath}！");
    }

    // 将模板字符串中的参数替换为实际的值并返回
    private static string FillTemplate(string template, Dictionary<string, string> dict)
    {
        string ret = template;
        foreach (string key in dict.Keys)
        {
            ret = ret.Replace($"${key}$", dict[key]);
        }
        return ret;
    }

    // 将Assets开始的相对路径转为绝对路径
    public static string GetTargetPath(string path)
    {
        string ret = $"{Application.dataPath}/{path}";
        if (!ret.EndsWith(".cs"))
            ret += ".cs";
        return ret;
    }
    // 将模板文件夹ScriptTemplates开始的相对路径转为绝对路径
    public static string GetTemplatePath(string path)
    {
        const string TEMPLATE_FOLDER = "ScriptTemplates";
        string[] paths = AssetDatabase.FindAssets(TEMPLATE_FOLDER, new string[] { "Assets/Scripts" });
        if (paths.Length == 0)
            throw new System.Exception("未找到模板文件夹" + TEMPLATE_FOLDER);
        string folder = AssetDatabase.GUIDToAssetPath(paths[0]);
        return Path.Join(folder, path);
    }
}
