using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 事件中心类
/// </summary>
public class EventMgr : SingletonBase<EventMgr>
{
    private Dictionary<string, IEventInfo> events = new Dictionary<string, IEventInfo>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="action">需要添加的监听函数</param>
    public void AddListener<T>(string eventName, UnityAction<T> action)
    {
        if (events.ContainsKey(eventName) && events[eventName] as EventInfo<T> != null)
            (events[eventName] as EventInfo<T>).actions += action;
        else if (!events.ContainsKey(eventName))
            events.Add(eventName, new EventInfo<T>(action));
    }
    public void AddListener(string eventName, UnityAction action)
    {
        if (events.ContainsKey(eventName) && events[eventName] as EventInfo != null)
            (events[eventName] as EventInfo).actions += action;
        else if (!events.ContainsKey(eventName))
            events.Add(eventName, new EventInfo(action));
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="action">需要移除的监听函数</param>
    public void RemoveListener<T>(string eventName, UnityAction<T> action)
    {
        if (events.ContainsKey(eventName) && events[eventName] as EventInfo<T> != null)
            (events[eventName] as EventInfo<T>).actions -= action;
    }
    public void RemoveListener(string eventName, UnityAction action)
    {
        if (events.ContainsKey(eventName) && events[eventName] as EventInfo != null)
            (events[eventName] as EventInfo).actions -= action;
    }

    /// <summary>
    /// 触发一个事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="info">触发事件时传入的参数</param>
    /// <returns>是否成功触发 即是否存在该事件</returns>
    public bool TriggerEvent<T>(string eventName, T info)
    {
        if (events.ContainsKey(eventName) && events[eventName] as EventInfo<T> != null && (events[eventName] as EventInfo<T>).actions != null)
        {
            (events[eventName] as EventInfo<T>).actions(info);
            return true;
        }
        else
            return false;
    }
    public bool TriggerEvent(string eventName)
    {
        if (events.ContainsKey(eventName) && events[eventName] as EventInfo != null && (events[eventName] as EventInfo).actions != null)
        {
            (events[eventName] as EventInfo).actions();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 清空事件中心 一般用于场景切换
    /// </summary>
    public void Clear()
    {
        events.Clear();
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
