
using UnityEngine;
using System;
using System.Collections.Generic;

public struct UIListData
{
    public Action<GUIStyle, GUILayoutOption[]> action;
    public Func<GUIStyle> style;
    public Func<GUILayoutOption[]> options;
}

public class LayoutNode
{
    public List<object> _list = new List<object>();

    //public List<LayoutNode> _children = new List<LayoutNode>();

    public Action _layout;

    public LayoutNode parent { get; set; }

    public void AddUi(UIListData listData)
    {
        _list.Add(listData);
    }

    public void AddUi(LayoutNode listData)
    {
        _list.Add(listData);
        listData.parent = this;
    }

    public void SetLayout(Action<GUIStyle, GUILayoutOption[]> layout, (Func<GUIStyle>, Func<GUILayoutOption[]>) layoutStyle)
    {
        _layout = ()=> {
            layout(layoutStyle.Item1(),layoutStyle.Item2());
        };
        //_layoutStyle = layoutStyle;
    }
}
