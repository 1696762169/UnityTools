using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Excel工具编辑器脚本
/// </summary>
[CustomEditor(typeof(ExcelToolGUI))]
public class ExcelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ExcelToolGUI excel = target as ExcelToolGUI;

        GUILayout.Space(10);
        if (GUILayout.Button("生成表格文件", GUILayout.Width(150)))
        {
            string dataType = excel.dataType.Replace(".", "+");
            Type type = typeof(ExcelToolGUI).Assembly.GetType(dataType);
            ExcelTools.GenerateFile(type, $"{Application.streamingAssetsPath}/{excel.fileName}.xlsx", excel.overwrite);
            Debug.Log($"文件{excel.fileName}.xlsx生成成功");
        }
    }
}
