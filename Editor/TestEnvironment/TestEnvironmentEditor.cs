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

    [MenuItem("Tools/测试环境配置")]
    public static void ShowWindow()
    {
        GetWindow<TestEnvironmentEditor>("测试环境配置");
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

        // 更新 TestEnvironment.Current 属性的值
        TestEnvironment.SetCurrent(m_TestEnvironmentNames[index]);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("选择测试环境", EditorStyles.boldLabel);
        int newIndex = EditorGUILayout.Popup(m_SelectedIndex, m_TestEnvironmentNames);

        if (newIndex != m_SelectedIndex)
        {
            LoadTestEnvironmentAtIndex(newIndex);
        }

        if (m_SerializedTestEnvironment == null)
        {
            EditorGUILayout.HelpBox($"默认资源 \"{DEFAULT_RESOURCE_NAME}\" 不存在。请从下拉列表中选择一个测试环境。", MessageType.Warning);
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
