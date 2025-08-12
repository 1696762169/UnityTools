using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

/// <summary>
/// 缓存池类
/// 可使用多种方式加载缓存对象
/// </summary>
public class PoolMgr : SingletonBase<PoolMgr>
{
	private readonly GameObject m_PoolParent;
	private readonly Dictionary<string, Pool> m_Pools = new();
	public PoolMgr()
	{
		m_PoolParent = new GameObject("POOL_ROOT");
		Object.DontDestroyOnLoad(m_PoolParent);
	}

	/// <summary>
	/// 从Resources缓存池中同步获取对象
	/// </summary>
	/// <param name="resourcesPath">Resources文件夹中的资源路径</param>
	/// <param name="customPoolName">自定义缓存池名称 默认使用资源路径作为名称</param>
	public GameObject FetchResources(string resourcesPath, string customPoolName = null)
	{
		string poolName = GetPoolName(PoolType.Resources, customPoolName ?? resourcesPath);
		return Fetch(poolName, resourcesPath, Resources.Load<GameObject>);
	}

	/// <summary>
	/// 从Resources缓存池中同步获取对象 并设置该对象位置与旋转信息
	/// </summary>
	/// <param name="resourcesPath">Resources文件夹中的资源路径</param>
	/// <param name="position">全部坐标位置</param>
	/// <param name="rotation">全局旋转</param>
	/// <param name="customPoolName">自定义缓存池名称 默认使用资源路径作为名称</param>
	public GameObject FetchResources(string resourcesPath, Vector3 position, Quaternion rotation = default, string customPoolName = null)
	{
		string poolName = GetPoolName(PoolType.Resources, customPoolName ?? resourcesPath);
		GameObject obj = Fetch(poolName, resourcesPath, Resources.Load<GameObject>);
		OnLoaded(obj, position, rotation);
		return obj;
	}

	/// <summary>
	/// 从Resources缓存池中同步获取对象
	/// </summary>
	/// <param name="resourcesPath">Resources文件夹中的资源路径</param>
	/// <param name="customPoolName">自定义缓存池名称 默认使用资源路径作为名称</param>
	/// <param name="token">取消加载标记</param>
	public async UniTask<GameObject> FetchResourcesAsync(string resourcesPath, string customPoolName = null, CancellationToken token = default)
	{
		string poolName = GetPoolName(PoolType.Resources, customPoolName ?? resourcesPath);
		return await FetchAsync(poolName, resourcesPath, ResourcesLoadAsync);
	}

	/// <summary>
	/// 从Resources缓存池中同步获取对象 并设置该对象位置与旋转信息
	/// </summary>
	/// <param name="resourcesPath">Resources文件夹中的资源路径</param>
	/// <param name="position">全部坐标位置</param>
	/// <param name="rotation">全局旋转</param>
	/// <param name="customPoolName">自定义缓存池名称 默认使用资源路径作为名称</param>
	/// <param name="token">取消加载标记</param>
	public async UniTask<GameObject> FetchResourcesAsync(string resourcesPath, Vector3 position, Quaternion rotation = default, string customPoolName = null, CancellationToken token = default)
	{
		string poolName = GetPoolName(PoolType.Resources, customPoolName ?? resourcesPath);
		GameObject obj = await FetchAsync(poolName, resourcesPath, ResourcesLoadAsync);
		OnLoaded(obj, position, rotation);
		return obj;
	}

	/// <summary>
	/// 向缓存池归还对象
	/// </summary>
	/// <param name="resPath">Resources文件夹中的资源路径</param>
	/// <param name="obj">待归还的物体</param>
	public void Store(string resPath, GameObject obj)
	{
		if (!m_Pools.ContainsKey(resPath))
		{
			// 创建缓存池和场景上的父对象
			GameObject parent = new(resPath);
			parent.transform.SetParent(m_PoolParent.transform);
			m_Pools.Add(resPath, new Pool(resPath));
		}
		m_Pools[resPath].Push(obj);
	}

	/// <summary>
	/// 清除缓存池中的全部内容
	/// </summary>
	public void Clear()
	{
		foreach (string resPath in m_Pools.Keys)
			Clear(resPath);
	}
	/// <summary>
	/// 清除单个缓存池中的内容
	/// </summary>
	public void Clear(string resPath)
	{
		if (!m_Pools.ContainsKey(resPath)) return;
		Object.Destroy(m_Pools[resPath].parent.gameObject);
		m_Pools.Remove(resPath);
	}

	#region 缓存池名称相关

	/// <summary>
	/// 缓存池类型
	/// </summary>
	public enum PoolType
	{
		Addressables,
		Resources,
		Temporary,
	}

	/// <summary>
	/// 按类型获取标准格式的缓存池名称
	/// </summary>
	/// <param name="poolType">缓存池类型</param>
	/// <param name="key">缓存池名称键 一般是资源路径</param>
	public string GetPoolName(PoolType poolType, string key) => poolType switch
	{
		PoolType.Addressables => POOL_PREFIX_AA + key,
		PoolType.Resources => POOL_PREFIX_RES + key,
		PoolType.Temporary => POOL_PREFIX_TEMP + key,
		_ => null,
	};

	private string GetPoolNameCheckPrefix(PoolType poolType, string key)
	{
		if (key.StartsWith(POOL_PREFIX_AA) ||
			key.StartsWith(POOL_PREFIX_RES) ||
			key.StartsWith(POOL_PREFIX_TEMP))
			return key;
		return GetPoolName(poolType, key);
	}

	public const string POOL_PREFIX_AA = "AA_";
	public const string POOL_PREFIX_RES = "RES_";
	public const string POOL_PREFIX_TEMP = "TEMP_";

	#endregion

	private GameObject Fetch(string poolName, string resPath, System.Func<string, GameObject> createFunc)
	{
		if (!m_Pools.TryGetValue(poolName, out Pool pool))
			pool = m_Pools[poolName] = new Pool(poolName);

		GameObject ret =  pool.Count > 0 ? pool.Pop() : createFunc(resPath);
		ret.name = poolName;
		return ret;
	}

	private async UniTask<GameObject> FetchAsync(string poolName, string resPath, System.Func<string, UniTask<GameObject>> createFunc)
	{
		if (!m_Pools.TryGetValue(poolName, out Pool pool))
			pool = m_Pools[poolName] = new Pool(poolName);

		GameObject ret = pool.Count > 0 ? pool.Pop() : await createFunc(resPath);
		ret.name = poolName;
		return ret;
	}

	private async UniTask<GameObject> ResourcesLoadAsync(string resourcesPath)
	{
		var ret = await Resources.LoadAsync<GameObject>(resourcesPath);
		return ret as GameObject;
	}

	private async UniTask<GameObject> AddressablesLoadAsync(string addressablePath)
	{
		var handle = Addressables.LoadAssetAsync<GameObject>(addressablePath);
		return await handle.Task;
	}

	private static void OnLoaded(GameObject obj, Vector3 position, Quaternion rotation)
	{
		if (rotation == default)
			rotation = Quaternion.identity;
		obj.transform.SetPositionAndRotation(position, rotation);
	}

	private class Pool
	{
		public readonly Transform parent;
		public readonly string name;

		private readonly Stack<GameObject> m_Objects = new();
		public int Count => m_Objects.Count;

		public Pool(string poolName)
		{
			GameObject parentObj = new GameObject("POOL_" + poolName);
			parentObj.transform.SetParent(PoolMgr.Instance.m_PoolParent.transform);
			parent = parentObj.transform;
			name = poolName;
		}
		public void Push(GameObject obj, bool moveToParent = true)
		{
			obj.SetActive(false);
			m_Objects.Push(obj);
			if (moveToParent)
				obj.transform.SetParent(parent);
		}
		public GameObject Pop()
		{
			GameObject ret = m_Objects.Pop();
			ret.SetActive(true);
			return ret;
		}
	}
}