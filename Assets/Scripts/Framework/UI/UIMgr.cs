using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// UI面板管理器
/// </summary>
// ReSharper disable once InconsistentNaming
public class UIMgr : SingletonBase<UIMgr>
{
    // Canvas对象的位置
    public RectTransform Canvas { get; private set; }
    // Canvas上显示不同面板的层级位置
    private readonly List<Transform> m_Layers = new();
	// Canvas上已经加载出的面板
	private readonly Dictionary<string, PanelBase> m_Panel = new();
    public UIMgr()
    {
        // 尝试获取场景上的Canvas和EventSystem对象
        GameObject canvas = null, es = null;
        if (Object.FindObjectOfType<Canvas>() != null)
            canvas = Object.FindObjectOfType<Canvas>().gameObject;
        if (Object.FindObjectOfType<EventSystem>() != null)
            es = Object.FindObjectOfType<EventSystem>().gameObject;

        // 尝试从Resources里加载对象
        if (canvas == null)
            canvas = ResMgr.Instance.Load<GameObject>(PanelPath.CANVAS);
        if (es == null)
            es = ResMgr.Instance.Load<GameObject>(PanelPath.EVENT_SYSTEM);

        // 检查是否加载成功
        if (canvas == null)
        {
            Debug.LogError("场景上没有带有Canvas组件的对象 Resources中没有UI/Canvas对象 无法初始化UIMgr");
            return;
        }
        if (es == null)
        {
            Debug.LogError("场景上没有带有EventSystem组件的对象 Resources中没有UI/EventSystem对象 无法初始化UIMgr");
            return;
        }

        // 防止对象被销毁
        Object.DontDestroyOnLoad(canvas);
        Object.DontDestroyOnLoad(es);

        // 记录位置
        this.Canvas = canvas.transform as RectTransform;
        if (canvas.transform.childCount == 0)
            m_Layers.Add(canvas.transform);
        else
            for (int i = 0; i < canvas.transform.childCount; i++)
                m_Layers.Add(canvas.transform.GetChild(i));
    }

    /// <summary>
    /// 从Resources里加载并打开面板
    /// </summary>
    /// <param name="panelPath">面板在Resources中的路径</param>
    /// <param name="layer">面板在Canvas中的层级</param>
    public GameObject ShowPanel<T>(string panelPath, int layer = 0) where T : PanelBase
    {
        return ProcessPanel<T>(ResMgr.Instance.Load<GameObject>(panelPath), panelPath, layer);
    }

    /// <summary>
    /// 从Resources里加载并打开面板(异步加载面板)
    /// </summary>
    /// <param name="panelPath">面板在Resources中的路径</param>
    /// <param name="layer">面板在Canvas中的层级</param>
    /// <param name="callback">回调函数</param>
    public void ShowPanelAsync<T>(string panelPath, int layer = 0, UnityAction<T> callback = null) where T : PanelBase
    {
        ResMgr.Instance.LoadAsync<GameObject>(panelPath, (obj) =>
        {
            if (ProcessPanel<T>(obj, panelPath, layer))
                callback?.Invoke(obj.GetComponent<T>());
        });
    }

    /// <summary>
    /// 获取面板
    /// </summary>
    /// <param name="panelName">面板对象名称 null表示使用类型名</param>
    /// <returns>未找到面板时返回null</returns>
    public T GetPanel<T>(string panelName = null) where T : PanelBase
    {
        string key = panelName ?? typeof(T).Name;
        m_Panel.TryGetValue(key, out PanelBase panel);
        return panel as T;
    }

    /// <summary>
    /// 关闭并销毁面板
    /// </summary>
    public void HidePanel<T>(string panelName = null)
    {
        string key = panelName ?? typeof(T).Name;
        if (!m_Panel.ContainsKey(key)) 
	        return;
        m_Panel[key].Hide();
        Object.Destroy(m_Panel[key].gameObject);
        m_Panel.Remove(key);
    }

    protected GameObject ProcessPanel<T>(GameObject obj, string panelPath, int layer = 0) where T : PanelBase
    {
        // 设置父对象
        if (layer < 0 || layer >= m_Layers.Count)
        {
            Debug.LogWarning($"面板{panelPath}层级有误");
            layer = 0;
        }
        obj.transform.SetParent(m_Layers[layer]);
        obj.name = typeof(T).Name;

        // 设置初始位置
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        (obj.transform as RectTransform).offsetMax = Vector2.zero;
        (obj.transform as RectTransform).offsetMin = Vector2.zero;

        // 调用面板显示函数
        if (!obj.TryGetComponent(out T panel))
        {
            Debug.LogError($"对象{obj.name}不是{typeof(T).Name}面板");
            return null;
        }
        panel.Show();

        // 记录面板
        m_Panel.Add(panel.name, panel);
        return obj;
    }
}
