

using System;

/// <summary>
/// ˮƽ���� ����isStart��true����Start��false����End
/// </summary>
public class EL_Horizontal : Attribute
{
    private readonly bool _isStart;
    public EL_Horizontal(bool isStart)
    {
        _isStart = isStart;
    }

    public bool IsStart()
    {
        return _isStart;
    }
}
