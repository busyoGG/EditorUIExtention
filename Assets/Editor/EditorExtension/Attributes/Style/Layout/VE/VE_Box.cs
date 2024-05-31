using System;
using System.Runtime.CompilerServices;

namespace EditorUIExtension
{
    public class VE_Box: EBase
    {
        private bool _isHorizontal;

        private bool _isCreate;

        private string _boxName;

        private bool _isFold;

        public VE_Box(bool isHorizontal = false,bool isFold = false,string boxName = "",[CallerLineNumber] int lineNumber = 0): base(lineNumber)
        {
            _isCreate = true;
            _isHorizontal = isHorizontal;
            _isFold = isFold;
            _boxName = boxName;
        }

        public VE_Box(string name,[CallerLineNumber] int lineNumber = 0): base(lineNumber)
        {
            _boxName = name;
        }

        public string GetName()
        {
            return _boxName;
        }

        public bool IsCreate()
        {
            return _isCreate;
        }

        public bool IsFold()
        {
            return _isFold;
        }

        public bool IsHorizontal()
        {
            return _isHorizontal;
        }
    }
}