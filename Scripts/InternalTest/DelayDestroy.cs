using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDestroy : MonoBehaviour
{
    public float delay;
    void OnEnable()
    {
        Invoke("Push", delay);
    }

    void Push() => PoolMgr.Instance.Store(name, gameObject);
}
