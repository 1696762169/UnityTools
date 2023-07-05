using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;


/// <summary>
/// Excel表格生成器
/// </summary>
public class ExcelEditorWindow : EditorWindow
{
    // UI属性
    private string m_DataType;
    private string m_FileName;
    private bool m_Overwrite;

    // 添加菜单项以打开窗口
    [MenuItem("Tools/Excel生成器")]
    public static void ShowWindow()
    {
        GetWindow<ExcelEditorWindow>("Excel生成器");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // 绘制UI
        m_DataType = EditorGUILayout.TextField("待创建表格的对象类型", m_DataType);
        m_FileName = EditorGUILayout.TextField("待创建的表格文件名", m_FileName);
        m_Overwrite = EditorGUILayout.Toggle("是否覆盖原文件", m_Overwrite);

        EditorGUILayout.Space();

        // 查找类型
        Type type = null;
        if (!string.IsNullOrEmpty(m_DataType))
        {
            string dataTypeFullName = m_DataType.Replace(".", "+");
            type = typeof(GameManager).Assembly.GetType(dataTypeFullName);
            if (type == null)
                EditorGUILayout.LabelField($"未找到名为【{m_DataType}】的类型");
        }
        EditorGUILayout.Space();

        if (type != null && GUILayout.Button("生成表格文件", GUILayout.Width(150)))
        {
            string fileName = string.IsNullOrWhiteSpace(m_FileName) ? type.Name.Replace("Raw", "") + "s" : m_FileName;
            ExcelEditor.GenerateFile(type, $"{Application.streamingAssetsPath}/{fileName}.xlsx", m_Overwrite);
            Debug.Log($"文件{fileName}.xlsx生成成功");
        }
    }
}