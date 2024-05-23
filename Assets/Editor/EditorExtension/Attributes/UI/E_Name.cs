using System;

namespace EditorUIExtension
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method)]
    public class E_Name : Attribute
    {
        private readonly string _name;
        public E_Name(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }
    }
}