using System.Collections.Generic;
using UnityEngine.Events;
using Delegate = System.Delegate;

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
	/// <returns>是否成功订阅 即事件参数类型是否匹配</returns>
	public bool AddListener<T>(string eventName, UnityAction<T> action)
	{
		if (m_Events.TryGetValue(eventName, out IEventInfo eventInfo))
		{
			if (eventInfo is EventInfo<T> eventInfoT)
			{
				eventInfoT.actions += action;
				return true;
			}

			EasyLogger.LogWarningAuto($"Add Event '{eventName}' need parameter of type '{eventInfo.TypeName}', got '{typeof(T).FullName}'!");
			return false;
		}

		m_Events.Add(eventName, new EventInfo<T>(action));
		return true;
	}

	/// <summary>
	/// 订阅事件
	/// </summary>
	/// <param name="eventName">事件名</param>
	/// <param name="action">需要添加的监听函数</param>
	/// <returns>是否成功订阅 即事件参数类型是否匹配</returns>
	public bool AddListener(string eventName, UnityAction action)
	{
		if (m_Events.TryGetValue(eventName, out IEventInfo eventInfo))
		{
			if (eventInfo is EventInfo eventInfoT)
			{
				eventInfoT.actions += action;
				return true;
			}

			EasyLogger.LogWarningAuto($"Add Event '{eventName}' need parameter of type '{eventInfo.TypeName}', got no parameter!");
			return false;
		}

		m_Events.Add(eventName, new EventInfo(action));
		return true;
	}

	/// <summary>
	/// 移除订阅事件
	/// </summary>
	/// <param name="eventName">事件名</param>
	/// <param name="action">需要移除的监听函数</param>
	/// <returns>是否成功移除 即事件参数类型是否匹配</returns>
	public bool RemoveListener<T>(string eventName, UnityAction<T> action)
	{
		if (m_Events.TryGetValue(eventName, out IEventInfo eventInfo))
		{
			if (eventInfo is EventInfo<T> eventInfoT)
			{
				Delegate d = eventInfoT.actions;
				bool result = RemoverListenerInternal(eventName, ref d, action);
				eventInfoT.actions = d as UnityAction<T>;
				return result;
			}
			EasyLogger.LogWarningAuto($"Remove Event '{eventName}' need parameter of type '{eventInfo.TypeName}', got '{typeof(T).FullName}'!");
			return false;
		}

		EasyLogger.LogWarningAuto($"Event '{eventName}' not found!");
		return false;
	}
	/// <summary>
	/// 移除订阅事件
	/// </summary>
	/// <param name="eventName">事件名</param>
	/// <param name="action">需要移除的监听函数</param>
	/// <returns>是否成功移除 即事件参数类型是否匹配</returns>
	public bool RemoveListener(string eventName, UnityAction action)
	{
		if (m_Events.TryGetValue(eventName, out IEventInfo eventInfo))
		{
			if (eventInfo is EventInfo eventInfoT)
			{
				Delegate d = eventInfoT.actions;
				bool result = RemoverListenerInternal(eventName, ref d, action);
				eventInfoT.actions = d as UnityAction;
				return result;
			}
			EasyLogger.LogWarningAuto($"Remove Event '{eventName}' need parameter of type '{eventInfo.TypeName}', got no parameter!");
			return false;
		}

		EasyLogger.LogWarningAuto($"Event '{eventName}' not found!");
		return false;
	}

	private bool RemoverListenerInternal(string eventName, ref Delegate source, Delegate action)
	{
		int prevCount = source.GetInvocationList().Length;
		source = Delegate.Remove(source, action);
		if (source == null)
			m_Events.Remove(eventName);

		int currentCount = source == null ? 0 : source.GetInvocationList().Length;
		if (prevCount - 1 == currentCount)
			return true;
		EasyLogger.LogWarningAuto($"Remove Event '{eventName}' listener '{action.Method.Name}' not found!");
		return false;
	}

	/// <summary>
	/// 触发一个事件
	/// </summary>
	/// <param name="eventName">事件名</param>
	/// <param name="info">触发事件时传入的参数</param>
	/// <returns>是否成功触发 即是否存在该事件</returns>
	public bool TriggerEvent<T>(string eventName, T info)
	{
		if (m_Events.TryGetValue(eventName, out IEventInfo eventInfo))
		{
			if (eventInfo is EventInfo<T> eventInfoT)
			{
				if (eventInfoT.actions == null)
				{
					EasyLogger.LogWarningAuto($"Event '{eventName}' has no listener to trigger!");
					return false;
				}

				eventInfoT.actions.Invoke(info);
				return true;
			}
			EasyLogger.LogWarningAuto($"Trigger Event '{eventName}' need parameter of type '{eventInfo.TypeName}', got '{typeof(T).FullName}'!");
			return false;
		}

		EasyLogger.LogWarningAuto($"Event '{eventName}' not found!");
		return false;
	}
	/// <summary>
	/// 触发一个事件
	/// </summary>
	/// <param name="eventName">事件名</param>
	/// <returns>是否成功触发 即是否存在该事件</returns>
	public bool TriggerEvent(string eventName)
	{
		if (m_Events.TryGetValue(eventName, out IEventInfo eventInfo))
		{
			if (eventInfo is EventInfo eventInfoT)
			{
				if (eventInfoT.actions == null)
				{
					EasyLogger.LogWarningAuto($"Event '{eventName}' has no listener to trigger!");
					return false;
				}
				eventInfoT.actions.Invoke();
				return true;
			}
			EasyLogger.LogWarningAuto($"Trigger Event '{eventName}' need parameter of type '{eventInfo.TypeName}', got no parameter!");
			return false;
		}

		EasyLogger.LogWarningAuto($"Event '{eventName}' not found!");
		return false;
	}

	/// <summary>
	/// 清空事件中心 一般用于场景切换
	/// </summary>
	public void ClearAll()
	{
		m_Events.Clear();
	}
	/// <summary>
	/// 清空指定事件
	/// </summary>
	/// <param name="eventName">事件名</param>
	/// <returns>是否成功移除 即是否存在该事件</returns>
	public bool Clear(string eventName)
	{
		if (m_Events.ContainsKey(eventName))
		{
			m_Events.Remove(eventName);
			return true;
		}
		EasyLogger.LogWarningAuto($"Event '{eventName}' not found!");
		return false;
	}

	// 为避免装箱拆箱而进行的包裹
	private class EventInfo<T> : IEventInfo
	{
		public UnityAction<T> actions;
		public EventInfo(UnityAction<T> action)
		{
			actions += action;
			TypeName = typeof(T).FullName;
		}

		public string TypeName { get; }
	}
	private class EventInfo : IEventInfo
	{
		public UnityAction actions;
		public EventInfo(UnityAction action) => actions += action;
		public string TypeName => "null";
	}

	private interface IEventInfo
	{
		public string TypeName { get; }
	}
}
