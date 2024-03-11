using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Field)]
public class ELabel : EBase
{
    public ELabel([CallerLineNumber] int lineNumber = 0) {
        _lineNum = lineNumber;
    }
}
