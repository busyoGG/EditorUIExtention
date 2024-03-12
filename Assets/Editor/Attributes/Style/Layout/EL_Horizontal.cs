

using System;

public class EL_Horizontal : Attribute
{
    private bool _isStart;
    public EL_Horizontal(bool isStart)
    {
        _isStart = isStart;
    }

    public bool IsStart()
    {
        return _isStart;
    }
}
