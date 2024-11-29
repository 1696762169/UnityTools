using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public Vector3 speed;

    void Update()
    {
        transform.Translate(speed * Time.deltaTime);
    }
}
