using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class SeedMgr : RuntimeData<SeedMgr>
{
    [NonSerializedField]
    public override string FilePath => $"{Application.persistentDataPath}/Seed.json";

    // 用于生成其它种子的全局种子
    public int GlobalSeed { get; set; }
    // 记录所有游戏中所使用的种子
    [SerializeField]
    protected Dictionary<string, SeedData> m_Seeds = new Dictionary<string, SeedData>();

    // 当前使用的种子数值
    [NonSerializedField]
    public int CurrentSeed => m_Seeds[CurrentSeedName].Seed;
    // 当前使用的种子名
    public string CurrentSeedName { get; protected set; }
    // 当前使用的种子数据
    protected SeedData m_CurrentSeed => m_Seeds[CurrentSeedName];

    public override void InitData()
    {
        m_Seeds = new Dictionary<string, SeedData>();
#if UNITY_EDITOR
        if (GameManager.Instance.useRandomSeed)
            GlobalSeed = Random.Range(0, int.MaxValue / 2);
#endif
        Random.InitState(GlobalSeed);

        foreach (FieldInfo key in typeof(SeedName).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            m_Seeds.Add(key.GetValue(null).ToString(), new SeedData(Random.Range(0, int.MaxValue / 2)));
        }
    }
#if UNITY_EDITOR
    protected override void LoadData()
    {
        if (!GameManager.Instance.useRandomSeed)
            base.LoadData();
    }
#endif

    // 检测SeedName类是否发生了改变 并保存改变后的数据
    protected override void InitExtend()
    {
        bool changed = false;
        foreach (FieldInfo key in typeof(SeedName).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            string seedName = key.GetValue(null).ToString();
            if (!m_Seeds.ContainsKey(seedName))
            {
                m_Seeds.Add(seedName, new SeedData(Random.Range(0, int.MaxValue / 2)));
                changed = true;
            }
        }
        if (changed)
            SaveData();
    }

    /// <summary>
    /// 设定当前使用的种子
    /// </summary>
    /// <param name="seedName">使用的种子名</param>
    public void UseSeed(string seedName)
    {
        if (!m_Seeds.ContainsKey(seedName))
            throw new System.ArgumentException("使用了不存在的种子：" + seedName);

        CurrentSeedName = seedName;
    }

    /// <summary>
    /// 使用随机种子 保护原本使用的种子
    /// </summary>
    public void UseRandomSeed()
    {
        CurrentSeedName = "随机种子";
        Random.InitState((int)(System.DateTime.Now.Ticks >> 20));
    }

    /* 仿照Random类提供接口 */
    public static float value
    {
        get
        {
            Instance.m_CurrentSeed.Use();
            return Random.value;
        }
    }
    public static int Range(int minInclusive, int maxExclusive)
    {
        Instance.m_CurrentSeed.Use();
        return Random.Range(minInclusive, maxExclusive);
    }
    public static float Range(float minInclusive, float maxInclusive)
    {
        Instance.m_CurrentSeed.Use();
        return Random.Range(minInclusive, maxInclusive);
    }


    // 用于记录的种子数据类
    protected class SeedData
    {
        public int Seed => m_Seed + m_Offset;   // 当前所使用的实际种子
        protected int m_Seed;       // 原始的种子值
        protected int m_Offset;     // 种子偏移量
        public SeedData(int seed)
        {
            m_Seed = seed;
            m_Offset = 0;
        }

        // 每次使用该种子后都应该进行记录
        public void Use()
        {
            ++m_Offset;
            Random.InitState(Seed);
        }
    }
}
