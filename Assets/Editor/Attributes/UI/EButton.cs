using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method)]
public class EButton : EBase
{
    private string _name;
    public EButton(string name, [CallerLineNumber] int lineNumber = 0)
    {
        _name = name;
        _lineNum = lineNumber;
    }

    public string GetName()
    {
        return _name;
    }
}
