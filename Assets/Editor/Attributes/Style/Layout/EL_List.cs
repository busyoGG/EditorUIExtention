

using System;
using UnityEngine;

public enum EL_ListType
{
    Horizontal,
    Verticle,
    Flex
}

/// <summary>
/// 列表布局 参数isStart：true代表Start，false代表End
/// </summary>
public class EL_List : Attribute
{
    private bool _isStart;

    private EL_ListType _listType;

    private bool _scroll;

    private float _width;

    private float _height;

    private ESPercent _percent;

    private Vector2 _scrollPosition = Vector2.zero;

    private bool _isSingle;

    public EL_List(bool isStart, EL_ListType listType, bool isSingle)
    {
        _isStart = isStart;
        _listType = listType;
        _isSingle = isSingle;
    }

    public EL_List(bool isStart, EL_ListType listType, bool isSingle, bool scroll, float width, float height, ESPercent percent = ESPercent.None)
    {
        _isStart = isStart;
        _listType = listType;
        _scroll = scroll;
        _width = width;
        _height = height;
        _percent = percent;
        _isSingle = isSingle;
    }

    public EL_ListType ListType()
    {
        return _listType;
    }

    public bool Scroll()
    {
        return _scroll;
    }

    public float Width()
    {
        return _width;
    }

    public float Height()
    {
        return _height;
    }

    public Vector2 ScrollPosition()
    {
        return _scrollPosition;
    }

    public void ScrollPosition(Vector2 position)
    {
        _scrollPosition = position;
    }

    public ESPercent GetPercent()
    {
        return _percent;
    }

    public bool IsStart()
    {
        return _isStart;
    }

    public bool IsSingle()
    {
        return _isSingle;
    }
}
