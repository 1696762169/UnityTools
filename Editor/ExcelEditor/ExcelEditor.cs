using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Excel���߱༭���ű�
/// </summary>
[CustomEditor(typeof(ExcelToolGUI))]
public class ExcelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ExcelToolGUI excel = target as ExcelToolGUI;

        GUILayout.Space(10);
        if (GUILayout.Button("���ɱ���ļ�", GUILayout.Width(150)))
        {
            string dataType = excel.dataType.Replace(".", "+");
            Type type = typeof(ExcelToolGUI).Assembly.GetType(dataType);
            ExcelTools.GenerateFile(type, $"{Application.streamingAssetsPath}/{excel.fileName}.xlsx", excel.overwrite);
            Debug.Log($"�ļ�{excel.fileName}.xlsx���ɳɹ�");
        }
    }
}
