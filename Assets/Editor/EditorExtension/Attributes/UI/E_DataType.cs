using System;

namespace EditorUIExtension
{
    public enum DataType
    {
        Int,
        Float,
        String,
        Double,
        Bool
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method)]
    public class E_DataType : Attribute
    {
        private DataType _dataType;

        public E_DataType(DataType dataType)
        {
            _dataType = dataType;
        }

        public DataType GetDataType()
        {
            return _dataType;
        }
    }

}