using System;

[Serializable]
public struct SKeyValue<TKey,TValue>
{
    public TKey Key;
    public TValue Value;

    public SKeyValue(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}