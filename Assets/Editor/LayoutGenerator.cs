
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

public class LayoutGenerator
{
    public static Action<GUIStyle, GUILayoutOption[]> GenerateHorizontal(bool isStart)
    {
        Action<GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            if (isStart)
            {
                EditorGUILayout.BeginHorizontal(style, options);
            }
            else
            {
                GUILayout.EndHorizontal();
            }
        };
        return action;
    }

    public static Action<GUIStyle, GUILayoutOption[]> GenerateVertical(bool isStart)
    {
        Action<GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            if (isStart)
            {
                EditorGUILayout.BeginVertical(style, options);
            }
            else
            {
                GUILayout.EndVertical();
            }
        };
        return action;
    }
}
