using System.Runtime.CompilerServices;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class E_Input : EBase
{
    private bool _isDoubleLine;

    private bool _isPercent;

    private int _width;
    public E_Input(int width,bool isPercent = true,bool isDoubleLine = false, [CallerLineNumber] int lineNumber = 0)
    {
        _width = width;
        _isPercent = isPercent;
        _isDoubleLine = isDoubleLine;
        _lineNum = lineNumber;
    }

    public bool IsPercent()
    {
        return _isPercent;
    }

    public int GetWidth()
    {
        return _width;
    }

    public bool IsDoubleLine()
    {
        return _isDoubleLine;
    }
}
