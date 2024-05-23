using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorUIExtension
{
    public class UIGenerator
    {
        private static readonly BindingFlags Flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                                    BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 生成Button
        /// </summary>
        /// <param name="buttonName">按钮名字</param>
        /// <param name="method">按钮方法</param>
        /// <param name="ins">实例</param>
        /// <returns></returns>
        public static Action<GUIStyle, GUILayoutOption[]> GenerateButton(string buttonName, MethodInfo method,
            object ins)
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
        /// 生成Label
        /// </summary>
        /// <param name="labelName">标签名</param>
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
        /// 生成Enum
        /// </summary>
        /// <param name="def"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Action<GUIStyle, GUILayoutOption[]> GenerateEnum<T>(string name, T def, Action<object> setVal,
            EditorWindow obj,float width, bool isPercent = true, bool doubleLine = false) where T : Enum
        {
            void EnumPop(GUIStyle style, GUILayoutOption[] options)
            {
                Action generator = () =>
                {
                    def = (T)EditorGUILayout.EnumPopup(def, style);
                    setVal(def);
                };
                GenerateLabelAndInputField(name, generator, obj, width, options, isPercent, doubleLine);
            }

            return EnumPop;
        }

        /// <summary>
        /// 创建滑动条
        /// </summary>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="duration"></param>
        /// <param name="obj"></param>
        /// <param name="width"></param>
        /// <param name="isPercent"></param>
        /// <param name="doubleLine"></param>
        /// <returns></returns>
        public static Action<GUIStyle, GUILayoutOption[]> GenerateSlider(string name, float start, float end,
            float duration, Action<object> setVal, EditorWindow obj, float width, bool isPercent = true,
            bool doubleLine = false)
        {
            void Slider(GUIStyle style, GUILayoutOption[] options)
            {
                Action generator = () =>
                {
                    duration = EditorGUILayout.Slider(duration, start, end);
                    setVal(duration);
                };
                GenerateLabelAndInputField(name, generator, obj, width, options, isPercent, doubleLine);
            }

            return Slider;
        }

        /// <summary>
        /// 创建整型滑动条
        /// </summary>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="duration"></param>
        /// <param name="obj"></param>
        /// <param name="width"></param>
        /// <param name="isPercent"></param>
        /// <param name="doubleLine"></param>
        /// <returns></returns>
        public static Action<GUIStyle, GUILayoutOption[]> GenerateSlider(string name, int start, int end,
            int duration, Action<object> setVal, EditorWindow obj, float width, bool isPercent = true,
            bool doubleLine = false)
        {
            void Slider(GUIStyle style, GUILayoutOption[] options)
            {
                Action generator = () =>
                {
                    duration = EditorGUILayout.IntSlider(duration, start, end);
                    setVal(duration);
                };
                GenerateLabelAndInputField(name, generator, obj, width, options, isPercent, doubleLine);
            }

            return Slider;
        }

        /// <summary>
        /// 生成TextField
        /// </summary>
        /// <param name="name"></param>
        /// <param name="res"></param>
        /// <param name="obj"></param>
        /// <param name="width"></param>
        /// <param name="isPercent"></param>
        /// <param name="doubleLine"></param>
        /// <returns></returns>
        public static Action<GUIStyle, GUILayoutOption[]> GenerateInput(string name, string res, Action<object> setVal,
            EditorWindow obj, float width, bool isPercent = true, bool doubleLine = false)
        {
            void Input(GUIStyle style, GUILayoutOption[] options)
            {
                Action generator = () =>
                {
                    res = EditorGUILayout.TextField(res, style);
                    setVal(res);
                };
                GenerateLabelAndInputField(name, generator, obj, width, options, isPercent, doubleLine);
            }

            return Input;
        }

        public static Action<GUIStyle, GUILayoutOption[]> GenerateRadio(string[] selections, int selection,
            Action<object> setVal)
        {
            void Radio(GUIStyle style, GUILayoutOption[] options)
            {
                for (int i = 0; i < selections.Length; i++)
                {
                    if (GUILayout.Toggle(selection == i, selections[i]))
                    {
                        selection = i;
                        setVal(i);
                    }
                }
            }

            return Radio;
        }

        /// <summary>
        /// 绘制object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Action<GUIStyle, GUILayoutOption[]> GenerateObject<T>(string name, T obj, Action<object> setVal)
            where T : UnityEngine.Object
        {
            void Action(GUIStyle style, GUILayoutOption[] options)
            {
                EditorGUILayout.BeginVertical(GetExtent(options));
                GUILayout.Label(name);
                obj = EditorGUILayout.ObjectField(obj, typeof(T), true, options) as T;
                setVal(obj);
                EditorGUILayout.EndVertical();
            }

            return Action;
        }

        /// <summary>
        /// 获得扩展后的布局
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

        /// <summary>
        /// 创建标签和输入区
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="obj"></param>
        /// <param name="width"></param>
        /// <param name="isPercent"></param>
        /// <param name="doubleLine"></param>
        private static void GenerateLabelAndInputField(string name, Action action, EditorWindow obj, float width
            , GUILayoutOption[] options, bool isPercent = true, bool doubleLine = false)

        {
            if (doubleLine)
            {
                if (isPercent)
                {
                    EditorGUILayout.BeginVertical(options);
                    GUILayout.Label(name, GUILayout.Width(obj.position.width * width / 100));
                    action();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(name, GUILayout.Width(width));
                    action();
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                if (isPercent)
                {
                    EditorGUILayout.BeginHorizontal(options);
                    GUILayout.Label(name, GUILayout.Width(obj.position.width * width / 100));
                    action();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(name, GUILayout.Width(width));
                    action();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}