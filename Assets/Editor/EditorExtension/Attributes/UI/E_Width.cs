using System;

namespace EditorUIExtension
{
    public enum WidthType
    {
        Percent,
        Pixel
    }
    
    /// <summary>
    /// �������б�ǩ��UIԪ�صı�ǩ���
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