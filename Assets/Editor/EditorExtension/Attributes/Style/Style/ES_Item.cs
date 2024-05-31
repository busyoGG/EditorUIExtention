using System;

namespace EditorUIExtension
{
    public class ES_Item: Attribute
    {
        private float _width;

        private float _height;

        private bool _grow;
        
        public ES_Item(float width,float height)
        {
            _width = width;
            _height = height;
        }

        public ES_Item(bool grow)
        {
            _grow = grow;
        }

        public float GetWidth()
        {
            return _width;
        }

        public float GetHeight()
        {
            return _height;
        }

        public bool GetGrow()
        {
            return _grow;
        }
    }
}