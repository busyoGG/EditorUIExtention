using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class UIGenerator
{
    /// <summary>
    /// ����Button
    /// </summary>
    /// <param name="buttonName">��ť����</param>
    /// <param name="method">��ť����</param>
    /// <param name="ins">ʵ��</param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateButton(string buttonName, MethodInfo method, object ins)
    {
        Action click = (Action)Delegate.CreateDelegate(typeof(Action), ins, method);

        Action<GUIStyle, GUILayoutOption[]> button = (GUIStyle style, GUILayoutOption[] options) =>
        {
            if (GUILayout.Button(buttonName, style, options))
            {
                click();
            }
        };

        return button;
    }

    /// <summary>
    /// ����Label
    /// </summary>
    /// <param name="labelName">��ǩ��</param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateLabel(string labelName)
    {
        Action<GUIStyle, GUILayoutOption[]> label = (GUIStyle style, GUILayoutOption[] options) =>
        {
            GUILayout.Label(labelName, style, options);
        };

        return label;
    }

    /// <summary>
    /// ����TextField
    /// </summary>
    /// <param name="res">��������ֶ�</param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateInput(string res)
    {
        Action<GUIStyle, GUILayoutOption[]> input = (GUIStyle style, GUILayoutOption[] options) =>
        {
            res = GUILayout.TextField(res, style, options);
        };

        return input;
    }
}
