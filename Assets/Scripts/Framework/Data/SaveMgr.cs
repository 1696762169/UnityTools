using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 存档数据管理类
/// </summary>
public class SaveMgr : RuntimeConfig<SaveMgr>, IGetOriginValue<MetaData>
{
    // 存档元数据字典
    private readonly Dictionary<int, MetaData> m_MetaData = new();
    [SerializeField]
    private readonly Dictionary<int, MetaData.MetaDataRaw> m_RawData = new();

    // 存储时间格式字符串
    public const string TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

    // 当前使用的存档
    public int CurSaveIndex => CurSaveFile.SaveIndex;
	public SaveFile CurSaveFile { get; protected set; }
	// 最大存档数
	public const int MAX_SAVE_COUNT = 3;

    /// <summary>
    /// 加载某一实例的玩家存档
    /// </summary>
    public void LoadData(SaveFile saveFile)
    {
        switch (saveFile.SaveIndex)
        {
        case < 0:
        case > MAX_SAVE_COUNT:
	        throw new ArgumentException($"存档索引：【{saveFile.SaveIndex}】超出范围");
        case 0 when !GameManager.Instance.test:
	        throw new ArgumentException("非测试模式下不可初始化0号存档");
        }

        CurSaveFile = saveFile;
        
        // 更新元数据
        UpdateMetaData();
    }

    /// <summary>
    /// 存储当前的存档
    /// </summary>
    public override void SaveData()
    {
        // 更新元数据
        UpdateMetaData();

        // 存储元数据
        base.SaveData();

        if (!m_RawData.ContainsKey(CurSaveIndex))
        {
            Debug.LogWarning($"待存储的{CurSaveIndex}号存档不存在");
            return;
        }

		/* 存储所有管理类数据 */
        CurSaveFile.SaveData();
    }

	/// <summary>
	/// 获取一个存档数据 null表示没有存档
	/// </summary>
	public MetaData GetOriginValue(int index)
    {
        m_MetaData.TryGetValue(index, out MetaData metaData);
        return metaData;
    }
    /// <summary>s
    /// 获取所有存档数据
    /// </summary>
    public IEnumerable<MetaData> GetAllOriginValue()
    {
	    return m_MetaData.Values;
    }

    // 更新当前的元数据
    protected void UpdateMetaData()
    {
        // 设置元数据
        m_RawData[CurSaveIndex] = MetaData.MetaDataRaw.UpdateMetaData(CurSaveIndex);

        // 加载刚刚存储的截图
        m_MetaData[CurSaveIndex] = new MetaData(m_RawData[CurSaveIndex]);
    }

    protected override void InitData()
    {
        m_RawData.Clear();
        m_MetaData.Clear();
    }

    protected override void PostProcess()
    {
	    // 读取存档数据
	    foreach (MetaData metaData in m_RawData.Values.Select(rawData => new MetaData(rawData)))
	    {
		    m_MetaData.Add(metaData.ID, metaData);
	    }
    }
}

/// <summary>
/// 存档元数据
/// </summary>
public class MetaData : IUnique
{
	public int ID { get; protected set; }

	public MetaData(MetaDataRaw raw)
	{
		ID = raw.ID;
	}

	// 用于存储的数据类

	public class MetaDataRaw : IUnique
	{
		public int ID { get; set; }

		public static MetaDataRaw UpdateMetaData(int index)
		{
            return new MetaDataRaw()
            {
				ID = index,
			};
		}
	}
}
