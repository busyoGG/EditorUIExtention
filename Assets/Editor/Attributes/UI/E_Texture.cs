using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Field)]
public class E_Texture : EBase
{
    public E_Texture([CallerLineNumber] int lineNumber = 0)
    {
        _lineNum = lineNumber;
    }
}
