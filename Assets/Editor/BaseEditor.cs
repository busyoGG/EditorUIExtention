using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class BaseEditor<T> : EditorWindow where T : EditorWindow
{
    private Type _type;
    private List<Action<GUIStyle, GUILayoutOption[]>> _onGUI = new List<Action<GUIStyle, GUILayoutOption[]>>();

    private List<Func<GUIStyle>> _onStyle = new List<Func<GUIStyle>>();

    //private List<>

    private BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private void OnEnable()
    {
        //�������
        _type = GetType();

        //��ʼ����������
        EName ename = _type.GetCustomAttribute<EName>();
        InitWindow(ename.GetName());

        //��ʼ��UI
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

        //ɸѡ�����༭����չ���ͻ���Ķ���
        var members = type.GetMembers(flag)
                            .Select(m => new
                            {
                                Member = m,
                                Attribute = m.GetCustomAttributes(typeof(EBase), false).FirstOrDefault() as EBase
                            })
                            .Where(x => x.Attribute != null)
                            .OrderBy(x => x.Attribute._lineNum);

        //�������󣬴������ɺ���
        foreach (var item in members)
        {
            MemberInfo member = item.Member;
            var propAttrs = member.GetCustomAttributes(true);

            List<object> styles = new List<object>();
            object ui = null;

            //�����������ԣ���װ����ʽ��ui
            foreach (var prop in propAttrs)
            {
                Type attrType = prop.GetType();
                if (attrType.Name.IndexOf("ES") != -1)
                {
                    styles.Add(prop);
                }
                else
                {
                    ui = prop;
                }
            }

            //������ʽ���ƺ���
            SetStyle(ui, styles);

            //����ui
            if (member.MemberType == MemberTypes.Field)
            {
                //�ֶ�
                switch (ui)
                {
                    case ELabel:
                        FieldInfo field = (FieldInfo)member;
                        GenerateUI(UIGenerator.GenerateLabel((string)field.GetValue(this)));
                        break;
                    case EInput:
                        FieldInfo input = (FieldInfo)member;
                        GenerateUI(UIGenerator.GenerateInput((string)input.GetValue(this)));
                        break;
                }
            }
            else if (member.MemberType == MemberTypes.Method)
            {
                //����
                switch (ui)
                {
                    case EButton:
                        EButton attr = (EButton)ui;
                        GenerateUI(UIGenerator.GenerateButton(attr.GetName(), (MethodInfo)member, this));
                        break;
                }

            }
        }
    }

    private void OnGUI()
    {
        //��������ui
        for (int i = 0; i < _onGUI.Count; i++)
        {
            GUIStyle style = _onStyle[i]();
            _onGUI[i](style, null);
        }
    }


    /// <summary>
    /// ������ʽ���ƺ���
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    private void SetStyle(object ui, List<object> styles)
    {
        GUIStyle guiStyle = null;

        Func<GUIStyle> setStyle = () =>
        {
            guiStyle = GenerateStyle(ui);

            //����GUIStyle
            foreach (var style in styles)
            {
                switch (style)
                {
                    case ESSize:
                        ESSize size = (ESSize)style;
                        ESPercent percent = size.GetSizeType();
                        Vector2 vec2Size = size.GetSize();

                        switch (percent)
                        {
                            case ESPercent.All:
                                vec2Size = position.size * vec2Size / 100;
                                break;
                            case ESPercent.Width:
                                vec2Size.x = position.width * vec2Size.x / 100;
                                break;
                            case ESPercent.Height:
                                vec2Size.y = position.height * vec2Size.y / 100;
                                break;
                        }

                        guiStyle.fixedWidth = vec2Size.x;
                        guiStyle.fixedHeight = vec2Size.y;

                        break;
                }
            }

            return guiStyle;
        };

        _onStyle.Add(setStyle);
    }

    /// <summary>
    /// �����ɵ�UI����ί����ӵ�ί���б�
    /// </summary>
    /// <param name="action"></param>
    private void GenerateUI(Action<GUIStyle, GUILayoutOption[]> action)
    {
        _onGUI.Add(action);
    }

    /// <summary>
    /// ����GUIStyle����һ���µ�GUIStyle
    /// </summary>
    /// <param name="styleDef"></param>
    /// <returns></returns>
    private GUIStyle GenerateStyle(object ui)
    {
        switch (ui)
        {
            case ELabel:
                return new GUIStyle(GUI.skin.label);
            case EInput:
                return new GUIStyle(GUI.skin.textField);
            case EButton:
                return new GUIStyle(GUI.skin.button);
        }
        return new GUIStyle();
    }
}
