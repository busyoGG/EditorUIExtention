using System;

[Serializable]
public struct SValue<TValue>
{
    public TValue Value;

    public SValue(TValue value)
    {
        Value = value;
    }
}
