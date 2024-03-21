using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class UIGenerator
{
    private static readonly BindingFlags Flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
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

        void Button(GUIStyle style, GUILayoutOption[] options)
        {
            if (GUILayout.Button(buttonName, style, options))
            {
                click();
            }
        }

        return Button;
    }

    /// <summary>
    /// ����Label
    /// </summary>
    /// <param name="labelName">��ǩ��</param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateLabel(Func<string> labelName)
    {
        void Label(GUIStyle style, GUILayoutOption[] options)
        {
            GUILayout.Label(labelName(), style, options);
        }

        return Label;
    }

    /// <summary>
    /// ����TextField
    /// </summary>
    /// <param name="name"></param>
    /// <param name="res"></param>
    /// <param name="obj"></param>
    /// <param name="width"></param>
    /// <param name="isPercent"></param>
    /// <param name="doubleLine"></param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateInput(string name, string res, EditorWindow obj, int width, bool isPercent = true, bool doubleLine = false)
    {
        void Input(GUIStyle style, GUILayoutOption[] options)
        {
            if (doubleLine)
            {
                if (isPercent)
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(name, GUILayout.Width(obj.position.width * width / 100));
                    res = EditorGUILayout.TextField(res, style, options);
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(name, GUILayout.Width(width));
                    res = EditorGUILayout.TextField(res, style, options);
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                if (isPercent)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(name, GUILayout.Width(obj.position.width * width / 100));
                    res = EditorGUILayout.TextField(res, style, options);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(name, GUILayout.Width(width));
                    res = EditorGUILayout.TextField(res, style, options);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        return Input;
    }

    /// <summary>
    /// ����object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateObject<T>(string name, T obj) where T : UnityEngine.Object
    {
        void Action(GUIStyle style, GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(GetExtent(options));
            GUILayout.Label(name);
            obj = EditorGUILayout.ObjectField(obj, typeof(T), true, options) as T;
            EditorGUILayout.EndVertical();
        }

        return Action;
    }

    /// <summary>
    /// �����չ��Ĳ���
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    private static GUILayoutOption[] GetExtent(GUILayoutOption[] options)
    {
        List<GUILayoutOption> list = new List<GUILayoutOption>();
        foreach (var option in options)
        {
            Type type = option.GetType();
            float num;
            if (type.GetField("type", Flag)?.GetValue(option).ToString() == "fixedHeight")
            {
                num = (float)type.GetField("value", Flag)?.GetValue(option)!;
                list.Add(GUILayout.Height(num + 20f));
            }
            else if (type.GetField("type", Flag)?.GetValue(option).ToString() == "fixedWidth")
            {
                num = (float)type.GetField("value", Flag)?.GetValue(option)!;
                list.Add(GUILayout.Width(num + 6f));
            }
        }
        return list.ToArray();
    }
}
