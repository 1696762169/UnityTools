using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using LitJson;

public class SeedMgr : RuntimeDM<SeedMgr>
{
	// 用于生成其它种子的全局种子
    public int GlobalSeed { get; set; }
    private const string GLOBAL_SEED_NAME = "GlobalSeed";
    private const string RANDOM_SEED_NAME = "随机种子";

	// 记录所有游戏中所使用的种子
	[SerializeJson]
    private Dictionary<string, SeedData> m_Seeds = new();

    // 当前使用的种子数值
    [NonSerializeJson]
    public int CurrentSeed => m_Seeds[CurrentSeedName].Seed;
    // 当前使用的种子名
    public string CurrentSeedName { get; private set; }
    // 当前使用的种子数据
    private SeedData CurrentSeedData => m_Seeds[CurrentSeedName];

    protected int TimeSeed => (int)(System.DateTime.Now.Ticks >> 20);

	protected override void InitData()
    {
        m_Seeds = new Dictionary<string, SeedData>();

        // 设定全局种子
        GlobalSeed = GameManager.Instance.useRandomSeed ? Random.Range(0, int.MaxValue / 2) : GameManager.Instance.seed;
        Random.InitState(GlobalSeed);

        foreach (FieldInfo key in typeof(SeedName).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            m_Seeds.Add(key.GetValue(null).ToString(), new SeedData(Random.Range(0, int.MaxValue / 2)));
        }
    }

    protected override SeedMgr LoadData()
    {
        if (!GameManager.Instance.useRandomSeed)
            return base.LoadData();

        SeedMgr instance = new();
        instance.PreProcess();
        instance.InitData();
        return instance;
	}


    // 检测SeedName类是否发生了改变 并保存改变后的数据
    protected override void PostProcess()
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

        if (!m_Seeds.ContainsKey(GLOBAL_SEED_NAME))
        {
	        m_Seeds[GLOBAL_SEED_NAME] = new SeedData(GlobalSeed);
	        changed = true;
        }
        if (!m_Seeds.ContainsKey(RANDOM_SEED_NAME))
        {
	        m_Seeds[RANDOM_SEED_NAME] = new SeedData(TimeSeed);
	        changed = true;
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
        CurrentSeedName = RANDOM_SEED_NAME;
        Random.InitState(TimeSeed);
    }

    /* 仿照Random类提供接口 */
    public float Value
    {
        get
        {
            CurrentSeedData.Use();
            return Random.value;
        }
    }
    public int Range(int minInclusive, int maxExclusive)
    {
        CurrentSeedData.Use();
        return Random.Range(minInclusive, maxExclusive);
    }
    public float Range(float minInclusive, float maxInclusive)
    {
        CurrentSeedData.Use();
        return Random.Range(minInclusive, maxInclusive);
    }


	// 用于记录的种子数据类
	public class SeedData : IUnique
	{
		public int ID => 1;
		public int Seed { get; protected set; } // 当前所使用的实际种子
		public SeedData() { }
		public SeedData(int seed)
		{
			Seed = seed;
		}

		// 每次使用该种子后都应该进行记录
		public void Use()
		{
			if (Seed < int.MaxValue / 3)
				Seed = Seed * 3 + 11;
			else
				Seed /= 17;
			//if (Seed < int.MaxValue / 3)
			//    Seed = Seed * 3 + 1;
			//else
			//    Seed %= 7;
		}
	}
}
