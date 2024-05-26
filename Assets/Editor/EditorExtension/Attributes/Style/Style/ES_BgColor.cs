

using System;
using UnityEngine;

namespace EditorUIExtension
{
    public class ES_BgColor : Attribute
    {
        private readonly Color _color;

        public ES_BgColor(float r, float g, float b ,float a = 1)
        {
            _color = new Color(r, g, b, a);
        }

        public Color GetColor()
        {
            return _color;
        }
    } 
}