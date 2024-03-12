

using System;
using UnityEngine;

public enum ESPercent
{
    None,
    All,
    Width,
    Height
}

public class ES_Size : Attribute
{
    /// <summary>
    /// ��С
    /// </summary>
    private Vector2 _size;
    /// <summary>
    /// �Ƿ������
    /// </summary>
    private ESPercent _isPercent;

    public ES_Size(float x, float y, ESPercent percent = ESPercent.None)
    {
        _size.Set(x, y);
        _isPercent = percent;
    }

    public ESPercent GetSizeType()
    {
        return _isPercent;
    }

    public Vector2 GetSize()
    {
        return _size;
    }

    public float GetWidth()
    {
        return _size.x;
    }

    public float GetHeight()
    {
        return _size.y;
    }
}
