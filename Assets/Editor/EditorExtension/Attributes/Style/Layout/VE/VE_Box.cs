using System;
using System.Runtime.CompilerServices;

namespace EditorUIExtension
{
    public class VE_Box: EBase
    {
        private bool _isHorizontal;

        private bool _isCreate;

        private string _boxName;

        public VE_Box(bool isCreate,bool isHorizontal = false,[CallerLineNumber] int lineNumber = 0): base(lineNumber)
        {
            _isCreate = isCreate;
            _isHorizontal = isHorizontal;
        }

        public VE_Box(string name,[CallerLineNumber] int lineNumber = 0): base(lineNumber)
        {
            _boxName = name;
        }

        public VE_Box(string name, bool isCreate, bool isHorizontal = false,[CallerLineNumber] int lineNumber = 0): base(lineNumber)
        {
            _boxName = name;
            _isCreate = isCreate;
            _isHorizontal = isHorizontal;
        }

        public string GetName()
        {
            return _boxName;
        }

        public bool IsCreate()
        {
            return _isCreate;
        }

        public bool IsHorizontal()
        {
            return _isHorizontal;
        }
    }
}