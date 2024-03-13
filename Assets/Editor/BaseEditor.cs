using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BaseEditor<T> : EditorWindow where T : EditorWindow
{
    private bool _isInited = false;

    private Type _type;
    private List<Action<GUIStyle, GUILayoutOption[]>> _onGUI = new List<Action<GUIStyle, GUILayoutOption[]>>();

    private List<Func<GUIStyle>> _onStyle = new List<Func<GUIStyle>>();

    private List<Func<GUILayoutOption[]>> _onOption = new List<Func<GUILayoutOption[]>>();

    //Dictionary<int, Action> _flex = new Dictionary<int, Action>();

    private float _totalWidth = 0;
    private float curWidth = 0;
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
            SetUI(member, layout, ui, styles);

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
            //Action flex;
            //_flex.TryGetValue(i,out flex);
            //if(flex != null)
            //{
            //    flex();
            //}
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

                            vec2Size = LayoutGenerator.CalSize(percent, vec2Size, this);

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
                    _onGUI.Add(LayoutGenerator.GenerateHorizontal(front));
                }
                break;
            case EL_Vertical:
                EL_Vertical vertical = (EL_Vertical)ui;
                if (vertical.IsStart() == front)
                {
                    SetStyle(ui);
                    SetOption(ui);
                    _onGUI.Add(LayoutGenerator.GenerateVertical(front));
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
    private void SetUI(MemberInfo member, object layout, object ui, List<object> styles)
    {

        //创建ui
        if (member.MemberType == MemberTypes.Field)
        {
            FieldInfo fieldInfo = (FieldInfo)member;

            Func<int, object> GetVal;
            object val;
            string name;

            int loop = 1;
            bool isList = false;
            bool isFlex = false;

            EL_List elList = null;

            if (layout is EL_List)
            {
                elList = (EL_List)layout;
                loop = ((dynamic)fieldInfo.GetValue(this)).Count;

                GetVal = (int i) =>
                {
                    object res = fieldInfo.GetValue(this);
                    if (res != null)
                    {
                        return ((dynamic)res)[i];
                    }
                    return null;
                };
                isList = true;
                isFlex = ((EL_List)layout).ListType() == EL_ListType.Flex;
            }
            else
            {
                GetVal = (int i) =>
                {
                    return fieldInfo.GetValue(this);
                };
            }

            Action doUI = () =>
            {
                for (int i = 0; i < loop; i++)
                {
                    //Rect currentRect = new Rect(0, 0, 0f, 0f);
                    Action<GUIStyle, GUILayoutOption[]> action = null;

                    val = GetVal(i);
                    name = fieldInfo.Name;
                    if (isList)
                    {
                        name += "_" + i;
                    }

                    //流式布局特殊判断，是否换行
                    if (isFlex)
                    {
                        int index = i;
                        action = (GUIStyle style, GUILayoutOption[] options) =>
                        {
                            //由于难以正确获取列表对象大小，因此请手动传入
                            ES_Size size = member.GetAttribute<ES_Size>();
                            ESPercent percent = size.GetSizeType();
                            Vector2 vec2Size = size.GetSize();
                            vec2Size = LayoutGenerator.CalSize(percent, vec2Size, this);

                            if (index == 0)
                            {
                                Debug.Log("重置");
                                curWidth = 6;
                            }

                            curWidth += vec2Size.x + 6;

                            //_totalWidth = LayoutGenerator.CalSize(elList.GetPercent(), new Vector2(elList.Width(), elList.Height()), this).x;
                            //if (_totalWidth == 0)
                            //{
                            //    _totalWidth = position.width * 0.75f;
                            //}
                            _totalWidth = position.width;
                            Debug.Log("总宽度" + _totalWidth);

                            if (index < loop && curWidth > _totalWidth)
                            {
                                // End the current horizontal group and begin a new one for the next row
                                curWidth = vec2Size.x + 6;
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box));
                                Debug.Log("另起一行");
                            }
                            Debug.Log("换行" + index + ":" + curWidth + " ---- " + _totalWidth);
                        };

                        //_flex.Add(_onGUI.Count - 1, flex);
                    }

                    //设置样式绘制函数
                    SetStyle(ui, styles);
                    SetOption(ui, styles);
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
                                    return (string)GetVal(i);
                                };
                            }
                            else
                            {
                                //不可变
                                getValueDelegate = () =>
                                {
                                    return (string)val;
                                };
                            }

                            action += UIGenerator.GenerateLabel(getValueDelegate);
                            break;
                        case E_Input:
                            E_Input input = (E_Input)ui;
                            action += UIGenerator.GenerateInput(name, (string)val,
                                this, input.GetWidth(), input.IsPercent(), input.IsDoubleLine());
                            break;
                        case E_Texture:
                            action += UIGenerator.GenerateObject(name, (Texture)val);
                            break;
                    }

                    //_onGUI.Add(action);
                    
                    _onGUI.Add(action);
                }
            };

            if (isList)
            {
                SetStyle(layout);
                SetOption(layout);
                Debug.Log("开始流式布局");
                _onGUI.Add(LayoutGenerator.GenerateList(true, elList, this));

                doUI();

                SetStyle(layout);
                SetOption(layout);
                _onGUI.Add(LayoutGenerator.GenerateList(false, elList, this));
            }
            else
            {
                doUI();
            }
        }
        else if (member.MemberType == MemberTypes.Method)
        {
            Action<GUIStyle, GUILayoutOption[]> action = null;
            //设置样式绘制函数
            SetStyle(ui, styles);
            SetOption(ui, styles);
            //方法
            switch (ui)
            {
                case E_Button:
                    E_Button attr = (E_Button)ui;
                    action = UIGenerator.GenerateButton(attr.GetName(), (MethodInfo)member, this);
                    break;
            }
            _onGUI.Add(action);
        }
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
            case EL_List:
                layoutDef = new GUIStyle(GUI.skin.box);
                return layoutDef;
            case E_Texture:
                layoutDef = new GUIStyle();
                return layoutDef;
        }
        return new GUIStyle();
    }
}
