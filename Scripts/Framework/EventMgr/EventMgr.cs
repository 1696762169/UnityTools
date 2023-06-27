using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �¼�������
/// </summary>
public class EventMgr : SingletonBase<EventMgr>
{
    private Dictionary<string, IEventInfo> events = new Dictionary<string, IEventInfo>();

    /// <summary>
    /// �����¼�
    /// </summary>
    /// <param name="eventName">�¼���</param>
    /// <param name="action">��Ҫ��ӵļ�������</param>
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
    /// ȡ�������¼�
    /// </summary>
    /// <param name="eventName">�¼���</param>
    /// <param name="action">��Ҫ�Ƴ��ļ�������</param>
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
    /// ����һ���¼�
    /// </summary>
    /// <param name="eventName">�¼���</param>
    /// <param name="info">�����¼�ʱ����Ĳ���</param>
    /// <returns>�Ƿ�ɹ����� ���Ƿ���ڸ��¼�</returns>
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
    /// ����¼����� һ�����ڳ����л�
    /// </summary>
    public void Clear()
    {
        events.Clear();
    }

    // Ϊ����װ���������еİ���
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
