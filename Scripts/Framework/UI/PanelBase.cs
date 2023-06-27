//#define DEBUG_PANELBASE
//#define DEBUG_MULTI
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class PanelBase : MonoBehaviour
{
    // �洢�ؼ����ֵ�
    private Dictionary<Type, Dictionary<string, UIBehaviour>> m_Elements = new Dictionary<Type, Dictionary<string, UIBehaviour>>();

    // ����֧�ַ��ʵĿؼ�����
    public static readonly Type[] SupportTypes = { 
        // �����ؼ�
        typeof(Image),
        typeof(Text),
        typeof(RawImage),
        // ��Ͽؼ�
        typeof(Button),
        typeof(Toggle),
        typeof(InputField),
        typeof(Slider),
        typeof(ScrollRect),
        typeof(Dropdown),
    };

    protected virtual void Awake()
    {
        foreach (Type type in SupportTypes)
            AddElements(type);
    }

    /// <summary>
    /// �õ�������ϼ�¼��ĳһ���͵Ŀؼ�
    /// </summary>
    /// <typeparam name="T">�ؼ�����</typeparam>
    /// <param name="elementName">�ؼ���</param>
    public T GetElement<T>(string elementName) where T : UIBehaviour
    {
        // ����Ƿ���ڸ����Ϳؼ�
        if (m_Elements.ContainsKey(typeof(T)))
        {
            // ��ȡͬ���ؼ�
            m_Elements[typeof(T)].TryGetValue(elementName, out UIBehaviour element);
#if DEBUG_PANELBASE && UNITY_EDITOR
            if (element == null)
                Debug.LogError($"���{this.name}��û����Ϊ{elementName}��{typeof(T).Name}�ؼ�");
#endif
            return element as T;
        }

        // û���ҵ������͵Ŀؼ�ʱ ��������Ƿ�֧�ָ�����
        else
        {
#if DEBUG_PANELBASE && UNITY_EDITOR
            bool support = false;
            foreach (Type type in SupportTypes)
            {
                if (type == typeof(T))
                {
                    support = true;
                    break;
                }
            }
            if (support)
                Debug.LogError($"���{this.name}��û��{typeof(T).Name}�ؼ�");
            else
                Debug.LogError($"PanelBase��֧�ּ�¼{typeof(T).Name}�ؼ�");
#endif
            return null;
        }
    }

    /// <summary>
    /// Ϊһ���ؼ����EventTrigger�¼�
    /// </summary>
    /// <typeparam name="T">�ؼ�����</typeparam>
    /// <param name="name">�ؼ���</param>
    /// <param name="eventID">�¼�����</param>
    /// <param name="func">�¼�����ʱ�Ĵ�����</param>
    public void AddEntry<T>(string name, EventTriggerType eventID, UnityAction<BaseEventData> func) where T : UIBehaviour
    {
        T element = GetElement<T>(name);
        if (element == null)
            return;

        // ���EventTrigger���
        EventTrigger trigger = element.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = element.gameObject.AddComponent<EventTrigger>();
        // ���Entry
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventID;
        entry.callback.AddListener(func);
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// ��ʾ�����
    /// </summary>
    public abstract void Show();
    /// <summary>
    /// ���ش����
    /// </summary>
    public abstract void Hide();
    
    // ��¼�����ĳһ���͵Ŀؼ�
    private void AddElements(Type type)
    {
        foreach (UIBehaviour element in GetComponentsInChildren(type, true))
        {
            if (!m_Elements.ContainsKey(type))
                m_Elements.Add(type, new Dictionary<string, UIBehaviour>());
            if (m_Elements[type].ContainsKey(element.name))
            {
#if DEBUG_MULTI && UNITY_EDITOR
            Debug.LogError($"���{name}�ϴ��ڶ����Ϊ{element.name}��{type.Name}�ؼ�");
#endif
            }
            else
            {
                m_Elements[type].Add(element.name, element);
            }
        }
    }
}
