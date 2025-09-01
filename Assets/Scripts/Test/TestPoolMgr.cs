using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;
using Task = System.Threading.Tasks.Task;


[TestFixture]
public class TestPoolMgr
{
	public const int TEST_COUNT = 10000;
	public const string TEST_ADDRESSABLES_PATH = "Art/Model/Capsule";
	public const string TEST_RESOURCES_PATH = "Cube";

	private PoolMgr m_Mgr;

	[SetUp]
	public void SetUp()
	{
		m_Mgr = PoolMgr.Instance;
		AssertNoPool(m_Mgr);
	}

	[UnityTearDown]
	public IEnumerator ClearAll()
	{
		m_Mgr.ClearAll();
		m_Mgr = null;
		yield return new WaitForEndOfFrame(); // 等待所有缓存池对象Destroy完毕
	}

	[Test, Description("基础功能测试 - Addressables")]
	[TestCaseSource(nameof(PoolBaseTestCases))]
	public async Task BaseAddressablesTest(PoolMgr.PoolType poolType, string path, bool isAsync)
	{
		// 获取对象
		string poolName = m_Mgr.GetPoolName(poolType, path);
		GameObject obj = await FetchGameObject(poolType, path, isAsync);
		Assert.IsNotNull(obj);
		Assert.AreEqual(poolName, obj.name);
		Assert.Zero(m_Mgr.GetObjectCount(poolType, path));

		// 回收对象
		StoreGameObject(poolType, path, obj);
		Assert.IsNotNull(obj);
		Assert.IsFalse(obj.activeSelf);
		Assert.AreEqual(1, m_Mgr.GetObjectCount(poolType, path));


		// 再次获取对象
		GameObject anotherObj = await FetchGameObject(poolType, path, isAsync);
		Assert.AreEqual(obj, anotherObj);
		Assert.IsNotNull(obj);
		Assert.AreEqual(poolName, obj.name);
		Assert.Zero(m_Mgr.GetObjectCount(poolType, path));
		
		// 清理缓存池
		StoreGameObject(poolType, path, obj);
		m_Mgr.Clear(poolName);
		Assert.Zero(m_Mgr.GetObjectCount(poolType, path));

		// 再次获取对象
		GameObject newObj = await FetchGameObject(poolType, path, isAsync);
		Assert.IsNotNull(newObj);
		Assert.AreNotEqual(obj, newObj);
		Assert.AreEqual(poolName, newObj.name);
		Assert.Zero(m_Mgr.GetObjectCount(poolType, path));
	}

	private static IEnumerable PoolBaseTestCases()
	{
		yield return new object[] { PoolMgr.PoolType.Addressables, TEST_ADDRESSABLES_PATH, true };
		yield return new object[] { PoolMgr.PoolType.Resources, TEST_RESOURCES_PATH, true };
		yield return new object[] { PoolMgr.PoolType.Resources, TEST_RESOURCES_PATH, false };
	}

	private async UniTask<GameObject> FetchGameObject(PoolMgr.PoolType poolType, string path, bool isAsync)
	{
		switch (poolType)
		{
		case PoolMgr.PoolType.Addressables:
		{
			return await m_Mgr.FetchAsync(path);
		}
		case PoolMgr.PoolType.Resources:
		{
			if (isAsync)
			{
				return await m_Mgr.FetchResourcesAsync(path);
			}
			else
			{
				// ReSharper disable once MethodHasAsyncOverload
				return m_Mgr.FetchResources(path);
			}
		}
		}
		return null;
	}

	private void StoreGameObject(PoolMgr.PoolType poolType, string path, GameObject obj)
	{
		switch (poolType)
		{
		case PoolMgr.PoolType.Addressables:
			m_Mgr.Store(path, obj);
			break;
		case PoolMgr.PoolType.Resources:
			m_Mgr.StoreResources(path, obj);
			break;
		}
	}

	// 用反射检查当前不存在Pool对象
	private static void AssertNoPool(PoolMgr mgr)
	{
		Assert.IsNotNull(mgr);

		// 反射获取缓存池根对象
		var parentField = typeof(PoolMgr).GetField("m_PoolParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var permanentField = typeof(PoolMgr).GetField("m_PoolParentPermanent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.IsNotNull(parentField);
		Assert.IsNotNull(permanentField);

		Transform parent = parentField.GetValue(mgr) as Transform;
		Transform permanent = permanentField.GetValue(mgr) as Transform;

		// 检查缓存池根对象是否为空
		Assert.IsNotNull(parent);
		Assert.IsNotNull(permanent);

		// 检查缓存池根对象下是否有Pool对象
		if (parent.childCount > 0)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				EasyLogger.Log(child.name);
			}
		}
		Assert.AreEqual(0, parent.childCount);
		Assert.AreEqual(0, permanent.childCount);

		// 反射获取缓存池字典
		var poolDictField = typeof(PoolMgr).GetField("m_Pools", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.IsNotNull(poolDictField);
		var countProperty = poolDictField.FieldType.GetProperty("Count");
		Assert.IsNotNull(countProperty);

		// 检查字典中没有缓存池记录
		object count = countProperty.GetValue(poolDictField.GetValue(mgr));
		Assert.AreEqual(0, count);
	}
}
