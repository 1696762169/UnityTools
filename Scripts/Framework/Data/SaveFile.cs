using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家运行时需存储的所有数据
/// </summary>
public class SaveFile
{
	public int SaveIndex { get; protected set; } = 1;

	public SaveFile(int saveIndex)
	{
		SaveIndex = saveIndex;
	}
	public void SaveData()
	{
		// 存储所有数据
	}
}
