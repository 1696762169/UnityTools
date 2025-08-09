using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 更易用的日志输出工具类
/// </summary>
/// <description>
/// Tips:
/// [HideInCallstack]只能隐藏堆栈信息 不能防止点击控制台输出时跳转到此处
/// 真正防止无效跳转的是类名和方法名 当类名以“Logger”结尾且方法名以“Log”开头时 控制台不会跳转到此处
/// </description>
public static class EasyLogger
{
	private const string LOG_CONDITION = "ENABLE_PLAYER_LOG";
	private const string WARNING_CONDITION = "ENABLE_PLAYER_WARNING";

	#region 快捷方法

	[Conditional("DEBUG")]
	[Conditional(LOG_CONDITION)]
	[HideInCallstack]
	public static void Log(object message, Color color, Object context = null)
	{
		Debug.unityLogger.Log(LogType.Log, (object) message.ToString().ToLogColor(color), context);
	}

	[Conditional("DEBUG")]
	[Conditional(LOG_CONDITION)]
	[HideInCallstack]
	public static void Log(object message, string tag, Object context = null)
	{
		Color color = GetAutoColor(tag);
		Debug.unityLogger.Log(LogType.Log, tag, message.ToString().ToLogColor(color), context);
	}

	[Conditional("DEBUG")]
	[Conditional(LOG_CONDITION)]
	[HideInCallstack]
	public static void Log(object message, string tag, LogType logType, Object context = null)
	{
		Color color = GetAutoColor(tag);
		Debug.unityLogger.Log(logType, tag, message.ToString().ToLogColor(color), context);
	}

	[Conditional("DEBUG")]
	[Conditional(LOG_CONDITION)]
	[HideInCallstack]
	public static void LogAuto(object message, Object context = null)
	{
		StackTrace stackTrace = new StackTrace();
		string tag = stackTrace.GetFrame(1).GetMethod().DeclaringType?.Name;
		Log(message, tag, LogType.Log, context);
	}

	[Conditional("DEBUG")]
	[Conditional(LOG_CONDITION), Conditional(WARNING_CONDITION)]
	[HideInCallstack]
	public static void LogWarningAuto(object message, Object context = null)
	{
		StackTrace stackTrace = new StackTrace();
		string tag = stackTrace.GetFrame(1).GetMethod().DeclaringType?.Name;
		Log(message, tag, LogType.Warning, context);
	}

	[HideInCallstack]
	public static void LogErrorAuto(object message, Object context = null)
	{
		StackTrace stackTrace = new StackTrace();
		string tag = stackTrace.GetFrame(1).GetMethod().DeclaringType?.Name;
		Log(message, tag, LogType.Error, context);
	}

	private static readonly Dictionary<string, Color> ColorDict = new();
	private static Color GetAutoColor(string tag)
	{
		if (ColorDict.TryGetValue(tag, out Color color))
			return color;
		int hashCode = tag.GetHashCode();
		color = new Color((hashCode | 0xFF) / 255f, (hashCode >> 8 & 0xFF) / 255f, (hashCode >> 16 & 0xFF) / 255f);
		color = Color.Lerp(color, Color.white, 0.8f);
		ColorDict.Add(tag, color);
		return color;
	}
	#endregion

	#region 日志格式化
#if UNITY_EDITOR
	public static string ToLogBold(this string str) => $"<b>{str}</b>";
	public static string ToLogItalic(this string str) => $"<i>{str}</i>";
	public static string ToLogColor(this string str, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
	public static string ToLogSize(this string str, int size) => $"<size={size}>{str}</size>";
	public static string ToLogAssetLink(this string str, string assetPath, int line = 0) => $"<a href=\"{assetPath}\" line=\"{line}\">{str}</a>";
	public static string ToLogFileLink(this string str, string absolutePath) => $"<a href=\"file:///{absolutePath}\">{str}</a>";
	public static string ToLogUrlLink(this string str, string url) => $"<a href=\"{url}\">{str}</a>";
#else
	public static string ToLogBold(this string str) => str;
	public static string ToLogItalic(this string str) => str;
	public static string ToLogColor(this string str, Color color) => str;
	public static string ToLogSize(this string str, int size) => str;
	public static string ToLogAssetLink(this string str, string assetPath, int line = 0) => str;
	public static string ToLogFileLink(this string str, string absolutePath) => str;
	public static string ToLogUrlLink(this string str, string url) => str;
#endif
	#endregion

	#region 条件编译封装

	[Conditional("DEBUG")]
	[Conditional(LOG_CONDITION)]
	[HideInCallstack]
	public static void Log(object message, Object context = null)
	{
		Debug.unityLogger.Log(LogType.Log, message, context);
	}

	[Conditional("DEBUG")]
	[Conditional(LOG_CONDITION), Conditional(WARNING_CONDITION)]
	[HideInCallstack]
	public static void LogWarning(object message, Object context = null)
	{
		Debug.unityLogger.Log(LogType.Warning, message, context);
	}

	//[Conditional("DEBUG"), Conditional("ENABLE_PLAYER_ERROR")]
	[HideInCallstack]
	public static void LogError(object message, Object context = null)
	{
		Debug.unityLogger.Log(LogType.Error, message, context);
	}

	//[Conditional("DEBUG"), Conditional("ENABLE_PLAYER_ERROR")]
	[HideInCallstack]
	public static void LogException(System.Exception exception, Object context = null)
	{
		Debug.unityLogger.LogException(exception, context);
	}

	//[Conditional("DEBUG"), Conditional("ENABLE_PLAYER_ERROR")]
	[HideInCallstack]
	public static void Assert(bool condition, object message, Object context = null)
	{
		if (condition)
			return;
		Debug.unityLogger.Log(LogType.Assert, message, context);
	}

	#endregion
}
