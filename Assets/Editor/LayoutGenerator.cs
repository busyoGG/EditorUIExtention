
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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



    public static Action<GUIStyle, GUILayoutOption[]> GenerateList(bool isStart, EL_List elList, EditorWindow obj)
    {
        Action<GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            Vector2 size = CalSize(elList.GetPercent(), new Vector2(elList.Width(), elList.Height()), obj);
            switch (elList.ListType())
            {
                case EL_ListType.Verticle:
                    if (elList.Scroll())
                    {
                        if (isStart)
                        {
                            elList.ScrollPosition(EditorGUILayout.BeginScrollView(elList.ScrollPosition(), style,GUILayout.Height(size.y), GUILayout.Width(size.x)));
                            EditorGUILayout.BeginVertical();
                        }
                        else
                        {
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndScrollView();
                        }
                    }
                    else
                    {
                        if (isStart)
                        {
                            EditorGUILayout.BeginVertical(style, GUILayout.Height(size.y), GUILayout.Width(size.x));
                        }
                        else
                        {
                            EditorGUILayout.EndVertical();
                        }
                    }
                    break;
                case EL_ListType.Horizontal:
                case EL_ListType.Flex:
                    if (elList.Scroll())
                    {
                        if (isStart)
                        {
                            elList.ScrollPosition(EditorGUILayout.BeginScrollView(elList.ScrollPosition(), style, GUILayout.Height(size.y), GUILayout.Width(size.x)));
                            EditorGUILayout.BeginHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndScrollView();
                        }
                    }
                    else
                    {
                        if (isStart)
                        {
                            EditorGUILayout.BeginHorizontal(style, GUILayout.Height(size.y), GUILayout.Width(size.x));
                        }
                        else
                        {
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    break;
            }
        };
        return action;
    }

    /// <summary>
    /// º∆À„Size
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="vec2Size"></param>
    /// <returns></returns>
    public static Vector2 CalSize(ESPercent percent, Vector2 vec2Size,EditorWindow obj)
    {
        switch (percent)
        {
            case ESPercent.All:
                vec2Size = obj.position.size * vec2Size / 100;
                break;
            case ESPercent.Width:
                vec2Size.x = obj.position.width * vec2Size.x / 100;
                break;
            case ESPercent.Height:
                vec2Size.y = obj.position.height * vec2Size.y / 100;
                break;
        }
        return vec2Size;
    }
}
