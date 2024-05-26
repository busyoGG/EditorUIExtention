using System;

namespace EditorUIExtension
{
    public enum StyleFieldType
    {
        Inner,
        Outer,
        All
    }
    
    /// <summary>
    /// 样式作用范围
    /// </summary>
    public class StyleField: Attribute
    {
        private StyleFieldType _type;

        public StyleField(StyleFieldType type)
        {
            _type = type;
        }

        public StyleFieldType GetField()
        {
            return _type;
        }
    }
}