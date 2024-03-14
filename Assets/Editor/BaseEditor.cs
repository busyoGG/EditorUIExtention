using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BaseEditor<T> : EditorWindow where T : EditorWindow
{
    private bool _isInited = false;

    private LayoutNode _root;

    private Type _type;

    private float _totalWidth = 0;
    private float curWidth = 0;

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

    private void RefreshUIInit()
    {
        _root = null;
        Init();
    }

    private void Init()
    {
        _root = new LayoutNode();
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

        LayoutNode node = _root;
        //遍历对象，创建生成函数
        foreach (var item in members)
        {
            MemberInfo member = item.Member;
            var propAttrs = member.GetCustomAttributes(true);

            List<object> styles = new List<object>();
            object ui = null;

            //object layout = null;
            List<object> layouts = new List<object>();

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
                    //layout = prop;
                    layouts.Add(prop);
                }
            }

            int back = 0;
            if (layouts.Count > 0)
            {
                foreach (var layout in layouts)
                {
                    dynamic layoutData = layout;

                    if (layoutData.IsStart())
                    {
                        LayoutNode next = new LayoutNode();

                        next.SetLayout(SetLayout(layout, next, member), (SetStyle(layout), SetOption(layout)));

                        node.AddUi(next);
                        node = next;

                        if (layoutData is EL_List)
                        {
                            if (layoutData.IsSingle())
                            {
                                back++;
                            }
                        }
                    }
                    else
                    {
                        //node = node.parent;
                        back++;
                    }
                }
            }


            //定义Name和GetVal
            Func<object> GetVal;

            bool isField = false;

            if (member.MemberType == MemberTypes.Field)
            {
                isField = true;
            }

            FieldInfo field = member as FieldInfo;

            //判断数据类型是否List
            bool isList = isField && field.FieldType.Name.IndexOf("List") != -1;

            if (isList)
            {
                dynamic list = field.GetValue(this);

                for (int i = 0; i < list.Count; i++)
                {
                    int index = i;
                    if (isField)
                    {
                        GetVal = () =>
                        {
                            return list[index];
                        };
                        node.AddUi(SetUI(member, ui, styles, field.Name + "_" + i, GetVal));
                    }
                    else
                    {
                        node.AddUi(SetUI(member, ui, styles, "", null));
                    }
                }
            }
            else
            {
                if (isField)
                {
                    GetVal = () =>
                    {
                        object res = field.GetValue(this);
                        if (res != null)
                        {
                            return res;
                        }
                        return null;
                    };
                    node.AddUi(SetUI(member, ui, styles, field.Name, GetVal));
                }
                else
                {
                    node.AddUi(SetUI(member, ui, styles, "", null));
                }
            }


            for (int i = 0; i < back; i++)
            {
                node = node.parent;
            }
        }
    }

    private void OnGUI()
    {
        Render(_root);
    }

    private void Render(LayoutNode node)
    {
        if (node._layout != null)
        {
            node._layout();
        }
        else
        {
            NormalRender(node)();
        }

    }


    /// <summary>
    /// 设置样式绘制函数
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    private Func<GUIStyle> SetStyle(object ui, List<object> styles = null)
    {
        if (ui == null) return null;

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

        return setStyle;
    }

    private Func<GUILayoutOption[]> SetOption(object ui, List<object> styles = null)
    {
        if (ui == null) return null;
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

        return action;
    }

    /// <summary>
    /// 设置布局
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="front"></param>
    private Action<GUIStyle, GUILayoutOption[]> SetLayout(object layout, LayoutNode node, MemberInfo member)
    {
        if (layout == null) return null;
        switch (layout)
        {
            case EL_Horizontal:
                return LayoutGenerator.GenerateHorizontal(NormalRender(node));
            case EL_Vertical:
                return LayoutGenerator.GenerateVertical(NormalRender(node));
            case EL_List:
                return LayoutGenerator.GenerateList(FlexRender(node, member), (EL_List)layout, this);
            case EL_Foldout:
                return LayoutGenerator.GenerateFoldout(NormalRender(node), (EL_Foldout)layout);
        }
        return null;
    }

    /// <summary>
    /// 设置UI
    /// </summary>
    /// <param name="member"></param>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    private UIListData SetUI(MemberInfo member, object ui, List<object> styles, string name, Func<object> getVal)
    {
        UIListData list = new UIListData();
        //创建ui
        if (member.MemberType == MemberTypes.Field)
        {
            //字段
            Action<GUIStyle, GUILayoutOption[]> action = null;

            object val = getVal();

            switch (ui)
            {
                case E_Label:
                    Func<string> labelName = () =>
                    {
                        return (string)getVal();
                    };
                    action = UIGenerator.GenerateLabel(labelName);
                    break;
                case E_Input:
                    E_Input input = (E_Input)ui;
                    action = UIGenerator.GenerateInput(name, (string)val,
                        this, input.GetWidth(), input.IsPercent(), input.IsDoubleLine());
                    break;
                case E_Texture:
                    action = UIGenerator.GenerateObject(name, (Texture)val);
                    break;
            }

            list.style = SetStyle(ui, styles);
            list.options = SetOption(ui, styles);
            list.action = action;
        }
        else if (member.MemberType == MemberTypes.Method)
        {
            //方法
            Action<GUIStyle, GUILayoutOption[]> action = null;
            switch (ui)
            {
                case E_Button:
                    E_Button attr = (E_Button)ui;
                    action = UIGenerator.GenerateButton(attr.GetName(), (MethodInfo)member, this);
                    break;
            }

            list.style = SetStyle(ui, styles);
            list.options = SetOption(ui, styles);
            list.action = action;
        }

        return list;
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
        }
        return new GUIStyle();
    }

    /// <summary>
    /// 普通渲染
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private Action NormalRender(LayoutNode node)
    {
        Action action = () =>
        {
            foreach (var ui in node._list)
            {
                if (ui is LayoutNode)
                {
                    Render((LayoutNode)ui);
                }
                else
                {
                    UIListData data = (UIListData)ui;
                    data.action(data.style(), data.options());
                }
            }
        };
        return action;
    }

    /// <summary>
    /// 流式渲染
    /// </summary>
    /// <param name="node"></param>
    /// <param name="member"></param>
    /// <returns></returns>
    private Action FlexRender(LayoutNode node, MemberInfo member)
    {
        Action action = () =>
        {
            int i = 0;
            foreach (var ui in node._list)
            {
                int index = i;

                float space = 6;
                //由于难以正确获取列表对象大小，因此请手动传入
                ES_Size size = member.GetAttribute<ES_Size>();
                ESPercent percent = size.GetSizeType();
                Vector2 vec2Size = size.GetSize();
                vec2Size = LayoutGenerator.CalSize(percent, vec2Size, this);

                if (index == 0)
                {
                    curWidth = space;
                }

                curWidth += vec2Size.x + space;

                _totalWidth = position.width;

                if (index < node._list.Count && curWidth > _totalWidth)
                {
                    curWidth = vec2Size.x + space * 2;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box));
                }

                if (ui is LayoutNode)
                {
                    Render((LayoutNode)ui);
                }
                else
                {
                    UIListData data = (UIListData)ui;
                    data.action(data.style(), data.options());
                }
                i++;
            }
        };
        return action;
    }
}
