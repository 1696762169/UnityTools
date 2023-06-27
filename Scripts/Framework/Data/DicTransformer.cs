using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 方便字典进行Json存储的工具类
/// </summary>
public class DicTransformer<TKey, TValue>
{
    public Dictionary<string, TValue>.ValueCollection Values => m_Ref.Values;
    public Dictionary<string, TValue>.KeyCollection Keys => m_Ref.Keys;
    public TValue this[TKey key]
    {
        get => m_Ref[key.ToString()];
        set => m_Ref[key.ToString()] = value;
    }

    protected Dictionary<string, TValue> m_Ref;
    public DicTransformer(Dictionary<string, TValue> @ref)
    {
        m_Ref = @ref;
    }

    public void Add(TKey key, TValue value) => m_Ref.Add(key.ToString(), value);
    public void Remove(TKey key) => m_Ref.Remove(key.ToString());
    public int Count => m_Ref.Count;
    public bool ContainsKey(TKey key) => m_Ref.ContainsKey(key.ToString());
    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => m_Ref.GetEnumerator();
}
