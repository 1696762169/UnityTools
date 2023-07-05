using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 使用脚本生成器的编辑器脚本
/// </summary>
[CustomEditor(typeof(ScriptGeneratorGUI))]
public class ScriptGeneratorEditor : Editor
{
    private bool m_Ensure = false;
    private readonly string[] m_Content = { "确认", "取消" };

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

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);
        if (GUILayout.Button("生成脚本") || m_Ensure)
        {
            ScriptGeneratorGUI sg = target as ScriptGeneratorGUI;

            // 确认是否需要覆盖
            bool execute = !File.Exists(ScriptGenerator.GetTargetPath(sg.target));
            if (!execute)
            {
                if (!sg.overwrite)
                {
                    Debug.LogWarning($"无法生成脚本：{sg.target}，该路径已有同名脚本");
                    return;
                }
                else
                {
                    m_Ensure = true;
                    GUILayout.Space(10);
                    GUILayout.Label("确认要用模板覆盖已有的代码吗？");
                    switch (GUILayout.SelectionGrid(-1, m_Content, 2, GUILayout.Width(150)))
                    {
                        case 0:
                            m_Ensure = false;
                            execute = true;
                            break;
                        case 1:
                            m_Ensure = false;
                            break;
                    }
                }
            }
            
            // 生成代码
            if (execute)
            {
                // 生成模板参数替换字典
                Dictionary<string, string> dict = new Dictionary<string, string>
                {
                    { "ShortClassName", sg.shortClassName },
                    { "ClassComment", sg.classComment },
                };
                ScriptGenerator.Generate(m_TemplatePath[sg.template], sg.target, dict, sg.overwrite);
            }
        }
    }
}
