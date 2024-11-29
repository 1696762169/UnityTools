#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities.Editor;
using UnityEngine.Rendering;
using System.Runtime.Serialization;

[Serializable]
[CreateAssetMenu(menuName = "测试环境文件", fileName = DEFAULT_FILE_NAME)]
public class TestEnvironment : ScriptableObject
{
	public const string DEFAULT_FILE_NAME = "DefaultTestEnvironment";
	// 由GameManager在启动时直接设置
	public static TestEnvironment Current
	{
		get
		{
			if (m_Release || m_Current != null)
				return m_Current;
			m_Current = LoadCurrentEnvironment?.Invoke();
			if (m_Current == null)
			{
				m_Release = true;
				m_Current = CreateInstance<TestEnvironment>();
			}
			return m_Current;
		}
	}

	private static bool m_Release;
	private static TestEnvironment m_Current;
	public static event Func<TestEnvironment> LoadCurrentEnvironment; 

	// 当前的环境文件名 不包括扩展名
	public string FileName { get; set; }

	#region 配置表更改部分
	[Serializable]
	public class PropertyData
	{
		[LabelText("属性名称")] public string name;
		[LabelText("属性值")] public string value;
	}
	[Serializable]
	public class PropertyList
	{
		public List<PropertyData> list  = new();
	}

	[LabelText("全局配置更改"), PropertySpace(10), PropertyOrder(1), ConfigModifier]
	public SerializedDictionary<string, PropertyList> configModifier = new();
	private class ConfigModifierAttribute : Attribute { }
	private class ConfigModifierDrawer : OdinAttributeDrawer<ConfigModifierAttribute, SerializedDictionary<string, PropertyList>>
	{
		private bool m_TitleExtend = true;
		private readonly HashSet<string> m_ExtendClass = new();

		private Assembly m_Assembly;
		private readonly List<string> m_ClassOptions = new();
		private readonly Dictionary<string, List<string>> m_PropertyOptions = new();

		private const int BTN_WIDTH = 100;
		protected override void Initialize()
		{
			Type baseType = typeof(GlobalConfigXlsx<>);
			m_Assembly = baseType.Assembly;

			// 记录全局配置类
			List<Type> temp = new();
			foreach (Type type in m_Assembly.GetTypes())
			{
				if (type.BaseType?.Name != baseType.Name)
					continue;
				temp.Add(type);
				m_ClassOptions.Add(type.Name);
			}

			// 记录类型属性
			HashSet<string> ignore = baseType.GetProperties().Select(property => property.Name).ToHashSet();
			ignore.Add(nameof(IUnique.ID));
			foreach (Type type in temp)
			{
				List<string> propertyList = type.GetProperties()
					.Select(property => property.Name)
					.Where(name => !ignore.Contains(name))
					.ToList();
				m_PropertyOptions.Add(type.Name, propertyList);
			}
			//ValueEntry.SmartValue.Clear();
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			label.text += $"(点击标题可{(m_TitleExtend ? "折叠" : "展开")})";
			m_TitleExtend = SirenixEditorGUI.Foldout(m_TitleExtend, label, SirenixGUIStyles.BoldTitle);
			if (!m_TitleExtend)
				return;

			// 添加类别
			DrawAddClassPart();

			// 内容为空展示
			var dict = ValueEntry.SmartValue;
			if (dict.Count == 0)
			{
				SirenixEditorGUI.InfoMessageBox("未添加任何配置");
				return;
			}

			HashSet<string> removeKey = new HashSet<string>();
			foreach ((string cls, PropertyList list) in dict)
			{
				bool extend = m_ExtendClass.Contains(cls);
				EditorGUILayout.BeginHorizontal();

				extend = SirenixEditorGUI.Foldout(extend, cls);
				if (GUILayout.Button("移除" + cls, GUILayout.MaxWidth(BTN_WIDTH * 2)))
					removeKey.Add(cls);

				// 设置类别是否展开
				if (!extend)
				{
					m_ExtendClass.Remove(cls);
					EditorGUILayout.EndHorizontal();
					continue;
				}
				m_ExtendClass.Add(cls);
				
				EditorGUILayout.EndHorizontal();

				// 绘制该类别内容
				DrawPropertyList(cls, list.list);
			}

			// 移除类别
			foreach (string cls in removeKey)
				dict.Remove(cls);
		}
		private void DrawPropertyList(string cls, List<PropertyData> list)
		{
			// 绘制添加部分
			DrawAddPropertyPart(cls, list);

			// 绘制每个属性
			for (int i = 0; i < list.Count; i++)
			{
				
				string name = list[i].name;
				PropertyInfo property = m_Assembly.GetType(cls).GetProperty(name);
				Type type = property!.PropertyType;
				ExcelCommentAttribute comment = property.GetCustomAttribute<ExcelCommentAttribute>();
				GUIContent content = new GUIContent(comment != null ? $"{name} ({comment.Comment})" : name);
				EditorGUILayout.BeginHorizontal();
				Rect rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);

				if (type == typeof(float))
					list[i].value = SirenixEditorFields.FloatField(rect, content, float.Parse(list[i].value)).ToString(CultureInfo.InvariantCulture);
				else if (type == typeof(int))
					list[i].value = SirenixEditorFields.IntField(rect, content, int.Parse(list[i].value)).ToString();
				else if (type == typeof(bool))
					list[i].value = EditorGUI.Toggle(rect, content, bool.Parse(list[i].value)).ToString();
				else
					list[i].value = SirenixEditorFields.TextField(rect, content, list[i].value);
				
				if (GUILayout.Button("移除", GUILayout.MaxWidth(BTN_WIDTH)))
				{
					list.RemoveAt(i);
					--i;
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		private void DrawAddClassPart()
		{
			var dict = ValueEntry.SmartValue;
			if (dict.Count >= m_ClassOptions.Count)
				return;
			if (EditorGUILayout.DropdownButton(new GUIContent("请选择待添加的配置类"), FocusType.Keyboard,
				    SirenixGUIStyles.DropDownMiniButton, GUILayout.MaxWidth(200)))
			{
				GenericMenu menu = new GenericMenu();
				foreach (string option in m_ClassOptions.Where(cls => !dict.ContainsKey(cls)))
				{
					menu.AddItem(new GUIContent(option), false, () =>
					{
						dict.TryAdd(option, new PropertyList());
					});
				}
				menu.ShowAsContext();
			}
		}

		private void DrawAddPropertyPart(string cls, List<PropertyData> list)
		{
			if (list.Count >= m_PropertyOptions[cls].Count)
				return;
			if (EditorGUILayout.DropdownButton(new GUIContent("请选择待添加的属性名称"), FocusType.Keyboard,
				    SirenixGUIStyles.DropDownMiniButton, GUILayout.MaxWidth(200)))
			{
				GenericMenu menu = new GenericMenu();
				foreach (string option in m_PropertyOptions[cls])
				{
					if (list.Any(chosen => chosen.name == option))
						continue;

					PropertyInfo property = m_Assembly.GetType(cls).GetProperty(option);
					string display = option;
					ExcelCommentAttribute comment = property!.GetCustomAttribute<ExcelCommentAttribute>();
					if (comment != null)
						display += $" ({comment.Comment})";
					menu.AddItem(new GUIContent(display), false, () =>
					{
						object value = FormatterServices.GetSafeUninitializedObject(property.PropertyType);
						list.Add(new PropertyData() { name = option, value = value.ToString() });
					});
				}
				menu.ShowAsContext();
			}
		}
	}

	#endregion

	// 是否检查配置表属性缺失
	public bool DebugExcelColumnCheck = false;

	/* 以下是可更改部分 */

}
#endif