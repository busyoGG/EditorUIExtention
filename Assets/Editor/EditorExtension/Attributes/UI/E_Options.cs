using System;

namespace EditorUIExtension
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method)]
    public class E_Options : Attribute
    {
        private string[] _options;

        public E_Options(params string[] options)
        {
            _options = options;
        }

        public string[] GetOptions()
        {
            return _options;
        }
    }

}