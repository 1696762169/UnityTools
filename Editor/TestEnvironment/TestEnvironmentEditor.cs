using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TestEnvironmentEditor : EditorWindow
{
    private TestEnvironment m_TestEnvironment;
    private SerializedObject m_SerializedTestEnvironment;
    private string[] m_TestEnvironmentNames;
    private int m_SelectedIndex = -1;
    public const string DEFAULT_RESOURCE_NAME = "TestEnvironment\\DefaultTestEnvironment";

    [MenuItem("Tools/���Ի�������")]
    public static void ShowWindow()
    {
        GetWindow<TestEnvironmentEditor>("���Ի�������");
    }

    private void OnEnable()
    {
        LoadTestEnvironmentNames();
        SelectCurrentResource();
    }

    private void LoadTestEnvironmentNames()
    {
        string resourcesPath = Application.dataPath + "/Resources";
        DirectoryInfo resourcesDirectory = new DirectoryInfo(resourcesPath);
        FileInfo[] testEnvironmentFiles = resourcesDirectory.GetFiles("*.asset", SearchOption.AllDirectories);

        m_TestEnvironmentNames = new string[testEnvironmentFiles.Length];
        for (int i = 0; i < testEnvironmentFiles.Length; i++)
        {
            string temp = Path.GetRelativePath(resourcesPath, testEnvironmentFiles[i].FullName);
            m_TestEnvironmentNames[i] = Path.Combine(Path.GetDirectoryName(temp), Path.GetFileNameWithoutExtension(temp));
        }
    }

    private void SelectCurrentResource()
    {
        int index = -1;
        if (TestEnvironment.CurrentPath != null)
            index = System.Array.IndexOf(m_TestEnvironmentNames, TestEnvironment.CurrentPath);
        else
            index = System.Array.IndexOf(m_TestEnvironmentNames, DEFAULT_RESOURCE_NAME);
        if (index >= 0)
        {
            LoadTestEnvironmentAtIndex(index);
        }
    }

    private void LoadTestEnvironmentAtIndex(int index)
    {
        m_SelectedIndex = index;
        m_TestEnvironment = Resources.Load<TestEnvironment>(m_TestEnvironmentNames[index]);
        m_SerializedTestEnvironment = new SerializedObject(m_TestEnvironment);

        // ���� TestEnvironment.Current ���Ե�ֵ
        TestEnvironment.SetCurrent(m_TestEnvironmentNames[index]);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("ѡ����Ի���", EditorStyles.boldLabel);
        int newIndex = EditorGUILayout.Popup(m_SelectedIndex, m_TestEnvironmentNames);

        if (newIndex != m_SelectedIndex)
        {
            LoadTestEnvironmentAtIndex(newIndex);
        }

        if (m_SerializedTestEnvironment == null)
        {
            EditorGUILayout.HelpBox($"Ĭ����Դ \"{DEFAULT_RESOURCE_NAME}\" �����ڡ���������б���ѡ��һ�����Ի�����", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        FieldInfo[] fields = typeof(TestEnvironment).GetFields(bindingFlags);

        foreach (FieldInfo field in fields)
        {
            SerializedProperty property = m_SerializedTestEnvironment.FindProperty(field.Name);
            if (property != null)
            {
                TooltipAttribute tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                GUIContent label = tooltipAttribute != null ? new GUIContent(tooltipAttribute.tooltip) : new GUIContent(ObjectNames.NicifyVariableName(field.Name));
                EditorGUILayout.PropertyField(property, label);
            }
        }

        m_SerializedTestEnvironment.ApplyModifiedProperties();
    }
}
