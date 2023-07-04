using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 提供与Excel工具交互的GUI
/// </summary>
public class ExcelToolGUI : MonoBehaviour
{
    [Tooltip("待创建表格的对象类型")]
    public string dataType;
    [Tooltip("待创建的表格文件名")]
    public string fileName;
    [Tooltip("是否覆盖原文件")]
    public bool overwrite;
}
