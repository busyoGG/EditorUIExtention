

using System;
using UnityEngine;

public class EColor : Attribute
{
    private Color _color;

    public EColor(float r, float g, float b ,float a = 1)
    {
        _color = new Color(r, g, b, a);
    }

    public Color GetColor()
    {
        return _color;
    }
}
