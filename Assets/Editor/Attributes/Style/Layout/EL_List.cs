

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

    private EL_ListType _listType;

    private bool _scroll;

    private float _width;

    private float _height;

    private ESPercent _percent;

    private Vector2 _scrollPosition = Vector2.zero;

    public EL_List(EL_ListType listType)
    {
        _listType = listType;
    }

    public EL_List(EL_ListType listType, bool scroll, float width, float height,ESPercent percent = ESPercent.None)
    {
        _listType = listType;
        _scroll = scroll;
        _width = width;
        _height = height;
        _percent = percent;
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
}
