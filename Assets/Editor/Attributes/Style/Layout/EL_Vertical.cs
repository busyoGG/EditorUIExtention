

using System;
/// <summary>
/// ��ֱ���� ����isStart��true����Start��false����End
/// </summary>
public class EL_Vertical : Attribute
{
    private readonly bool _isStart;
    public EL_Vertical(bool isStart)
    {
        _isStart = isStart;
    }

    public bool IsStart()
    {
        return _isStart;
    }
}
