using System;
using System.Runtime.CompilerServices;

namespace EditorUIExtension
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
    public class E_Editor : EBase
    {
        private EType _type;

        /// <summary>
        /// ±Í«©
        /// </summary>
        /// <param name="lineNumber"></param>
        public E_Editor(EType type, [CallerLineNumber] int lineNumber = 0) : base(lineNumber)
        {
            _type = type;
        }

        public EType GetEType()
        {
            return _type;
        }
    }
}