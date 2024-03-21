using System.Runtime.CompilerServices;
using System;

/// <summary>
/// ����� ����width��Key���ֿ�ȣ�����isPercent���Ƿ�ٷֱȿ�ȣ�����isDoubleLine���Ƿ��������ʾ
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class E_Input : EBase
{
    private readonly bool _isDoubleLine;

    private readonly bool _isPercent;

    private readonly int _width;

    public E_Input(int width = 30,bool isPercent = true,bool isDoubleLine = false, [CallerLineNumber] int lineNumber = 0)
    {
        _width = width;
        _isPercent = isPercent;
        _isDoubleLine = isDoubleLine;
        lineNum = lineNumber;
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
