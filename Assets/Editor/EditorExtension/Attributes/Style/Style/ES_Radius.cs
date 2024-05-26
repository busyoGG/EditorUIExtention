using System;
using UnityEngine;

namespace EditorUIExtension
{
    
    public class ES_Radius: Attribute
    {
        private int _radius;

        private int _tl;

        private int _tr;

        private int _bl;

        private int _br;

        private bool _same;

        public ES_Radius(int radius)
        {
            _radius = radius;
            _same = true;
        }

        public ES_Radius(int tl, int tr, int bl, int br)
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

        public int GetRadius()
        {
            return _radius;
        }

        public IntVec4 GetAllRadius()
        {
            return new IntVec4(_tl, _tr, _bl, _br);
        }
    }
}