using Codice.Client.BaseCommands.BranchExplorer.Layout;
using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class BaseEditor<T> : EditorWindow where T : EditorWindow
{
    private bool _isInited = false;

    private Type _type;
    private List<Action<GUIStyle, GUILayoutOption[]>> _onGUI = new List<Action<GUIStyle, GUILayoutOption[]>>();

    private List<Func<GUIStyle>> _onStyle = new List<Func<GUIStyle>>();

    private List<Func<GUILayoutOption[]>> _onOption = new List<Func<GUILayoutOption[]>>();

    //private List<>

    private BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;


    public void OnEnable()
    {
        if (_isInited) return;

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

            object layout = null;

            //遍历所有特性，分装到样式和ui和布局
            foreach (var prop in propAttrs)
            {
                Type attrType = prop.GetType();
                if (attrType.Name.IndexOf("ES_") != -1)
                {
                    styles.Add(prop);
                }
                else if (attrType.Name.IndexOf("E_") != -1)
                {
                    ui = prop;
                }
                else
                {
                    layout = prop;
                }
            }

            //绘制前置layout
            SetLayout(layout, true);

            //绘制UI
            SetUI(member, ui, styles);

            //绘制后置layout
            SetLayout(layout, false);
        }
    }

    private void OnGUI()
    {
        //if (!_isDirty)
        //{
        //    _isDirty = true;
        //    int i = 0;
        //    foreach (var data in GUI.skin.customStyles)
        //    {
        //        Debug.Log(i + " -- " + data.name);
        //        i++;
        //    }
        //}
        //遍历绘制ui
        for (int i = 0; i < _onGUI.Count; i++)
        {
            GUIStyle style = _onStyle[i]();
            GUILayoutOption[] option = _onOption[i]();
            _onGUI[i](style, option);
        }
    }


    /// <summary>
    /// 设置样式绘制函数
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    private void SetStyle(object ui, List<object> styles = null)
    {
        if (ui == null) return;

        GUIStyle guiStyle = null;

        Func<GUIStyle> setStyle = () =>
        {
            guiStyle = GenerateStyle(ui);
            //guiStyle.normal.textColor = Color.black;
            //guiStyle.margin = new RectOffset(0,0,0,0);

            if (styles != null)
            {
                //创建GUIStyle
                foreach (var style in styles)
                {
                    //switch (style)
                    //{

                    //}
                }
            }

            return guiStyle;
        };

        _onStyle.Add(setStyle);
    }

    private void SetOption(object ui, List<object> styles = null)
    {
        if (ui == null) return;
        Func<GUILayoutOption[]> action = () =>
        {
            List<GUILayoutOption> options = new List<GUILayoutOption>();
            if (styles != null)
            {
                //创建GUIStyle
                foreach (var style in styles)
                {
                    switch (style)
                    {
                        case ES_Size:
                            ES_Size size = (ES_Size)style;
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

                            options.Add(GUILayout.Width(vec2Size.x));
                            options.Add(GUILayout.Height(vec2Size.y));

                            break;
                    }
                }
            }
            return options.ToArray();
        };

        _onOption.Add(action);
    }

    /// <summary>
    /// 设置布局
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="front"></param>
    private void SetLayout(object ui, bool front)
    {
        if (ui == null) return;
        switch (ui)
        {
            case EL_Horizontal:
                EL_Horizontal horizontal = (EL_Horizontal)ui;
                if (horizontal.IsStart() == front)
                {
                    SetStyle(ui);
                    SetOption(ui);
                    GenerateUI(LayoutGenerator.GenerateHorizontal(front));
                }
                break;
            case EL_Vertical:
                EL_Vertical vertical = (EL_Vertical)ui;
                if (vertical.IsStart() == front)
                {
                    SetStyle(ui);
                    SetOption(ui);
                    GenerateUI(LayoutGenerator.GenerateVertical(front));
                }
                break;
        }
    }

    /// <summary>
    /// 设置UI
    /// </summary>
    /// <param name="member"></param>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    private void SetUI(MemberInfo member, object ui, List<object> styles)
    {
        //设置样式绘制函数
        SetStyle(ui, styles);
        SetOption(ui, styles);

        //创建ui
        if (member.MemberType == MemberTypes.Field)
        {
            FieldInfo fieldInfo = (FieldInfo)member;
            //字段
            switch (ui)
            {
                case E_Label:
                    //获取字段的值
                    Func<string> getValueDelegate;
                    if (((E_Label)ui).IsCanChange())
                    {
                        //可变
                        getValueDelegate = () =>
                        {
                            return (string)fieldInfo.GetValue(this);
                        };
                    }
                    else
                    {
                        //不可变
                        string name = (string)fieldInfo.GetValue(this);
                        getValueDelegate = () =>
                        {
                            return name;
                        };
                    }

                    GenerateUI(UIGenerator.GenerateLabel(getValueDelegate));
                    break;
                case E_Input:
                    E_Input input = (E_Input)ui;
                    GenerateUI(UIGenerator.GenerateInput(fieldInfo.Name, (string)fieldInfo.GetValue(this),
                        this, input.GetWidth(), input.IsPercent(), input.IsDoubleLine()));
                    break;
                case E_Texture:
                    GenerateUI(UIGenerator.GenerateObject<Texture>(fieldInfo.Name, (Texture)fieldInfo.GetValue(this)));
                    break;
            }
        }
        else if (member.MemberType == MemberTypes.Method)
        {
            //方法
            switch (ui)
            {
                case E_Button:
                    E_Button attr = (E_Button)ui;
                    GenerateUI(UIGenerator.GenerateButton(attr.GetName(), (MethodInfo)member, this));
                    break;
            }

        }
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
        GUIStyle layoutDef;
        switch (ui)
        {
            case E_Label:
                layoutDef = new GUIStyle(GUI.skin.label);
                return layoutDef;
            case E_Input:
                layoutDef = new GUIStyle(GUI.skin.textField);
                layoutDef.wordWrap = true;
                return layoutDef;
            case E_Button:
                layoutDef = new GUIStyle(GUI.skin.button);
                return layoutDef;
            case EL_Horizontal:
            case EL_Vertical:
                layoutDef = new GUIStyle(GUI.skin.box);
                return layoutDef;
            case E_Texture:
                layoutDef = new GUIStyle();
                return layoutDef;
        }
        return new GUIStyle();
    }

}
