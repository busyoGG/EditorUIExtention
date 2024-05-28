using System;
using UnityEngine;

namespace EditorUIExtension
{
    
    public class ES_Border: Attribute
    {
        private int _width;

        private int _t;

        private int _r;

        private int _b;

        private int _l;

        private bool _same;

        public ES_Border(int width)
        {
            _width = width;
            _same = true;
        }

        public ES_Border(int t, int r, int b, int l)
        {
            _t = t;
            _r = r;
            _b = b;
            _l = l;
            _same = false;
        }

        public bool IsSame()
        {
            return _same;
        }

        public int GetWidth()
        {
            return _width;
        }

        public IntVec4 GetAllBorder()
        {
            return new IntVec4(_t, _r, _b, _l);
        }
    }
}