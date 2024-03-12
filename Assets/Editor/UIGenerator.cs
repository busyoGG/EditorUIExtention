using System;
using System.Reflection;
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
    public static Action<GUIStyle, GUILayoutOption[]> GenerateLabel(Func<string> labelName)
    {
        Action<GUIStyle, GUILayoutOption[]> label = (GUIStyle style, GUILayoutOption[] options) =>
        {

            GUILayout.Label(labelName(), style, options);
        };

        return label;
    }

    /// <summary>
    /// ����TextField
    /// </summary>
    /// <param name="res">��������ֶ�</param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateInput(string name, string res, EditorWindow obj, int width, bool isPercent = true, bool doubleLine = false)
    {
        Action<GUIStyle, GUILayoutOption[]> input = (GUIStyle style, GUILayoutOption[] options) =>
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
        };

        return input;
    }

    /// <summary>
    /// ����texture
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateObject<T>(string name, T obj) where T : UnityEngine.Object
    {
        Debug.Log("����Object");
        Action<GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label(name);
            obj = EditorGUILayout.ObjectField(obj, typeof(T), true, options) as T;
            EditorGUILayout.EndVertical();
        };
        return action;
    }
}
