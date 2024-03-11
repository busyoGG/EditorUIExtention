using System;

[AttributeUsage(AttributeTargets.Class)]
public class EName : Attribute
{
    private string _name;
    public EName(string name)
    {
        _name = name;
    }

    public string GetName()
    {
        return _name;
    }
}
