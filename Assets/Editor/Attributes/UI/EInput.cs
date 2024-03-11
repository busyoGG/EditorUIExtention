using System.Runtime.CompilerServices;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class EInput : EBase
{
    public EInput([CallerLineNumber] int lineNumber = 0)
    {
        _lineNum = lineNumber;
    }
}
