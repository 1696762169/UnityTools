using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using Regex = System.Text.RegularExpressions.Regex;

[TestFixture, TestMustExpectAllLogs]
public class TestEventMgr
{
	private const string EVENT_1 = "Event_1";
	private const string EVENT_ANOTHER = "Event_Another";

	private const string UNEXPECTED_STRING = "bad luck";

	private EventMgr m_Mgr;
	
	[SetUp]
	public void SetUp()
	{
		m_Mgr = EventMgr.Instance;
		AssertNoEvent(m_Mgr);
	}

	[TearDown]
	public void ClearAll()
	{
		m_Mgr.ClearAll();
		m_Mgr = null;
	}

	[Test, Description("事件全生命周期测试")]
	public void BasicTest()
	{
		bool eventTriggered = false;

		// 添加无参监听
		UnityAction action = () => eventTriggered = true;
		Assert.IsTrue(m_Mgr.AddListener(EVENT_1, action));

		// 触发事件
		Assert.IsTrue(m_Mgr.TriggerEvent(EVENT_1));
		Assert.IsTrue(eventTriggered);

		// 移除监听
		Assert.IsTrue(m_Mgr.RemoveListener(EVENT_1, action));

		// 验证移除后无法触发
		eventTriggered = false;
		ExpectEventNotFound(EVENT_1);
		Assert.IsFalse(m_Mgr.TriggerEvent(EVENT_1));
		Assert.IsFalse(eventTriggered);
	}


	[Test, Description("类型安全测试")]
	public void TypeSafetyTest()
	{
		TestObject obj = new TestObject();

		// 添加string类型监听
		Assert.IsTrue(m_Mgr.AddListener<string>(EVENT_1, obj.StringParam));
		Assert.IsTrue(m_Mgr.AddListener(EVENT_ANOTHER, obj.NoParam));

		// 尝试用错误类型触发
		ExpectTypeMismatch(OperationType.Trigger, EVENT_1, typeof(string), null);
		ExpectTypeMismatch(OperationType.Trigger, EVENT_1, typeof(string), typeof(int));
		ExpectTypeMismatch(OperationType.Trigger, EVENT_ANOTHER, null, typeof(string));
		Assert.IsFalse(m_Mgr.TriggerEvent(EVENT_1));
		Assert.IsFalse(m_Mgr.TriggerEvent(EVENT_1, 123));
		Assert.IsFalse(m_Mgr.TriggerEvent(EVENT_ANOTHER, UNEXPECTED_STRING));

		// 尝试添加错误类型的监听
		ExpectTypeMismatch(OperationType.Add, EVENT_1, typeof(string), null);
		ExpectTypeMismatch(OperationType.Add, EVENT_1, typeof(string), typeof(int));
		ExpectTypeMismatch(OperationType.Add, EVENT_ANOTHER, null, typeof(string));
		Assert.IsFalse(m_Mgr.AddListener(EVENT_1, () => { }));
		Assert.IsFalse(m_Mgr.AddListener<int>(EVENT_1, _ => { }));
		Assert.IsFalse(m_Mgr.AddListener<string>(EVENT_ANOTHER, value => { }));

		// 尝试移除错误类型的监听
		ExpectTypeMismatch(OperationType.Remove, EVENT_1, typeof(string), null);
		ExpectTypeMismatch(OperationType.Remove, EVENT_1, typeof(string), typeof(int));
		ExpectTypeMismatch(OperationType.Remove, EVENT_ANOTHER, null, typeof(string));
		Assert.IsFalse(m_Mgr.RemoveListener(EVENT_1, () => { }));
		Assert.IsFalse(m_Mgr.RemoveListener<int>(EVENT_1, _ => { }));
		Assert.IsFalse(m_Mgr.RemoveListener<string>(EVENT_ANOTHER, _ => { }));
	}

	[Test, Description("多播测试")]
	public void MulticastTest()
	{
		TestObject obj1 = new TestObject();
		TestObject obj2 = new TestObject();
		int callCount = 0;
		const string testString = "hello";

		// 添加多播监听
		Assert.IsTrue(m_Mgr.AddListener<string>(EVENT_1, _ => callCount++));
		Assert.IsTrue(m_Mgr.AddListener<string>(EVENT_1, obj1.StringParam));
		Assert.IsTrue(m_Mgr.AddListener<string>(EVENT_1, obj2.ReversedParam));

		// 正常触发事件
		Assert.IsTrue(m_Mgr.TriggerEvent(EVENT_1, testString));
		Assert.AreEqual(1, callCount);
		Assert.AreEqual(testString, obj1.Param);
		Assert.AreEqual(testString.Reverse().ToString(), obj2.Param);

		// 移除与添加监听
		Assert.IsTrue(m_Mgr.RemoveListener<string>(EVENT_1, obj1.StringParam));
		Assert.IsTrue(m_Mgr.AddListener<string>(EVENT_1, obj2.StringParam));
		Assert.IsTrue(m_Mgr.TriggerEvent(EVENT_1, UNEXPECTED_STRING));
		Assert.AreEqual(2, callCount);
		Assert.AreEqual(testString, obj1.Param);
		Assert.AreEqual(UNEXPECTED_STRING, obj2.Param);

		// 移除不存在的监听
		ExpectListenerNotFound(EVENT_1);
		Assert.IsFalse(m_Mgr.RemoveListener<string>(EVENT_1, obj1.StringParam));

		// 重复添加事件 测试移除仅移除一个
		obj2.Param = "";
		Assert.IsTrue(m_Mgr.AddListener<string>(EVENT_1, obj2.StringParam));
		Assert.IsTrue(m_Mgr.RemoveListener<string>(EVENT_1, obj2.StringParam));
		Assert.IsTrue(m_Mgr.TriggerEvent(EVENT_1, testString));
		Assert.AreEqual(3, callCount);
		Assert.AreEqual(testString, obj2.Param);

		// 移除所有监听 并 尝试触发不存在的事件
		ExpectEventNotFound(EVENT_1);
		Assert.IsTrue(m_Mgr.Clear(EVENT_1));
		Assert.IsFalse(m_Mgr.TriggerEvent(EVENT_1, "Nothing"));
		Assert.AreEqual(3, callCount);
		Assert.AreEqual(testString, obj1.Param);
		Assert.AreEqual(testString, obj2.Param);
	}


	// 用反射检查m_Events字典中包含的事件数量
	private static void AssertNoEvent(EventMgr mgr)
	{
		Assert.IsNotNull(mgr);

		// 反射获取m_Events字典
		var eventsField = typeof(EventMgr).GetField("m_Events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.IsNotNull(eventsField);

		var countProperty = eventsField.FieldType.GetProperty("Count");
		Assert.IsNotNull(countProperty);
		object count = countProperty.GetValue(eventsField.GetValue(mgr));

		Assert.AreEqual(0, count);
	}

	public enum OperationType
	{
		Add,
		Remove,
		Trigger
	}

	private static void ExpectEventNotFound(string eventName)
	{
		Regex pattern = new($@"Event\s+'{Regex.Escape(eventName)}'\s+not\s+found", RegexOptions.IgnoreCase);
		LogAssert.Expect(LogType.Warning, pattern);
	}

	private static void ExpectListenerNotFound(string eventName)
	{
		Regex pattern = new($@"Remove\s+Event\s+'{eventName}'.*not\s+found", RegexOptions.IgnoreCase);
		LogAssert.Expect(LogType.Warning, pattern);
	}

	private static void ExpectTypeMismatch(OperationType opType, string eventName, Type expectedType, Type actualType)
	{
		string opName = opType.ToString();
		string expectedTypeName = expectedType?.FullName ?? "null";
		string actualTypeName = actualType?.FullName ?? "";
		actualTypeName = actualTypeName == "" ? "no" : $"'{actualTypeName}'";

		string pattern = $@"{opName}\s+Event\s+'{Regex.Escape(eventName)}'.*" +
		                 $@"type\s+'{Regex.Escape(expectedTypeName)}'.*" +
		                 $@"got\s+{Regex.Escape(actualTypeName)}";

		LogAssert.Expect(LogType.Warning, new Regex(pattern, RegexOptions.IgnoreCase));
	}

	internal class TestObject
	{
		public const string LOG_NO_PARAM = "Method 'NoParam' called";

		public string Param { get; set; }

		public void NoParam()
		{
			Debug.Log(LOG_NO_PARAM);
		}

		public void StringParam(string param)
		{
			Param = param;
		}

		public void ReversedParam(string param)
		{
			Param = param.Reverse().ToString();
		}
	}
}

