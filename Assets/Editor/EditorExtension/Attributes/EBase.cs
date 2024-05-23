using System;
using System.Runtime.CompilerServices;

namespace EditorUIExtension
{
    public class EBase : Attribute
    {
        public int lineNum;

        public EBase([CallerLineNumber] int lineNumber = 0)
        {
            lineNum = lineNumber;
        }
    }
}