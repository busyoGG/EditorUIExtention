using System;

namespace EditorUIExtension
{
    public enum WidthType
    {
        Percent,
        Pixel
    }
    
    /// <summary>
    /// 调整带有标签的UI元素的标签宽度
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method)]
    public class E_Width: Attribute
    {
        private float _width;

        private WidthType _widthType;
        
        public E_Width(float width,WidthType type)
        {
            _width = width;
            _widthType = type;
        }

        public float GetWidth()
        {
            return _width;
        }

        public WidthType GetWidthType()
        {
            return _widthType;
        }
    }
}