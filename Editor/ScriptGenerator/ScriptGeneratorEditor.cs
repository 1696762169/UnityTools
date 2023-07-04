using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// ʹ�ýű��������ı༭���ű�
/// </summary>
[CustomEditor(typeof(ScriptGeneratorGUI))]
public class ScriptGeneratorEditor : Editor
{
    protected bool m_Ensure = false;
    protected string[] m_Content = new string[] { "ȷ��", "ȡ��" };

    // ģ��·��
    protected Dictionary<ScriptTemplateType, string> m_TemplatePath = new Dictionary<ScriptTemplateType, string>()
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
        if (GUILayout.Button("���ɽű�") || m_Ensure)
        {
            ScriptGeneratorGUI sg = target as ScriptGeneratorGUI;

            // ȷ���Ƿ���Ҫ����
            bool execute = !File.Exists(ScriptGenerator.GetTargetPath(sg.target));
            if (!execute)
            {
                if (!sg.overwrite)
                {
                    Debug.LogWarning($"�޷����ɽű���{sg.target}����·������ͬ���ű�");
                    return;
                }
                else
                {
                    m_Ensure = true;
                    GUILayout.Space(10);
                    GUILayout.Label("ȷ��Ҫ��ģ�帲�����еĴ�����");
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
            
            // ���ɴ���
            if (execute)
            {
                // ����ģ������滻�ֵ�
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
