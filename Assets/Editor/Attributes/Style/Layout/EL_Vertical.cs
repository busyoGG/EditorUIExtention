

using System;

public class EL_Vertical : Attribute
{
    private bool _isStart;
    public EL_Vertical(bool isStart)
    {
        _isStart = isStart;
    }

    public bool IsStart()
    {
        return _isStart;
    }
}
