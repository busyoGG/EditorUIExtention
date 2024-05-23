using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorUIExtension;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace EditorUIExtension
{
    public delegate void SetFieldDelegate(object target, object value);

    public delegate object GetFieldDelegate(object target);

    public class BaseEditor<T> : EditorWindow where T : EditorWindow
    {
        private bool _isInited = false;

        private LayoutNode _root;

        private Type _type;

        private float _totalWidth;
        private float _curWidth;

        private Vector2 _scrollPosition = Vector2.zero;

        private readonly BindingFlags _flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                              BindingFlags.Instance | BindingFlags.DeclaredOnly;


        public void OnEnable()
        {
            if (_isInited) return;

            //获得类型
            _type = GetType();

            //初始化窗口名称
            E_Name ename = _type.GetCustomAttribute<E_Name>();
            InitWindow(ename.GetName());

            //初始化UI
            Init();
        }

        private void InitWindow(string windowName)
        {
            T win = GetWindow<T>();
            win.titleContent = new GUIContent(windowName);
        }

        protected void RefreshUIInit()
        {
            _root = null;
            Init();
        }

        private void Init()
        {
            _root = new LayoutNode();
            Type type = GetType();

            //筛选包含编辑器拓展类型基类的对象
            var members = type.GetMembers(_flag)
                .Select(m => new
                {
                    Member = m,
                    Attribute = m.GetCustomAttributes(typeof(EBase), false).FirstOrDefault() as EBase
                })
                .Where(x => x.Attribute != null)
                .OrderBy(x => x.Attribute.lineNum);

            LayoutNode node = _root;
            //遍历对象，创建生成函数
            foreach (var item in members)
            {
                MemberInfo member = item.Member;
                var propAttrs = member.GetCustomAttributes(true);

                List<object> styles = new List<object>();
                E_Editor ui = null;

                //object layout = null;
                List<object> layouts = new List<object>();

                //遍历所有特性，分装到样式和ui和布局
                foreach (var prop in propAttrs)
                {
                    Type attrType = prop.GetType();
                    string attrName = attrType.Name;

                    //判断是否 Editor UI 的额外控制参数 
                    if (attrName.IndexOf("E_") != -1 && prop is not E_Editor) continue;

                    if (attrType.Name.IndexOf("ES_") != -1)
                    {
                        styles.Add(prop);
                    }
                    else if (prop is E_Editor editor)
                    {
                        ui = editor;
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
                GetFieldDelegate getVal;
                SetFieldDelegate setVal;

                bool isField = member.MemberType == MemberTypes.Field;

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
                            getVal = _ => list[index];
                            setVal = (_, res) => SetListValue(list, index, res);
                            node.AddUi(SetUI(member, ui, styles, field.Name + "_" + i, getVal, setVal));
                        }
                        else
                        {
                            node.AddUi(SetUI(member, ui, styles, "", null, null));
                        }
                    }
                }
                else
                {
                    if (isField)
                    {
                        // getVal = () =>
                        // {
                        //     object res = field.GetValue(this);
                        //     if (res != null)
                        //     {
                        //         return res;
                        //     }
                        //
                        //     return null;
                        // };

                        Type fieldType = field.GetType();
                        getVal = (GetFieldDelegate)Delegate.CreateDelegate(typeof(GetFieldDelegate), field, "GetValue",
                            false);
                        setVal = (SetFieldDelegate)Delegate.CreateDelegate(typeof(SetFieldDelegate), field, "SetValue",
                            false);

                        node.AddUi(SetUI(member, ui, styles, field.Name, getVal, setVal));
                    }
                    else
                    {
                        node.AddUi(SetUI(member, ui, styles, "", null, null));
                    }
                }


                for (int i = 0; i < back; i++)
                {
                    node = node.Parent;
                }
            }
        }

        public static void SetListValue<TValue>(List<TValue> list, int index, object value)
        {
            // 转换 object 类型的值为 T 类型
            TValue convertedValue = (TValue)value;

            // 设置列表中的值
            list[index] = convertedValue;
        }

        private void OnGUI()
        {
            //GUIStyle panel = new GUIStyle();
            //panel.margin = new RectOffset(0, 0, 0, 0);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUIStyle.none);
            Render(_root);
            EditorGUILayout.EndScrollView();
        }

        private void Render(LayoutNode node)
        {
            if (node.layout != null)
            {
                node.layout();
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
        /// <returns></returns>
        private Func<GUIStyle> SetStyle(object ui, List<object> styles = null)
        {
            if (ui == null) return null;

            GUIStyle Style()
            {
                GUIStyle guiStyle = GenerateStyle(ui);

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
            }

            return Style;
        }

        /// <summary>
        /// 设置GUILayout
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="styles"></param>
        /// <returns></returns>
        private Func<GUILayoutOption[]> SetOption(object ui, List<object> styles = null)
        {
            if (ui == null) return null;

            GUILayoutOption[] Action()
            {
                List<GUILayoutOption> options = new List<GUILayoutOption>();
                if (styles != null)
                {
                    //创建GUILayoutOption
                    foreach (var style in styles)
                    {
                        switch (style)
                        {
                            case ES_Size size:
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
            }

            return Action;
        }

        /// <summary>
        /// 设置布局
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="node"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        private Action<GUIStyle, GUILayoutOption[]> SetLayout(object layout, LayoutNode node, MemberInfo member)
        {
            if (layout == null) return null;
            switch (layout)
            {
                case EL_Horizontal:
                    return LayoutGenerator.GenerateHorizontal(NormalRender(node));
                case EL_Vertical:
                    return LayoutGenerator.GenerateVertical(NormalRender(node));
                case EL_List elList:
                    Action render = elList.ListType() == EL_ListType.Flex
                        ? FlexRender(node, member)
                        : NormalRender(node);
                    return LayoutGenerator.GenerateList(render, elList, this);
                case EL_Foldout foldout:
                    return LayoutGenerator.GenerateFoldout(NormalRender(node), foldout);
            }

            return null;
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        /// <param name="member"></param>
        /// <param name="ui"></param>
        /// <param name="styles"></param>
        /// <param name="propName"></param>
        /// <param name="getVal"></param>
        /// <returns></returns>
        private UIListData SetUI(MemberInfo member, E_Editor ui, List<object> styles, string propName,
            GetFieldDelegate getVal, SetFieldDelegate setVal)
        {
            UIListData list = new UIListData();
            //创建ui
            //字段
            Action<GUIStyle, GUILayoutOption[]> action = null;

            object val = getVal?.Invoke(this);
            Action<object> change = res => { setVal(this, res); };

            // ----- 额外参数 ----- start

            E_Name eName = member.GetCustomAttribute<E_Name>();

            E_Width eWidth = member.GetCustomAttribute<E_Width>();
            float width = eWidth?.GetWidth() ?? 30;
            bool percent = eWidth == null || eWidth.GetWidthType() == WidthType.Percent;

            E_Wrap eWrap = member.GetCustomAttribute<E_Wrap>();
            bool isWrap = eWrap == null || eWrap.GetWrap();

            // ----- 额外参数 ----- end

            string subName = eName != null ? eName.GetName() : propName;

            switch (ui.GetEType())
            {
                case EType.Label:

                    string LabelName()
                    {
                        return (string)getVal(this);
                    }

                    action = UIGenerator.GenerateLabel(LabelName);
                    break;
                case EType.Input:
                    action = UIGenerator.GenerateInput(subName, (string)val, change, this, width, percent, isWrap);
                    break;
                case EType.Texture:
                    action = UIGenerator.GenerateObject(subName, (Texture)val, change);
                    break;
                case EType.Button:
                    action = UIGenerator.GenerateButton(subName, (MethodInfo)member, this);
                    break;
                case EType.Enum:
                    action = UIGenerator.GenerateEnum(subName, val as Enum, change, this, width, percent, isWrap);
                    break;
                case EType.Slider:

                    E_Range range = member.GetCustomAttribute<E_Range>();

                    E_DataType dataType = member.GetCustomAttribute<E_DataType>();

                    if (dataType.GetDataType() == DataType.Int)
                    {
                        val = (int)range.GetStart();
                        action = UIGenerator.GenerateSlider(subName, (int)range.GetStart(), (int)range.GetEnd(),
                            (int)val, change, this, width, percent, isWrap);
                    }
                    else
                    {
                        val = range.GetStart();
                        action = UIGenerator.GenerateSlider(subName, range.GetStart(), range.GetEnd(), (float)val,
                            change, this, width, percent, isWrap);
                    }

                    break;
                case EType.Radio:
                    E_Options options = member.GetCustomAttribute<E_Options>();

                    action = UIGenerator.GenerateRadio(options.GetOptions(), (int)val, change);

                    break;
                case EType.Toggle:
                    break;
            }

            list.style = SetStyle(ui, styles);
            list.options = SetOption(ui, styles);
            list.action = action;

            return list;
        }

        /// <summary>
        /// 根据GUIStyle创建一个新的GUIStyle
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        private GUIStyle GenerateStyle(object ui)
        {
            GUIStyle layoutDef;
            switch (ui)
            {
                case E_Editor editor:
                    switch (editor.GetEType())
                    {
                        case EType.Label:
                            layoutDef = new GUIStyle(EditorStyles.label);
                            return layoutDef;
                        case EType.Input:
                            layoutDef = new GUIStyle(EditorStyles.textField)
                            {
                                wordWrap = true
                            };
                            return layoutDef;
                        case EType.Button:
                            layoutDef = new GUIStyle(GUI.skin.button);
                            return layoutDef;
                        case EType.Enum:
                            layoutDef = new GUIStyle(EditorStyles.popup);
                            return layoutDef;
                    }

                    break;
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
            void Action()
            {
                foreach (var ui in node.list)
                {
                    if (ui is LayoutNode layoutNode)
                    {
                        Render(layoutNode);
                    }
                    else
                    {
                        UIListData data = (UIListData)ui;
                        data.action(data.style(), data.options());
                    }
                }
            }

            return Action;
        }

        /// <summary>
        /// 流式渲染
        /// </summary>
        /// <param name="node"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        private Action FlexRender(LayoutNode node, MemberInfo member)
        {
            void Action()
            {
                int i = 0;
                foreach (var ui in node.list)
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
                        _curWidth = space;
                    }

                    _curWidth += vec2Size.x + space;

                    _totalWidth = position.width;

                    if (index < node.list.Count && _curWidth > _totalWidth)
                    {
                        _curWidth = vec2Size.x + space * 2;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box));
                    }

                    if (ui is LayoutNode layoutNode)
                    {
                        Render(layoutNode);
                    }
                    else
                    {
                        UIListData data = (UIListData)ui;
                        data.action(data.style(), data.options());
                    }

                    i++;
                }
            }

            return Action;
        }
    }
}