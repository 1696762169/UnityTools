using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// 缓存池类
/// 可使用多种方式加载缓存对象
/// </summary>
public class PoolMgr : SingletonBase<PoolMgr>
{
	private readonly Transform m_PoolParent;
	private readonly Transform m_PoolParentPermanent;
	private readonly Dictionary<string, Pool> m_Pools = new();
	public PoolMgr()
	{
		m_PoolParent = new GameObject("POOL_ROOT").transform;
		m_PoolParentPermanent = new GameObject("POOL_ROOT_PERMANENT").transform;
		Object.DontDestroyOnLoad(m_PoolParentPermanent);
	}

	#region Addressables API

	/// <summary>
	/// 从Addressables缓存池中异步获取对象
	/// </summary>
	/// <param name="addressablesKey">Addressables资源Key</param>
	/// <param name="customPoolName">自定义缓存池名称 默认使用资源路径作为名称</param>
	public async UniTask<GameObject> FetchAsync(string addressablesKey, string customPoolName = null)
	{
		string poolName = GetPoolName(PoolType.Addressables, customPoolName ?? addressablesKey);
		return await FetchAsync(poolName, addressablesKey, AddressablesLoadAsync);
	}

	/// <summary>
	/// 从Addressables缓存池中异步获取对象 并设置该对象位置与旋转信息
	/// </summary>
	/// <param name="addressablesKey">Addressables资源Key</param>
	/// <param name="position">全局坐标位置</param>
	/// <param name="rotation">全局旋转</param>
	/// <param name="customPoolName">自定义缓存池名称 默认使用资源路径作为名称</param>
	public async UniTask<GameObject> FetchAsync(string addressablesKey, Vector3 position, Quaternion rotation = default, string customPoolName = null)
	{
		string poolName = GetPoolName(PoolType.Addressables, customPoolName ?? addressablesKey);
		GameObject obj = await FetchAsync(poolName, addressablesKey, AddressablesLoadAsync);
		OnLoaded(obj, position, rotation);
		return obj;
	}

	/// <summary>
	/// 从Addressables缓存池中异步获取对象
	/// </summary>
	/// <param name="location">Addressables资源地址</param>
	/// <param name="customPoolName">自定义缓存池名称 默认使用资源路径作为名称</param>
	public async UniTask<GameObject> FetchAsync(IResourceLocation location, string customPoolName = null)
	{
		string poolName = GetPoolName(PoolType.Addressables, customPoolName ?? location.PrimaryKey);
		return await FetchAsync(poolName, location, AddressablesLoadAsync);
	}

	/// <summary>
	/// 向Addressables缓存池归还对象
	/// </summary>
	/// <param name="addressablesKey">Addressables资源Key</param>
	/// <param name="obj">待归还的物体</param>
	public void Store(string addressablesKey, GameObject obj)
	{
		string poolName = GetPoolName(PoolType.Addressables, addressablesKey);
		StoreInternal(poolName, obj);
	}

	#endregion

	#region Resources API

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
	/// <param name="position">全局坐标位置</param>
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
	/// 从Resources缓存池中异步获取对象
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
	/// 从Resources缓存池中异步获取对象 并设置该对象位置与旋转信息
	/// </summary>
	/// <param name="resourcesPath">Resources文件夹中的资源路径</param>
	/// <param name="position">全局坐标位置</param>
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
	/// 向Addressables缓存池归还对象
	/// </summary>
	/// <param name="resourcesPath">Resources文件夹中的资源路径</param>
	/// <param name="obj">待归还的物体</param>
	public void StoreResources(string resourcesPath, GameObject obj)
	{
		string poolName = GetPoolName(PoolType.Resources, resourcesPath);
		StoreInternal(poolName, obj);
	}

	#endregion

	/// <summary>
	/// 清除单个缓存池中的内容
	/// </summary>
	/// <param name="poolType">缓存池类型</param>
	/// <param name="key">缓存池名称键 一般是资源路径</param>
	public void Clear(PoolType poolType, string key)
	{
		string poolName = GetPoolName(poolType, key);
		if (!m_Pools.TryGetValue(poolName, out var pool))
			return;
		pool.Dispose();
	}

	/// <summary>
	/// 清除单个缓存池中的内容
	/// </summary>
	/// <param name="key">缓存池名称键 可猜测类型</param>
	/// <exception cref="System.ArgumentException">存在多个可能与key匹配的缓存池</exception>
	public void Clear(string key)
	{
		if (!m_Pools.TryGetValue(key, out var pool))
		{
			string[] possibleKeys = { GetPoolName(PoolType.Addressables, key), GetPoolName(PoolType.Resources, key), GetPoolName(PoolType.Temporary, key) };
			string[] existKeys = possibleKeys.Where(possibleKey => m_Pools.ContainsKey(possibleKey)).ToArray();
			switch (existKeys.Length)
			{
			case 0:
				return;
			case > 1:
				throw new System.ArgumentException("Multiple pools found for key: " + key);
			default:
				pool = m_Pools[existKeys[0]];
				break;
			}
		}
		pool.Dispose();
	}

	/// <summary>
	/// 清除缓存池中的全部内容
	/// </summary>
	public void ClearAll()
	{
		Pool[] pools = m_Pools.Values.ToArray();
		foreach (Pool pool in pools)
			pool.Dispose();
	}

	/// <summary>
	/// 获取缓存池中对象的数量
	/// </summary>
	/// <param name="poolType">缓存池类型</param>
	/// <param name="key">缓存池名称键 一般是资源路径</param>
	public int GetObjectCount(PoolType poolType, string key)
	{
		string poolName = GetPoolNameCheckPrefix(poolType, key);
		return m_Pools.TryGetValue(poolName, out var pool) ? pool.Count : 0;
	}

	/// <summary>
	/// 判断缓存池是否存在
	/// </summary>
	/// <param name="poolType">缓存池类型</param>
	/// <param name="key">缓存池名称键 一般是资源路径</param>
	public bool IsPoolExist(PoolType poolType, string key)
	{
		string poolName = GetPoolNameCheckPrefix(poolType, key);
		return m_Pools.ContainsKey(poolName);
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

	#region 内部实现

	private GameObject Fetch(string poolName, string resPath, System.Func<string, GameObject> createFunc)
	{
		if (!m_Pools.TryGetValue(poolName, out Pool pool))
			pool = m_Pools[poolName] = new Pool(poolName);

		GameObject ret;
		if (pool.Count > 0)
		{
			ret = pool.Pop();
		}
		else
		{
			GameObject prefab = createFunc(resPath);
			ret = Object.Instantiate(prefab, pool.parent);
		}

		ret.name = poolName;
		return ret;
	}

	private async UniTask<GameObject> FetchAsync<T>(string poolName, T resPath, System.Func<T, UniTask<GameObject>> createFunc)
	{
		if (!m_Pools.TryGetValue(poolName, out Pool pool))
			pool = m_Pools[poolName] = new Pool(poolName);

		GameObject ret;
		if (pool.Count > 0)
		{
			ret = pool.Pop();
		}
		else
		{
			GameObject prefab = await createFunc(resPath);
			ret = Object.Instantiate(prefab, pool.parent);
		}

		ret.name = poolName;
		return ret;
	}

	private void StoreInternal(string poolName, GameObject obj)
	{
		if (!m_Pools.TryGetValue(poolName, out var pool))
			pool = m_Pools[poolName] = new Pool(poolName);
		pool.Push(obj);
	}

	private static async UniTask<GameObject> ResourcesLoadAsync(string resourcesPath)
	{
		var ret = await Resources.LoadAsync<GameObject>(resourcesPath);
		return ret as GameObject;
	}

	private static async UniTask<GameObject> AddressablesLoadAsync(string addressablePath)
	{
		var handle = Addressables.LoadAssetAsync<GameObject>(addressablePath);
		return await handle.Task;
	}
	private static async UniTask<GameObject> AddressablesLoadAsync(IResourceLocation resourceLocation)
	{
		var handle = Addressables.LoadAssetAsync<GameObject>(resourceLocation);
		return await handle.Task;
	}

	private static void OnLoaded(GameObject obj, Vector3 position, Quaternion rotation)
	{
		if (rotation == default)
			rotation = Quaternion.identity;
		obj.transform.SetPositionAndRotation(position, rotation);
	}

    #endregion

	private class Pool : System.IDisposable
	{
		public readonly Transform parent;
		private readonly string m_Name;

		private readonly Stack<GameObject> m_Objects = new();
		public int Count => m_Objects.Count;

		public Pool(string poolName)
		{
			GameObject parentObj = new GameObject("POOL_" + poolName);
			parentObj.transform.SetParent(PoolMgr.Instance.m_PoolParent);
			parent = parentObj.transform;
			m_Name = poolName;
		}
		public void Push(GameObject obj, bool moveToParent = true)
		{
			obj.SetActive(false);
			m_Objects.Push(obj);
			// TODO: 配置是否自动设置父对象
			if (moveToParent)
				obj.transform.SetParent(parent);
		}
		public GameObject Pop()
		{
			GameObject ret = m_Objects.Pop();
			ret.SetActive(true);
			return ret;
		}

		public void Dispose()
		{
			Object.Destroy(parent.gameObject);
			PoolMgr.Instance.m_Pools.Remove(m_Name);
		}
	}
}