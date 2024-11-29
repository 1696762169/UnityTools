using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using LitJson;

public abstract class ListTransformer<T>
{
    [NonSerializeJson]
    public abstract T this[int index] { get; set; }
    [SerializeJson]

    protected List<int> m_Ref = new List<int>();

    public int Count => m_Ref.Count;

    public abstract void Add(T value);
    public abstract void Remove(T value);
    public virtual void RemoveAt(int index) => m_Ref.RemoveAt(index);
    public abstract int IndexOf(T value);
    public abstract IEnumerator<T> GetEnumerator();
    public virtual void Clear() => m_Ref.Clear();
}
