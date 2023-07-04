using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 脚本模板类型
/// 添加类型步骤：
/// 1. 添加枚举项
/// 2. 在Editor中添加模板路径
/// 3. 添加必要的参数（可选）
/// 4. 创建模板文件
/// </summary>
public enum ScriptTemplateType
{
    ReadOnlyData,       // 只读数据类
    ReadOnlyDB,         // 只读数据库
    RuntimeData,        // 运行时数据
    RuntimeMgr,         // 运行时数据管理器
    Panel,              // UI面板
    Editor,             // 编辑器
    GlobalConfigXlsx,   // 表格型全局配置类
    GlobalConfigJson,   // Json型全局配置类
    Enum,               // 枚举
}

/// <summary>
/// 脚本生成器交互脚本
/// </summary>
public class ScriptGeneratorGUI : MonoBehaviour
{
    [Tooltip("模板类型")]
    public ScriptTemplateType template;
    [Tooltip("脚本生成目标路径（Assets开始的相对路径）")]
    public string target;
    [Tooltip("是否覆盖原代码文件")]
    public bool overwrite;

    [Header("以下为模板参数")]
    [Tooltip("类名简写")]
    public string shortClassName;
    [Tooltip("类注释")]
    public string classComment;
}
