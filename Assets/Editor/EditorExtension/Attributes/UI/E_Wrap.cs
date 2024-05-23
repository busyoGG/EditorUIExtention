using System;

namespace EditorUIExtension
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method)]
    public class E_Wrap : Attribute
    {
        private bool _isWrap;

        public E_Wrap(bool isWrap)
        {
            _isWrap = isWrap;
        }

        public bool GetWrap()
        {
            return _isWrap;
        }
    }

}