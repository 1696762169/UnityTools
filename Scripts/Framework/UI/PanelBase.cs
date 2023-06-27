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
    // 存储控件的字典
    private Dictionary<Type, Dictionary<string, UIBehaviour>> m_Elements = new Dictionary<Type, Dictionary<string, UIBehaviour>>();

    // 此类支持访问的控件类型
    public static readonly Type[] SupportTypes = { 
        // 基础控件
        typeof(Image),
        typeof(Text),
        typeof(RawImage),
        // 组合控件
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
    /// 得到该面板上记录的某一类型的控件
    /// </summary>
    /// <typeparam name="T">控件类型</typeparam>
    /// <param name="elementName">控件名</param>
    public T GetElement<T>(string elementName) where T : UIBehaviour
    {
        // 检查是否存在该类型控件
        if (m_Elements.ContainsKey(typeof(T)))
        {
            // 获取同名控件
            m_Elements[typeof(T)].TryGetValue(elementName, out UIBehaviour element);
#if DEBUG_PANELBASE && UNITY_EDITOR
            if (element == null)
                Debug.LogError($"面板{this.name}上没有名为{elementName}的{typeof(T).Name}控件");
#endif
            return element as T;
        }

        // 没有找到该类型的控件时 检查自身是否支持该类型
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
                Debug.LogError($"面板{this.name}上没有{typeof(T).Name}控件");
            else
                Debug.LogError($"PanelBase不支持记录{typeof(T).Name}控件");
#endif
            return null;
        }
    }

    /// <summary>
    /// 为一个控件添加EventTrigger事件
    /// </summary>
    /// <typeparam name="T">控件类型</typeparam>
    /// <param name="name">控件名</param>
    /// <param name="eventID">事件类型</param>
    /// <param name="func">事件触发时的处理函数</param>
    public void AddEntry<T>(string name, EventTriggerType eventID, UnityAction<BaseEventData> func) where T : UIBehaviour
    {
        T element = GetElement<T>(name);
        if (element == null)
            return;

        // 添加EventTrigger组件
        EventTrigger trigger = element.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = element.gameObject.AddComponent<EventTrigger>();
        // 添加Entry
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventID;
        entry.callback.AddListener(func);
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// 显示此面板
    /// </summary>
    public abstract void Show();
    /// <summary>
    /// 隐藏此面板
    /// </summary>
    public abstract void Hide();
    
    // 记录面板上某一类型的控件
    private void AddElements(Type type)
    {
        foreach (UIBehaviour element in GetComponentsInChildren(type, true))
        {
            if (!m_Elements.ContainsKey(type))
                m_Elements.Add(type, new Dictionary<string, UIBehaviour>());
            if (m_Elements[type].ContainsKey(element.name))
            {
#if DEBUG_MULTI && UNITY_EDITOR
            Debug.LogError($"面板{name}上存在多个名为{element.name}的{type.Name}控件");
#endif
            }
            else
            {
                m_Elements[type].Add(element.name, element);
            }
        }
    }
}
