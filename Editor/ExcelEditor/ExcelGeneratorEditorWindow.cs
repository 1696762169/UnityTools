using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;


/// <summary>
/// Excel表格生成器
/// </summary>
public class ExcelGeneratorEditorWindow : EditorWindow
{
    // UI属性
    private string dataType;
    private string fileName;
    private bool overwrite;

    // 添加菜单项以打开窗口
    [MenuItem("Tools/Excel生成器")]
    public static void ShowWindow()
    {
        GetWindow<ExcelGeneratorEditorWindow>("Excel生成器");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // 绘制UI
        dataType = EditorGUILayout.TextField("待创建表格的对象类型", dataType);
        fileName = EditorGUILayout.TextField("待创建的表格文件名", fileName);
        overwrite = EditorGUILayout.Toggle("是否覆盖原文件", overwrite);

        EditorGUILayout.Space();

        // 查找类型
        Type type = null;
        if (!string.IsNullOrEmpty(dataType))
        {
            string dataTypeFullName = dataType.Replace(".", "+");
            type = typeof(ExcelToolGUI).Assembly.GetType(dataTypeFullName);
            if (type == null)
                EditorGUILayout.LabelField($"未找到名为【{dataType}】的类型");
        }
        EditorGUILayout.Space();

        if (type != null && GUILayout.Button("生成表格文件", GUILayout.Width(150)))
        {
            ExcelTools.GenerateFile(type, $"{Application.streamingAssetsPath}/{fileName}.xlsx", overwrite);
            Debug.Log($"文件{fileName}.xlsx生成成功");
        }
    }
}