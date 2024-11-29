using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ű�ģ������
/// ������Ͳ��裺
/// 1. ���ö����
/// 2. ��Editor�����ģ��·��
/// 3. ��ӱ�Ҫ�Ĳ�������ѡ��
/// 4. ����ģ���ļ�
/// </summary>
public enum ScriptTemplateType
{
    ReadOnlyData,       // ֻ��������
    ReadOnlyDB,         // ֻ�����ݿ�
    RuntimeData,        // ����ʱ����
    RuntimeMgr,         // ����ʱ���ݹ�����
    Panel,              // UI���
    Editor,             // �༭��
    GlobalConfigXlsx,   // �����ȫ��������
    GlobalConfigJson,   // Json��ȫ��������
    Enum,               // ö��
}

/// <summary>
/// �ű������������ű�
/// </summary>
public class ScriptGeneratorGUI : MonoBehaviour
{
    [Tooltip("ģ������")]
    public ScriptTemplateType template;
    [Tooltip("�ű�����Ŀ��·����Assets��ʼ�����·����")]
    public string target;
    [Tooltip("�Ƿ񸲸�ԭ�����ļ�")]
    public bool overwrite;

    [Header("����Ϊģ�����")]
    [Tooltip("������д")]
    public string shortClassName;
    [Tooltip("��ע��")]
    public string classComment;
}
