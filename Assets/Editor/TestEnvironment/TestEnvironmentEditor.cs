using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class TestEnvironmentEditor : OdinEditorWindow
{
	public static string ResourcesPath => Application.dataPath + "/Resources";
	public static string EnvironmentDir => Path.Combine(Application.dataPath, "Editor/Test/TestEnvironment");

	// 需要一个文件记录当前选中的环境名称 null表示没有选中
	public static string CurrentFilePath => Path.Combine(EnvironmentDir, "Current.txt");
	private const string NO_CURRENT_TIP = "null";

	// 当前选中的环境对象
	private TestEnvironment m_CurEnv;
	private bool m_Loading = true;

	// 所有环境的文件名列表 包含扩展名
	private List<string> m_AllEnvNames = new List<string>();
	private bool HaveEnv => m_AllEnvNames.Count > 0;

	#region 选择环境

	[LabelText("当前测试环境")]
	[ShowIf(nameof(HaveEnv)), ShowInInspector]
	[ValueDropdown(nameof(GetSelectDropdownItems))]
	[InfoBox("测试环境加载中...", InfoMessageType.Warning, nameof(m_Loading))]
	public string SelectedName
	{
		get => m_SelectedName;
		set
		{
			m_SelectedName = value;
			SetCurrentEnv();
		}
	}
	private string m_SelectedName;
	private IEnumerable<ValueDropdownItem<string>> GetSelectDropdownItems()
	{
		yield return new ValueDropdownItem<string>("不启用测试环境", NO_CURRENT_TIP);
		foreach (string envName in m_AllEnvNames)
			yield return new ValueDropdownItem<string>(Path.GetFileNameWithoutExtension(envName), envName);
	}

	private void SetCurrentEnv()
	{
		// 保存之前的环境
		SaveCurrentEnvironment();
		// 将新选中的环境写入文件
		SetCurrentEnvironment(m_SelectedName);
		// 更改当前环境
		if (m_SelectedName != NO_CURRENT_TIP)
		{
			if (m_CurEnv == null || m_CurEnv.FileName != m_SelectedName)
				LoadCurrentEnv();
		}
		else
		{
			m_CurEnv = null;
		}
	}

	private TestEnvironment LoadCurrentEnv()
	{
		m_CurEnv = LoadCurrentEnvironment();
		return m_CurEnv;
	}

	#endregion

	#region 创建环境
	[Button("创建默认测试环境")]
	[HideIf(nameof(HaveEnv))]
	[InfoBox("未找到任何测试环境文件")]
	public void CreateDefaultEnv()
	{
		m_CurEnv = CreateEnvironment(TestEnvironment.DEFAULT_FILE_NAME);
		SelectedName = m_CurEnv.FileName;
	}

	[LabelText("新环境名称")]
	[ShowIf(nameof(HaveEnv)), HorizontalGroup(nameof(CreateNewEnv))]
	public string newEnvName;

	[Button("创建环境")]
	[ShowIf(nameof(HaveEnv)), HorizontalGroup(nameof(CreateNewEnv))]
	public void CreateNewEnv()
	{
		if (string.IsNullOrWhiteSpace(newEnvName) || m_AllEnvNames.Contains(GetNameWithExtension(newEnvName)))
			return;
		CreateEnvironment(newEnvName);
		if (SelectedName == NO_CURRENT_TIP)
			SelectedName = newEnvName;
	}
	#endregion

	private bool HaveCurEnv => m_SelectedName != NO_CURRENT_TIP && m_CurEnv != null && !m_Loading;
	[LabelText("环境名称")]
	[DelayedProperty]
	[ShowInInspector, ShowIf(nameof(HaveCurEnv))]
	[PropertySpace(SpaceBefore = 10)]
	public string ChangeableName
	{
		get => m_CurEnv?.FileName;
		set
		{
			if (!HaveCurEnv || // 只允许更改当前环境
				string.IsNullOrWhiteSpace(value) || // 不允许名称为空
				m_AllEnvNames.Contains(GetNameWithExtension(value)))    // 判断是否与其它环境重复
				return;
			SetEnvironmentName(m_CurEnv.FileName, value);
		}
	}

	[Button("保存当前环境", ButtonSizes.Medium), ShowIf(nameof(HaveCurEnv))]
	[PropertySpace(SpaceAfter = 10), GUIColor(1.0f, 0.85f, 0.35f)]
	public void SaveCurrentEnvironment()
	{
		if (m_CurEnv == null)
			return;
		EditorUtility.SetDirty(m_CurEnv);
		AssetDatabase.SaveAssetIfDirty(m_CurEnv);
	}

	[MenuItem("Tools/测试环境配置")]
	public static void ShowWindow()
	{
		GetWindow<TestEnvironmentEditor>("测试环境配置");
	}

	protected override void Initialize()
	{
		m_AllEnvNames = GetAllEnvName().ToList();

		m_Loading = true;

		LoadCurrentEnv();
		SelectedName = m_CurEnv?.FileName ?? NO_CURRENT_TIP;
		m_Loading = false;
	}

	protected override IEnumerable<object> GetTargets()
	{
		yield return this;
		if (!m_Loading && m_SelectedName != null && m_SelectedName != NO_CURRENT_TIP)
			yield return m_CurEnv ?? LoadCurrentEnv();
	}

	protected override void OnDestroy()
	{
		SaveCurrentEnvironment();
		base.OnDestroy();
	}

	#region 测试环境文件操作
	/// <summary>
	/// 根据选中记录 加载测试环境
	/// </summary>
	/// <returns>可能为null 表示未使用测试环境</returns>
	public static TestEnvironment LoadCurrentEnvironment()
	{
		// 默认存一个null在文件里
		if (!File.Exists(CurrentFilePath))
		{
			SetCurrentEnvironment(NO_CURRENT_TIP);
			return null;
		}

		string currentName = File.ReadAllText(CurrentFilePath);
		if (currentName == NO_CURRENT_TIP)
			return null;

		foreach (string name in GetAllEnvName())
		{
			if (name == currentName)
			{
				// 加载环境文件
				string path = GetEnvPath(name);
				if (!File.Exists(path) || Path.GetExtension(path) != ".asset")
					return null;

				TestEnvironment env = AssetDatabase.LoadAssetAtPath<TestEnvironment>(EditorTools.ToAssetPath(path));
				env.FileName = Path.GetFileNameWithoutExtension(path);
				return env;
			}
		}
		SetCurrentEnvironment(NO_CURRENT_TIP);
		return null;
	}

	/// <summary>
	/// 创建一个环境
	/// </summary>
	/// <param name="envName">环境名称 若重名则会添加后缀</param>
	public TestEnvironment CreateEnvironment(string envName)
	{
		envName = Path.GetFileNameWithoutExtension(envName);
		string realName = GetNameWithExtension(envName);
		string filePath = GetEnvPath(realName);
		int index = 1;
		while (File.Exists(filePath))
		{
			realName = GetNameWithExtension($"{envName} {index++}");
			filePath = GetEnvPath(realName);
		}

		TestEnvironment env = CreateInstance<TestEnvironment>();
		env.FileName = Path.GetFileNameWithoutExtension(realName);
		if (!Directory.Exists(Path.GetDirectoryName(filePath)))
			Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
		AssetDatabase.CreateAsset(env, EditorTools.ToAssetPath(filePath));
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		m_AllEnvNames.Add(realName);
		return env;
	}

	/// <summary>
	/// 更改环境名称
	/// </summary>
	public bool SetEnvironmentName(string oldName, string newName)
	{
		oldName = GetNameWithExtension(oldName);
		newName = GetNameWithExtension(newName);
		if (!m_AllEnvNames.Contains(oldName) || !File.Exists(GetEnvPath(oldName)))
			return false;

		AssetDatabase.MoveAsset(
			EditorTools.ToAssetPath(GetEnvPath(oldName)),
			EditorTools.ToAssetPath(GetEnvPath(newName)));
		m_AllEnvNames[m_AllEnvNames.IndexOf(oldName)] = newName;
		SelectedName = Path.GetFileNameWithoutExtension(newName);
		return true;
	}

	#endregion

	#region 工具函数
	public static void SetCurrentEnvironment(string name)
	{
		if (name != NO_CURRENT_TIP)
			name = GetNameWithExtension(name);
		File.WriteAllText(CurrentFilePath, name);
	}

	public static IEnumerable<string> GetAllEnvName()
	{
		if (!Directory.Exists(EnvironmentDir))
			Directory.CreateDirectory(EnvironmentDir);
		DirectoryInfo environmentDir = new DirectoryInfo(EnvironmentDir);
		FileInfo[] testEnvironmentFiles = environmentDir.GetFiles("*.asset", SearchOption.AllDirectories);
		return testEnvironmentFiles.Select(file => file.Name);
	}

	public static string GetEnvPath(string envName) => Path.Combine(EnvironmentDir, GetNameWithExtension(envName));
	public static string GetNameWithExtension(string envName) => envName.EndsWith(".asset") ? envName : envName + ".asset";
	#endregion

	[InitializeOnEnterPlayMode]
	public static void InjectLoadFunction()
	{
		TestEnvironment.LoadCurrentEnvironment -= LoadCurrentEnvironment;
		TestEnvironment.LoadCurrentEnvironment += LoadCurrentEnvironment;
	}
}

