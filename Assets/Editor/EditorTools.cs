using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器开发工具函数
/// </summary>
public static class EditorTools
{
    /// <summary>
    ///  获得ToolTip上的信息 没有ToolTip则返回字段名
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static string GetTooltip(Type type, string fieldName)
    {
        FieldInfo field = type.GetField(fieldName);
        TooltipAttribute toolTip = field.GetCustomAttribute<TooltipAttribute>();
        return toolTip != null ? toolTip.tooltip : field.Name;
    }

    public static void ShowList<T>(IList<T> list, Func<T, int, T> func, string addTips = null)
    {
	    // 用于跟踪将要删除的元素索引
	    int indexToRemove = -1;

	    // 遍历列表元素
	    for (int i = 0; i < list.Count; i++)
	    {
		    EditorGUILayout.BeginHorizontal();

		    // 使用传入的func函数来修改和显示元素
		    list[i] = func(list[i], i);

		    // 添加一个删除按钮
		    if (GUILayout.Button("删除", GUILayout.Width(50)))
			    indexToRemove = i;

		    EditorGUILayout.EndHorizontal();
	    }

	    // 如果有元素被标记为删除，移除该元素
	    if (indexToRemove >= 0)
	    {
		    list.RemoveAt(indexToRemove);
		    GUI.changed = true; // 标记GUI改变，以便保存数据
	    }

	    addTips ??= "添加";
	    if (GUILayout.Button(addTips, GUILayout.Width(addTips.Length * 25 + 50)))
	    {
			list.Add(default);
		}
	}

    /// <summary>
    /// 在编辑器中显示一个int为键的字典
    /// </summary>
    public static void ShowDictionary<TValue>(Dictionary<int, TValue> dic, 
	    Func<TValue, TValue> valueFunc, 
	    Func<int, TValue> addFunc,
	    Dictionary<int, bool> fold = null, 
		bool horizontal = true
	    )
    {
	    if (valueFunc == null)
	    {
            Type type = typeof(TValue);
            if (type == typeof(int))
                valueFunc = value => (TValue)(object)EditorGUILayout.IntField((int)(object)value);
            else if (type == typeof(float))
                valueFunc = value => (TValue)(object)EditorGUILayout.FloatField((float)(object)value);
            else if (type == typeof(string))
                valueFunc = value => (TValue)(object)EditorGUILayout.TextField((string)(object)value);
			else if (type == typeof(bool))
				valueFunc = value => (TValue)(object)EditorGUILayout.Toggle((bool)(object)value);
			else if (type == typeof(Vector2))
				valueFunc = value => (TValue)(object)EditorGUILayout.Vector2Field("值", (Vector2)(object)value);
			else if (type == typeof(Vector3))
				valueFunc = value => (TValue)(object)EditorGUILayout.Vector3Field("值", (Vector3)(object)value);
			else if (type == typeof(Vector4))
				valueFunc = value => (TValue)(object)EditorGUILayout.Vector4Field("值", (Vector4)(object)value);
			else if (type == typeof(Color))
				valueFunc = value => (TValue)(object)EditorGUILayout.ColorField((Color)(object)value);
			else if (type == typeof(AnimationCurve))
				valueFunc = value => (TValue)(object)EditorGUILayout.CurveField((AnimationCurve)(object)value);
			else if (type == typeof(Bounds))
				valueFunc = value => (TValue)(object)EditorGUILayout.BoundsField((Bounds)(object)value);
			else if (type == typeof(Rect))
				valueFunc = value => (TValue)(object)EditorGUILayout.RectField((Rect)(object)value);
			else if (type == typeof(Gradient))
				valueFunc = value => (TValue)(object)EditorGUILayout.GradientField((Gradient)(object)value);
			else if (type == typeof(Quaternion))
				valueFunc = value => (TValue)(object)EditorGUILayout.Vector4Field("值", (Vector4)(object)value);
			else if (type == typeof(AnimationClip))
				valueFunc = value => (TValue)(object)EditorGUILayout.ObjectField((AnimationClip)(object)value, typeof(AnimationClip), false);
			else if (type == typeof(AudioClip))
				valueFunc = value => (TValue)(object)EditorGUILayout.ObjectField((AudioClip)(object)value, typeof(AudioClip), false);
			else if (type == typeof(Texture))
				valueFunc = value => (TValue)(object)EditorGUILayout.ObjectField((Texture)(object)value, typeof(Texture), false);
			else if (type == typeof(Sprite))
				valueFunc = value => (TValue)(object)EditorGUILayout.ObjectField((Sprite)(object)value, typeof(Sprite), false);
			else
				throw new ArgumentException("不支持的类型", nameof(TValue));
	    }

		addFunc ??= key => default;

		List<(int, int)> changeList = new List<(int, int)>();
		List<(int, TValue)> valueList = new List<(int, TValue)>();

		GUILayout.BeginVertical();
	    foreach (int key in dic.Keys)
	    {
		    if (fold != null)
		    {
				if (!fold.ContainsKey(key))
				    fold.Add(key, false);
			    fold[key] = EditorGUILayout.Foldout(fold[key], key.ToString(), true);
		    }
			if (fold != null && !fold[key])
				continue;
			if (horizontal)
				GUILayout.BeginHorizontal();
			// 键
			int newKey = EditorGUILayout.IntField(key, GUILayout.Width(100));
			if (key != newKey)
			{
				changeList.Add((key, newKey));
				typeof(TValue).GetProperty(nameof(IUnique.ID))?.SetValue(dic[key], newKey);
			}
			// 值
			TValue value = valueFunc(dic[key]);
			if (!EqualityComparer<TValue>.Default.Equals(value, dic[key]))
				valueList.Add((key, value));
			// 删除
			if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
				changeList.Add((key, 0));
			if (horizontal)
				GUILayout.EndHorizontal();
		}

		// 应用修改
	    foreach ((int old, int @new) in changeList)
	    {
			TValue value = dic[old];
			dic.Remove(old);
			fold?.Remove(old);

			if (@new == 0)
				continue;

			if (dic.ContainsKey(@new))
				Debug.LogError($"字典中已存在键{@new}");
			else
				dic.Add(@new, value);
	    }
		foreach ((int key, TValue value) in valueList)
			dic[key] = value;

		// 添加
	    if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
	    {
		    int key = 1;
		    while (dic.ContainsKey(key))
			    key++;
		    dic.Add(key, addFunc(key));
	    }

		GUILayout.Space(5);
		GUILayout.EndVertical();
	}
	
	private static readonly Dictionary<Type, string[]> enumDescDic = new Dictionary<Type, string[]>();
	/// <summary>
	/// 获取枚举项的描述
	/// </summary>
	/// <param name="type"></param>
	/// <returns>按顺序的枚举描述列表</returns>
    public static string[] GetEnumDesc(Type type)
    {
		if (!type.IsEnum)
			throw new ArgumentException("不是枚举类型", nameof(type));
		if (enumDescDic.TryGetValue(type, out string[] desc))
			return desc;

		desc = Enum.GetNames(type).Select(fieldName => GetTooltip(type, fieldName)).ToArray();
		enumDescDic.Add(type, desc);
		return desc;
    }

	public static string[] GetEnumDesc<T>() => GetEnumDesc(typeof(T));

	/// <summary>
	/// 处理并保存预制体
	/// </summary>
	/// <param name="folderPath">文件夹路径</param>
	/// <param name="processAction">处理预制体的函数</param>
	public static void ProcessAndSavePrefabs(string folderPath, Action<GameObject> processAction)
	{
		// 获取指定路径下所有的.prefab文件
		List<string> prefabPaths = GetAllPrefabs(folderPath);

		foreach (var prefabPath in prefabPaths)
		{
			// 加载原始预制体
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

			// 在此处理预制体
			processAction(prefab);

			// 保存预制体的更改
			EditorUtility.SetDirty(prefab);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// 递归遍历文件夹获取所有预制体的路径
	/// </summary>
	/// <param name="folderPath">文件夹路径</param>
	/// <param name="fullPath">是否返回完整文件路径</param>
	/// <returns>预制体路径</returns>
	public static List<string> GetAllPrefabs(string folderPath, bool fullPath = false)
	{
		List<string> prefabPaths = new List<string>();
		string[] files = Directory.GetFiles(folderPath, "*.prefab", SearchOption.AllDirectories);

		foreach (var file in files)
		{
			if (fullPath)
				prefabPaths.Add(file);
			else
			{
				// 转换为相对于Assets文件夹的路径
				string relativePath = file.Replace(Application.dataPath, "Assets");
				prefabPaths.Add(relativePath);
			}
		}

		return prefabPaths;
	}

	/// <summary>
	/// 使用反射更改一个对象身上的某个属性
	/// </summary>
	/// <param name="obj">对象</param>
	/// <param name="property">属性信息</param>
	/// <param name="typeExtend">扩展类型</param>
	/// <param name="showTitle">是否显示标题</param>
	public static void ModifyProperty(object obj, PropertyInfo property, Func<object, Type, bool> typeExtend = null, bool showTitle = true)
	{
		// 显示分隔段
		if (!showTitle)
			goto SHOW_PROPERTY;
		var header = property.GetCustomAttribute<PropertyHeaderAttribute>();
		if (header is { Header: { } })
		{
			GUILayout.Space(10);
			GUILayout.Label(header.Header, EditorStyles.boldLabel);
			GUILayout.Space(5);
		}

		// 显示注释
		var comment = property.GetCustomAttribute<ExcelCommentAttribute>();
		EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
		GUILayout.Label(comment != null ? comment.Comment : property.Name,
			header is { Header: null } ? EditorStyles.boldLabel : EditorStyles.label,
			GUILayout.Width(200));

		// 按照类型解析属性
		SHOW_PROPERTY:
		object value = property.GetValue(obj);
		if (value is float floatValue)
		{
			floatValue = EditorGUILayout.FloatField(floatValue);
			property.SetValue(obj, floatValue);
		}
		else if (value is int intValue)
		{
			intValue = EditorGUILayout.IntField(intValue);
			property.SetValue(obj, intValue);
		}
		else if (property.PropertyType == typeof(string))
		{
			string stringValue = EditorGUILayout.TextField(value as string);
			property.SetValue(obj, stringValue);
		}
		else if (value is bool boolValue)
		{
			boolValue = EditorGUILayout.Toggle(boolValue);
			property.SetValue(obj, boolValue);
		}

		// 添加其它处理类型
		else if (typeExtend != null)
			typeExtend(obj, property.PropertyType);
		if (showTitle)
			EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// 使用反射更改一个对象身上的某个属性
	/// 此方法需要指定处理方法
	/// </summary>
	/// <param name="obj">对象</param>
	/// <param name="property">属性信息</param>
	/// <param name="func">处理方法</param>
	/// <param name="showTitle">是否显示标题</param>
	public static void ModifyProperty(object obj, PropertyInfo property, Action<object, PropertyInfo> func, bool showTitle = true)
	{
		// 显示分隔段
		if (!showTitle)
			goto SHOW_PROPERTY;
		var header = property.GetCustomAttribute<PropertyHeaderAttribute>();
		if (header is { Header: { } })
		{
			GUILayout.Space(10);
			GUILayout.Label(header.Header, EditorStyles.boldLabel);
			GUILayout.Space(5);
		}

		// 显示注释
		var comment = property.GetCustomAttribute<ExcelCommentAttribute>();
		EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
		GUILayout.Label(comment != null ? comment.Comment : property.Name,
			header is { Header: null } ? EditorStyles.boldLabel : EditorStyles.label,
			GUILayout.Width(200));

		// 按照类型解析属性
		SHOW_PROPERTY:
		func(obj, property);

		if (showTitle)
			EditorGUILayout.EndHorizontal();
	}

	public static void ModifyField(Type type, SerializedObject obj, string field, bool includeChildren = true, string tips = null)
	{
		var comment = type.GetField(field)?.GetCustomAttribute<TooltipAttribute>();
		tips ??= comment != null ? comment.tooltip : field;
		EditorGUILayout.PropertyField(obj.FindProperty(field),
			new GUIContent(tips), includeChildren);
	}

	public static void ModifyField(Type type, SerializedProperty property, string field, bool includeChildren = true, string tips = null)
	{
		var comment = type.GetField(field)?.GetCustomAttribute<TooltipAttribute>();
		tips ??= comment != null ? comment.tooltip : field;
		EditorGUILayout.PropertyField(property.FindPropertyRelative(field),
			new GUIContent(tips), includeChildren);
	}

	public static string ToAssetPath(string fullPath) => fullPath.Replace(Application.dataPath, "Assets");
	public static string ToFullPath(string fullPath) => fullPath.Replace("Assets", Application.dataPath);
}
