using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    [Header("设定的种子数")]
    public int seed;
    [Header("使用随机种子")]
    public bool useRandomSeed;

    [Header("以下选项用于开启测试数据")]
    [Tooltip("开启测试模式")]
    public bool test;
    [Tooltip("在测试模式下保存数据到文件")]
    public bool saveTestData;
    //[Space(10)]
    // 表示是否正在进行初始化
    public  bool Initializing { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        /* 初始化只读数据 */

        /* 初始化游戏种子 */
        //SeedMgr.Instance.GlobalSeed = 0;
        //SeedMgr.Instance.InitInstance();

        /* 初始化运行时数据 */

        /* 初始化各类显示界面 */

        /* 添加全局事件 */
    }

    /// <summary>
    /// 初始化存档并覆盖原存档数据
    /// </summary>
    public void InitDataAndSave()
    {
        Initializing = true;

        /* 在此处进行初始化与存档覆盖 */

        Initializing = false;
    }

    // 初始化其它各种单例组件
    protected void InitView<T>() where T : Component
    {
        if (GetComponent<T>() == null)
            transform.AddComponent<T>();
    }
}
