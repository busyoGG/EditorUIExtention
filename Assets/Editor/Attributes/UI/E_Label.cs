using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Field)]
public class E_Label : EBase
{
    private bool _isCanChange;
    public E_Label(bool isCanChange, [CallerLineNumber] int lineNumber = 0) {
        _lineNum = lineNumber;
        _isCanChange = isCanChange;
    }

    public bool IsCanChange()
    {
        return _isCanChange;
    }
}
