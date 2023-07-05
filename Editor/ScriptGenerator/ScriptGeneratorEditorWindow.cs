using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

/// <summary>
/// 脚本生成器窗口
/// </summary>
public class ScriptGeneratorEditorWindow : EditorWindow
{
    // UI 属性
    private ScriptTemplateType m_Template;
    private string m_Target = "Scripts/";
    private bool m_Overwrite;

    // 帮助框显示控制
    private bool m_ShowHelpBox;

    // 脚本替换参数
    private string m_ShortClassName;
    private string m_ClassComment;

    // 确认变量和内容
    private bool m_Ensure = false;

    // 模板路径
    private readonly Dictionary<ScriptTemplateType, string> m_TemplatePath = new()
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

    // 添加菜单项以打开窗口
    [MenuItem("Tools/脚本生成器")]
    public static void ShowWindow()
    {
        GetWindow<ScriptGeneratorEditorWindow>("脚本生成器");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // 绘制 UI
        m_Template = (ScriptTemplateType)EditorGUILayout.EnumPopup("模板类型", m_Template);
        GUILayout.Label("脚本生成目标路径（Assets开始的相对路径）");
        m_Target = EditorGUILayout.TextField(m_Target);
        m_Overwrite = EditorGUILayout.Toggle("是否覆盖原代码文件", m_Overwrite);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("以下为模板参数", EditorStyles.boldLabel);
        m_ShortClassName = EditorGUILayout.TextField("类名简写", m_ShortClassName);
        m_ClassComment = EditorGUILayout.TextField("类注释", m_ClassComment);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("生成脚本") || m_Ensure)
        {
            // 检查目标文件是否存在
            bool execute = !File.Exists(ScriptGenerator.GetTargetPath(m_Target));

            // 显示警告并要求确认
            if (!execute)
            {
                if (!m_Overwrite)
                {
                    Debug.LogWarning($"无法生成脚本：{m_Target}，该路径已有同名脚本");
                    return;
                }
                else
                {
                    m_Ensure = true;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("确认要用模板覆盖已有的代码吗？", EditorStyles.boldLabel);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("确认"))
                    {
                        m_Ensure = false;
                        execute = true;
                    }
                    if (GUILayout.Button("取消"))
                    {
                        m_Ensure = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            // 生成代码
            if (execute)
            {
                // 创建模板参数替换字典
                Dictionary<string, string> dict = new()
                {
                    { "ShortClassName", m_ShortClassName },
                    { "ClassComment", m_ClassComment },
                };
                Generate(m_TemplatePath[m_Template], m_Target, dict, m_Overwrite);
            }
        }

        // 帮助框显示控制开关
        m_ShowHelpBox = EditorGUILayout.Toggle("显示模板类型说明", m_ShowHelpBox);
        // 帮助框内容
        if (m_ShowHelpBox)
        {
            EditorGUILayout.HelpBox("ReadOnlyData: 只读数据类\n" +
                                    "ReadOnlyDB: 只读数据库\n" +
                                    "RuntimeData: 运行时数据\n" +
                                    "RuntimeMgr: 运行时数据管理器\n" +
                                    "Panel: UI面板类\n" +
                                    "Editor: 编辑器脚本\n" +
                                    "GlobalConfigXlsx: 表格型全局配置类\n" +
                                    "GlobalConfigJson: Json型全局配置类\n" +
                                    "Enum: 枚举", MessageType.Info);
        }
    }

    /// <summary>
    /// 根据脚本模板生成脚本
    /// </summary>
    /// <param name="template">模板路径（从模板文件夹ScriptTemplates开始的相对路径）</param>
    /// <param name="target">脚本生成目标路径（Assets开始的相对路径）</param>
    /// <param name="dict">脚本中的参数替换表</param>
    /// <param name="overwrite">覆盖原脚本</param>
    private void Generate(string template, string target, Dictionary<string, string> dict, bool overwrite = false)
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
    private string FillTemplate(string template, Dictionary<string, string> dict)
    {
	    return dict.Keys.Aggregate(template, (current, key) => current.Replace($"${key}$", dict[key]));
    }

    // 将Assets开始的相对路径转为绝对路径
    private string GetTargetPath(string path)
    {
        string ret = $"{Application.dataPath}/{path}";
        if (!ret.EndsWith(".cs"))
            ret += ".cs";
        return ret;
    }
    // 将模板文件夹ScriptTemplates开始的相对路径转为绝对路径
    private string GetTemplatePath(string path)
    {
        const string TEMPLATE_FOLDER = "ScriptTemplates";
        string[] paths = AssetDatabase.FindAssets(TEMPLATE_FOLDER, new[] { "Assets/Editor" });
        if (paths.Length == 0)
            throw new System.Exception("未找到模板文件夹" + TEMPLATE_FOLDER);
        string folder = AssetDatabase.GUIDToAssetPath(paths[0]);
        return Path.Join(folder, path);
    }
}
