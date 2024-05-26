using System;
using UnityEngine;

namespace EditorUIExtension
{
    
    public class ES_Border: Attribute
    {
        private int _width;

        private int _tl;

        private int _tr;

        private int _bl;

        private int _br;

        private bool _same;

        public ES_Border(int width)
        {
            _width = width;
            _same = true;
        }

        public ES_Border(int tl, int tr, int bl, int br)
        {
            _tl = tl;
            _tr = tr;
            _bl = bl;
            _br = br;
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
            return new IntVec4(_tl, _tr, _bl, _br);
        }
    }
}