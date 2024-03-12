using System;
using System.Runtime.CompilerServices;

/// <summary>
/// ��ǩ ����isCanChange���Ƿ����ݿɱ�
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class E_Label : EBase
{
    private bool _isCanChange;
    /// <summary>
    /// ��ǩ
    /// </summary>
    /// <param name="isCanChange">�Ƿ����ݿɱ�</param>
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
