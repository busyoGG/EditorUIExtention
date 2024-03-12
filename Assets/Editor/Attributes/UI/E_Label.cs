using System;
using System.Runtime.CompilerServices;

/// <summary>
/// 标签 参数isCanChange：是否内容可变
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class E_Label : EBase
{
    private bool _isCanChange;
    /// <summary>
    /// 标签
    /// </summary>
    /// <param name="isCanChange">是否内容可变</param>
    /// <param name="lineNumber"></param>
    public E_Label(bool isCanChange= false, [CallerLineNumber] int lineNumber = 0) {
        _lineNum = lineNumber;
        _isCanChange = isCanChange;
    }

    public bool IsCanChange()
    {
        return _isCanChange;
    }
}
