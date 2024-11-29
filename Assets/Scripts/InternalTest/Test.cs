using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private readonly List<GameObject> m_LoopSound = new();
    protected void Start()
    {
        // UI测试
        //UIMgrTest();
        // 配置数据测试
        //ConfigTest();
        // 只读数据测试
        ReadonlyTest();
	}
	protected void Update()
    {
        // 缓存池测试
        //PoolMgrTest();
        // 音乐音效测试
        //MusicMgrTest();
    }
    
    void PoolMgrTest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PoolMgr.Instance.FetchAsync("Cube", Vector3.zero, Quaternion.identity);
        }
        if (Input.GetMouseButtonDown(1))
        {
            PoolMgr.Instance.FetchAsync("Sphere");
        }
    }

    void MusicMgrTest()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                MusicMgr.Instance.StopMusic();
            else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                MusicMgr.Instance.PauseMusic();
            else
                MusicMgr.Instance.PlayMusic("Music/BGM1");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                MusicMgr.Instance.PlaySoundAsync("Sound/AOE_Effect", true, (obj) =>
                {
                    m_LoopSound.Add(obj);
                });
            else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (m_LoopSound.Count > 0)
                {
                    MusicMgr.Instance.StopSound(m_LoopSound[0]);
                    m_LoopSound.RemoveAt(0);
                }
            }
            else
                MusicMgr.Instance.PlaySound("Sound/LaserShoot", false);
        }
    }

    void UIMgrTest()
    {
        UIMgr.Instance.ShowPanel<PanelBase>("UI/StartPanel");
    }

    void ConfigTest()
    {
		TestGlobalConfigJson tc = new TestGlobalConfigJson().InitInstance();
		TestGlobalConfigJson2 tc2 = new TestGlobalConfigJson2().InitInstance();
		TestGlobalConfigJson tc3 = new TestGlobalConfigJson().InitInstance();
    }

    void ReadonlyTest()
    {
        ExcelTestDB db = new ExcelTestDB().InitInstance() as ExcelTestDB;
    }
}

public class TestGlobalConfigJson : GlobalConfigJson<TestGlobalConfigJson>
{

}

public class TestGlobalConfigJson2 : GlobalConfigJson<TestGlobalConfigJson2>
{

}

public class ExcelTestDB : ReadonlyDB<ExcelTestData, ExcelTestData>
{

}