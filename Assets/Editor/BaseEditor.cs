using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseEditor<T> : EditorWindow where T : EditorWindow
{
    private Type _type;
    private List<Action> _onGUI = new List<Action>();

    private BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private void OnEnable()
    {
        //获得类型
        _type = GetType();

        //初始化窗口名称
        EName ename = _type.GetCustomAttribute<EName>();
        InitWindow(ename.GetName());

        //初始化UI
        Init();
    }

    protected void InitWindow(string name)
    {
        T win = GetWindow<T>();
        win.titleContent = new GUIContent(name);
    }

    private void Init()
    {
        Type type = GetType();

        var members = type.GetMembers(flag)
                            .Select(m => new
                            {
                                Member = m,
                                Attribute = m.GetCustomAttributes(typeof(EBase), false).FirstOrDefault() as EBase
                            })
                            .Where(x => x.Attribute != null)
                            .OrderBy(x => x.Attribute._lineNum);

        foreach (var item in members)
        {
            MemberInfo member = item.Member;
            Debug.Log(member.Name + " 行数" + item.Attribute._lineNum);
            var propAttrs = member.GetCustomAttributes(true);

            if (member.MemberType == MemberTypes.Field)
            {
                foreach (var prop in propAttrs)
                {
                    switch (prop)
                    {
                        case ELabel:
                            FieldInfo feild = (FieldInfo)member;
                            GenerateLabel((string)feild.GetValue(this));
                            break;
                    }
                }
            }
            else if (member.MemberType == MemberTypes.Method)
            {
                foreach (var prop in propAttrs)
                {
                    switch (prop)
                    {
                        case EButton:
                            EButton attr = (EButton)prop;
                            GenerateButton(attr.GetName(), (MethodInfo)member);
                            break;
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        foreach (var action in _onGUI)
        {
            action();
        }
    }

    private void GenerateButton(string buttonName, MethodInfo method)
    {
        //Debug.Log("创建按钮: " + buttonName);
        Action click = (Action)Delegate.CreateDelegate(typeof(Action), this, method);

        Action button = () =>
        {
            if (GUILayout.Button(buttonName))
            {
                click();
            }
        };

        _onGUI.Add(button);
    }

    private void GenerateLabel(string labelName)
    {
        Debug.Log("创建Label: " + labelName);
        Action label = () =>
        {
            GUILayout.Label(labelName);
        };

        _onGUI.Add(label);
    }
}
