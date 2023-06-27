using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    [Header("ʹ���������")]
    public bool useRandomSeed;

    [Header("����ѡ�����ڿ�����������")]
    [Tooltip("��������ģʽ")]
    public bool test;
    [Tooltip("�ڲ���ģʽ�±������ݵ��ļ�")]
    public bool saveTestData;
    //[Space(10)]
    // ��ʾ�Ƿ����ڽ��г�ʼ��
    public  bool Initializing { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        /* ��ʼ��ֻ������ */

        /* ��ʼ����Ϸ���� */
        SeedMgr.Instance.GlobalSeed = 0;
        SeedMgr.Instance.InitInstance();

        /* ��ʼ������ʱ���� */

        /* ��ʼ��������ʾ���� */

        /* ���ȫ���¼� */
    }

    /// <summary>
    /// ��ʼ���浵������ԭ�浵����
    /// </summary>
    public void InitDataAndSave()
    {
        Initializing = true;

        /* �ڴ˴����г�ʼ����浵���� */

        Initializing = false;
    }

    // ��ʼ���������ֵ������
    protected void InitView<T>() where T : Component
    {
        if (GetComponent<T>() == null)
            transform.AddComponent<T>();
    }
}
