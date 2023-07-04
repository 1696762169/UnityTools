using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;


/// <summary>
/// Excel���������
/// </summary>
public class ExcelGeneratorEditorWindow : EditorWindow
{
    // UI����
    private string dataType;
    private string fileName;
    private bool overwrite;

    // ��Ӳ˵����Դ򿪴���
    [MenuItem("Tools/Excel������")]
    public static void ShowWindow()
    {
        GetWindow<ExcelGeneratorEditorWindow>("Excel������");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // ����UI
        dataType = EditorGUILayout.TextField("���������Ķ�������", dataType);
        fileName = EditorGUILayout.TextField("�������ı���ļ���", fileName);
        overwrite = EditorGUILayout.Toggle("�Ƿ񸲸�ԭ�ļ�", overwrite);

        EditorGUILayout.Space();

        // ��������
        Type type = null;
        if (!string.IsNullOrEmpty(dataType))
        {
            string dataTypeFullName = dataType.Replace(".", "+");
            type = typeof(ExcelToolGUI).Assembly.GetType(dataTypeFullName);
            if (type == null)
                EditorGUILayout.LabelField($"δ�ҵ���Ϊ��{dataType}��������");
        }
        EditorGUILayout.Space();

        if (type != null && GUILayout.Button("���ɱ���ļ�", GUILayout.Width(150)))
        {
            ExcelTools.GenerateFile(type, $"{Application.streamingAssetsPath}/{fileName}.xlsx", overwrite);
            Debug.Log($"�ļ�{fileName}.xlsx���ɳɹ�");
        }
    }
}