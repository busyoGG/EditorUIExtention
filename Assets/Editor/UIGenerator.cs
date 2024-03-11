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
    /// 生成Button
    /// </summary>
    /// <param name="buttonName">按钮名字</param>
    /// <param name="method">按钮方法</param>
    /// <param name="ins">实例</param>
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
    /// 生成Label
    /// </summary>
    /// <param name="labelName">标签名</param>
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
    /// 生成TextField
    /// </summary>
    /// <param name="res">输入参数字段</param>
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
