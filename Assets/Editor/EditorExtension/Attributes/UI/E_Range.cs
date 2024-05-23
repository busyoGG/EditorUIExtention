using System;

namespace EditorUIExtension
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method)]
    public class E_Range : Attribute
    {
        private float _start;

        private float _end;
        
        public E_Range(float start,float end)
        {
            _start = start;
            _end = end;
        }

        public float GetStart()
        {
            return _start;
        }

        public float GetEnd()
        {
            return _end;
        }
    }
}