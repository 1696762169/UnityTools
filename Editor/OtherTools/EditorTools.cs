using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// �༭���������ߺ���
/// </summary>
public static class EditorTools
{
    /// <summary>
    ///  ���ToolTip�ϵ���Ϣ û��ToolTip�򷵻��ֶ���
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
