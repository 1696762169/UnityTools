using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    protected bool m_Initializing = false;
    protected string[] m_Content = new string[] { "确认", "取消" };
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);
        if (Application.isPlaying)
        {
            // 初始化存档需要确认
            if (GUILayout.Button("初始化存档数据", GUILayout.Width(150)) || m_Initializing)
            {
                m_Initializing = true;
                GUILayout.Space(10);
                GUILayout.Label("确认要初始化并覆盖已有的存档数据吗？");
                switch (GUILayout.SelectionGrid(-1, m_Content, 2, GUILayout.Width(150)))
                {
                case 0:
                    GameManager.Instance.InitDataAndSave();
                    m_Initializing = false;
                    break;
                case 1:
                    m_Initializing = false;
                    break;
                }
            }
        }
    }
}
