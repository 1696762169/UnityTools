using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 事件中心类 用于管理全局事件
/// </summary>
public class EventMgr : SingletonBase<EventMgr>
{
    private readonly Dictionary<string, IEventInfo> m_Events = new();

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="action">需要添加的监听函数</param>
    public void AddListener<T>(string eventName, UnityAction<T> action)
    {
        if (m_Events.ContainsKey(eventName) && m_Events[eventName] is EventInfo<T> eventInfo)
	        eventInfo.actions += action;
        else if (!m_Events.ContainsKey(eventName))
            m_Events.Add(eventName, new EventInfo<T>(action));
    }
    public void AddListener(string eventName, UnityAction action)
    {
        if (m_Events.ContainsKey(eventName) && m_Events[eventName] is EventInfo eventInfo)
	        eventInfo.actions += action;
        else if (!m_Events.ContainsKey(eventName))
            m_Events.Add(eventName, new EventInfo(action));
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="action">需要移除的监听函数</param>
    public void RemoveListener<T>(string eventName, UnityAction<T> action)
    {
        if (m_Events.ContainsKey(eventName) && m_Events[eventName] is EventInfo<T> eventInfo)
	        eventInfo.actions -= action;
    }
    public void RemoveListener(string eventName, UnityAction action)
    {
        if (m_Events.ContainsKey(eventName) && m_Events[eventName] is EventInfo eventInfo)
	        eventInfo.actions -= action;
    }

    /// <summary>
    /// 触发一个事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="info">触发事件时传入的参数</param>
    /// <returns>是否成功触发 即是否存在该事件</returns>
    public bool TriggerEvent<T>(string eventName, T info)
    {
	    if (!m_Events.ContainsKey(eventName) ||
	        m_Events[eventName] is not EventInfo<T> { actions: not null } eventInfo) return false;
	    eventInfo.actions(info);
	    return true;
    }
    public bool TriggerEvent(string eventName)
    {
	    if (!m_Events.ContainsKey(eventName) ||
	        m_Events[eventName] is not EventInfo { actions: not null } eventInfo) return false;
	    eventInfo.actions();
	    return true;
    }

    /// <summary>
    /// 清空事件中心 一般用于场景切换
    /// </summary>
    public void Clear()
    {
        m_Events.Clear();
    }

    // 为避免装箱拆箱而进行的包裹
    private class EventInfo<T> : IEventInfo
    {
        public UnityAction<T> actions;
        public EventInfo(UnityAction<T> action) => actions += action;
    }
    private class EventInfo : IEventInfo
    {
        public UnityAction actions;
        public EventInfo(UnityAction action) => actions += action;
    }
    private interface IEventInfo { }
}
