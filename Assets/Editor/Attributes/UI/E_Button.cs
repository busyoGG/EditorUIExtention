using System;
using System.Runtime.CompilerServices;

/// <summary>
/// ��ť ����name����ť����
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class E_Button : EBase
{
    private string _name;
    public E_Button(string name, [CallerLineNumber] int lineNumber = 0)
    {
        _name = name;
        _lineNum = lineNumber;
    }

    public string GetName()
    {
        return _name;
    }
}
