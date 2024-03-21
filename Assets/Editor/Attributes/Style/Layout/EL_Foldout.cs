

using System;

/// <summary>
/// ˮƽ���� ����isStart��true����Start��false����End
/// </summary>
public class EL_Foldout : Attribute
{
    private readonly bool _isStart;

    private readonly string _name;

    private bool _isOpen = true;
    public EL_Foldout(bool isStart, string name = "")
    {
        _isStart = isStart;
        _name = name;
    }

    public bool IsStart()
    {
        return _isStart;
    }

    public bool IsOpen()
    {
        return _isOpen;
    }

    public void IsOpen(bool isOpen)
    {
        _isOpen = isOpen;
    }

    public string Name()
    {
        return _name;
    }
}
