using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIMgr : SingletonBase<UIMgr>
{
    // Canvas�����λ��
    public RectTransform Canvas { get; protected set; }
    // Canvas����ʾ��ͬ���Ĳ㼶λ��
    protected List<Transform> m_Layers = new List<Transform>();
    // Canvas���Ѿ����س������
    protected Dictionary<string, PanelBase> m_Panel = new Dictionary<string, PanelBase>();
    public UIMgr()
    {
        // ���Ի�ȡ�����ϵ�Canvas��EventSystem����
        GameObject canvas = null, es = null;
        if (GameObject.FindObjectOfType<Canvas>() != null)
            canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
        if (GameObject.FindObjectOfType<EventSystem>() != null)
            es = GameObject.FindObjectOfType<EventSystem>().gameObject;

        // ���Դ�Resources����ض���
        if (canvas == null)
            canvas = ResMgr.Instance.Load<GameObject>("UI/Canvas");
        if (es == null)
            es = ResMgr.Instance.Load<GameObject>("UI/EventSystem");

        // ����Ƿ���سɹ�
        if (canvas == null)
        {
            Debug.LogError("������û�д���Canvas����Ķ��� Resources��û��UI/Canvas���� �޷���ʼ��UIMgr");
            return;
        }
        if (es == null)
        {
            Debug.LogError("������û�д���EventSystem����Ķ��� Resources��û��UI/EventSystem���� �޷���ʼ��UIMgr");
            return;
        }

        // ��ֹ��������
        GameObject.DontDestroyOnLoad(canvas);
        GameObject.DontDestroyOnLoad(es);

        // ��¼λ��
        this.Canvas = canvas.transform as RectTransform;
        if (canvas.transform.childCount == 0)
            m_Layers.Add(canvas.transform);
        else
            for (int i = 0; i < canvas.transform.childCount; i++)
                m_Layers.Add(canvas.transform.GetChild(i));
    }

    /// <summary>
    /// ��Resources����ز������
    /// </summary>
    /// <param name="panelPath">�����Resources�е�·��</param>
    /// <param name="layer">�����Canvas�еĲ㼶</param>
    public GameObject ShowPanel<T>(string panelPath, int layer = 0) where T : PanelBase
    {
        return ProcessPanel<T>(ResMgr.Instance.Load<GameObject>(panelPath), panelPath, layer);
    }

    /// <summary>
    /// ��Resources����ز������(�첽�������)
    /// </summary>
    /// <param name="panelPath">�����Resources�е�·��</param>
    /// <param name="layer">�����Canvas�еĲ㼶</param>
    public void ShowPanelAsync<T>(string panelPath, int layer = 0, UnityAction<T> callback = null) where T : PanelBase
    {
        ResMgr.Instance.LoadAsync<GameObject>(panelPath, (obj) =>
        {
            if (ProcessPanel<T>(obj, panelPath, layer))
                callback?.Invoke(obj.GetComponent<T>());
        });
    }

    /// <summary>
    /// ��ȡ���
    /// </summary>
    /// <param name="panelName">���������� null��ʾʹ��������</param>
    /// <returns>δ�ҵ����ʱ����null</returns>
    public T GetPanel<T>(string panelName = null) where T : PanelBase
    {
        string key = panelName ?? typeof(T).Name;
        m_Panel.TryGetValue(key, out PanelBase panel);
        return panel as T;
    }

    /// <summary>
    /// �رղ��������
    /// </summary>
    public void HidePanel<T>(string panelName = null)
    {
        string key = panelName ?? typeof(T).Name;
        if (m_Panel.ContainsKey(key))
        {
            m_Panel[key].Hide();
            GameObject.Destroy(m_Panel[key].gameObject);
            m_Panel.Remove(key);
        }
    }

    protected GameObject ProcessPanel<T>(GameObject obj, string panelPath, int layer = 0) where T : PanelBase
    {
        // ���ø�����
        if (layer < 0 || layer >= m_Layers.Count)
        {
            Debug.LogWarning($"���{panelPath}�㼶����");
            layer = 0;
        }
        obj.transform.SetParent(m_Layers[layer]);

        // ���ó�ʼλ��
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        (obj.transform as RectTransform).offsetMax = Vector2.zero;
        (obj.transform as RectTransform).offsetMin = Vector2.zero;

        // ���������ʾ����
        T panel = obj.GetComponent<T>();
        if (panel == null)
        {
            Debug.LogError($"����{obj.name}����{typeof(T).Name}���");
            return null;
        }
        panel.Show();

        // ��¼���
        m_Panel.Add(panel.name, panel);
        return obj;
    }
}
