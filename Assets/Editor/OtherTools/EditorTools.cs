using System;
using System.Collections.Generic;
using System.Reflection;
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
}
