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

        //筛选包含编辑器拓展类型基类的对象
        var members = type.GetMembers(flag)
                            .Select(m => new
                            {
                                Member = m,
                                Attribute = m.GetCustomAttributes(typeof(EBase), false).FirstOrDefault() as EBase
                            })
                            .Where(x => x.Attribute != null)
                            .OrderBy(x => x.Attribute._lineNum);

        //遍历对象，创建生成函数
        foreach (var item in members)
        {
            MemberInfo member = item.Member;
            var propAttrs = member.GetCustomAttributes(true);

            List<object> styles = new List<object>();
            object ui = null;

            //遍历所有特性，分装到样式和ui
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

            //设置样式绘制函数
            SetStyle(ui, styles);

            //创建ui
            if (member.MemberType == MemberTypes.Field)
            {
                //字段
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
                //方法
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
        //遍历绘制ui
        for (int i = 0; i < _onGUI.Count; i++)
        {
            GUIStyle style = _onStyle[i]();
            _onGUI[i](style, null);
        }
    }


    /// <summary>
    /// 设置样式绘制函数
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    private void SetStyle(object ui, List<object> styles)
    {
        GUIStyle guiStyle = null;

        Func<GUIStyle> setStyle = () =>
        {
            guiStyle = GenerateStyle(ui);

            //创建GUIStyle
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
    /// 把生成的UI生成委托添加到委托列表
    /// </summary>
    /// <param name="action"></param>
    private void GenerateUI(Action<GUIStyle, GUILayoutOption[]> action)
    {
        _onGUI.Add(action);
    }

    /// <summary>
    /// 根据GUIStyle创建一个新的GUIStyle
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
