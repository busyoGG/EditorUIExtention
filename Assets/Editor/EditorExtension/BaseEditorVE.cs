using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Attribute = Codice.Client.BaseCommands.Attribute;
using Object = UnityEngine.Object;

namespace EditorUIExtension
{
    public class BaseEditorVE<T> : EditorWindow where T : EditorWindow
    {
        private bool _isInited = false;
        private Type _type;

        private readonly BindingFlags _flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                              BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 保存所有创建的区域
        /// </summary>
        private Dictionary<string, VisualElement> _boxes = new Dictionary<string, VisualElement>();

        private Action onPickerClosed;

        /// <summary>
        /// 鼠标左键是否按下
        /// </summary>
        private bool _isLeftMouseDown;

        // ----- 默认属性 ----- start

        private readonly int _fontSize = 14;

        // ----- 默认属性 ----- end

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

        private void Init()
        {
            //获得类型
            _type = GetType();

            //创建一个基础滚动面板包裹整个编辑器
            ScrollView rootView = new ScrollView();
            rootView.style.flexGrow = 1;
            VEStyleUtils.SetPadding(rootView.style, 5);

            //监听鼠标
            rootView.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    _isLeftMouseDown = true;
                }
            });

            rootView.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    _isLeftMouseDown = false;
                }
            });

            rootVisualElement.Add(rootView);

            //筛选包含编辑器拓展类型基类的对象  
            var members = _type.GetMembers(_flag)
                .SelectMany(m => m.GetCustomAttributes<EBase>().Select(attr => new
                {
                    Member = m,
                    Attribute = attr
                }))
                .Where(x => x.Attribute != null)
                .OrderBy(x => x.Attribute.lineNum)
                .GroupBy(x => x.Member)
                .Select(g => new
                {
                    Member = g.Key,
                    Attributes = g.Select(x => x.Attribute).ToList() // 获取该成员的所有属性
                });

            foreach (var item in members)
            {
                MemberInfo member = item.Member;

                VE_Box veBox = member.GetCustomAttribute<VE_Box>();
                bool isCreate = veBox?.IsCreate() ?? false;
                string boxName = veBox?.GetName() ?? "";

                E_Editor editor = member.GetCustomAttribute<E_Editor>();

                if (editor != null)
                {
                    EType eType = editor.GetEType();

                    var attrs = member.GetCustomAttributes();

                    bool isField = member.MemberType == MemberTypes.Field;

                    VisualElement box = new VisualElement();
                    VEStyleUtils.SetMargin(box.style, 5, 0, 0, 0);

                    rootView.Add(box);

                    //判断 Box 相关逻辑
                    if (veBox != null)
                    {
                        if (!string.IsNullOrEmpty(boxName))
                        {
                            //添加元素到 Box
                            VisualElement createdBox;
                            _boxes.TryGetValue(veBox.GetName(), out createdBox);
                            if (createdBox != null)
                            {
                                createdBox.Add(box);
                            }
                            else
                            {
                                Debug.LogError(veBox.GetName() + " 不存在");
                            }
                        }
                    }

                    //排除列表
                    List<object> exclude = new List<object>()
                    {
                        EType.Button
                    };

                    StyleField sf = member.GetCustomAttribute<StyleField>();

                    if (sf != null && sf.GetField() == StyleFieldType.Inner)
                    {
                        exclude.Add(editor.GetEType());
                    }

                    //在排除列表外的 UI 对父元素初始化样式
                    if (exclude.IndexOf(eType) == -1)
                    {
                        //遍历 Attribute 设置容器样式
                        InitStyle(box, attrs);
                    }


                    // ----- 辅助对象 ----- start

                    object value = null;

                    SetFieldDelegate setValue = null;

                    if (isField)
                    {
                        FieldInfo field = member as FieldInfo;

                        value = field.GetValue(this);

                        setValue = (SetFieldDelegate)Delegate.CreateDelegate(typeof(SetFieldDelegate), field,
                            "SetValue",
                            false);
                    }

                    //辅助名称
                    E_Name eName = member.GetCustomAttribute<E_Name>();
                    string fieldName = member.Name.Replace("_", "");
                    string subName = eName != null
                        ? eName.GetName()
                        : fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);

                    Label subLabel;

                    E_Wrap eWrap = member.GetCustomAttribute<E_Wrap>();
                    bool isWrap = eWrap != null;

                    E_DataType dataType;

                    VisualElement child;

                    // ----- 辅助对象 ----- end

                    switch (eType)
                    {
                        case EType.Label:
                            Label label = new Label();
                            label.style.fontSize = _fontSize;
                            label.text = value.ToString();

                            box.Add(label);
                            break;
                        case EType.Input:
                            if (!isWrap)
                            {
                                box.style.flexDirection = FlexDirection.Row;
                            }

                            subLabel = GenerateSubLabel(subName);
                            box.Add(subLabel);

                            TextField text = new TextField();
                            text.style.flexGrow = 1;
                            text.style.height = _fontSize * 1.2f;

                            var fontChild = text.Children().FirstOrDefault().Children().FirstOrDefault();
                            fontChild.style.fontSize = _fontSize;

                            text.RegisterValueChangedCallback(evt => { setValue(this, evt.newValue); });
                            box.Add(text);

                            if (sf != null && sf.GetField() != StyleFieldType.Outer)
                            {
                                child = text.Children().FirstOrDefault();
                                InitInnerStyle(member, child);
                            }

                            break;
                        case EType.Button:
                            Button button = new Button();
                            button.text = subName;
                            Action clickAction =
                                (Action)Delegate.CreateDelegate(typeof(Action), this, member as MethodInfo);
                            button.clicked += clickAction;
                            box.Add(button);

                            if (InitInnerStyle(member, button))
                            {
                                //干掉激活按钮的触发器（鼠标左键）
                                button.clickable.activators.Clear();
                                button.RegisterCallback<ClickEvent>(evt => { clickAction(); });
                            }

                            break;
                        case EType.Enum:
                            if (!isWrap)
                            {
                                box.style.flexDirection = FlexDirection.Row;
                            }

                            subLabel = GenerateSubLabel(subName);
                            box.Add(subLabel);

                            EnumField enumField = new EnumField(value as Enum);
                            enumField.style.flexGrow = 1;

                            enumField.RegisterValueChangedCallback(evt => setValue(this, evt.newValue));

                            if (sf != null && sf.GetField() != StyleFieldType.Outer)
                            {
                                child = enumField.Children().FirstOrDefault();
                                if (InitInnerStyle(member, child))
                                {
                                    child.pickingMode = PickingMode.Position;
                                }
                            }

                            box.Add(enumField);
                            break;
                        case EType.Radio:
                            RadioButtonGroup radioButtonGroup = new RadioButtonGroup();
                            radioButtonGroup.value = 0;

                            box.Add(radioButtonGroup);

                            E_Options options = member.GetCustomAttribute<E_Options>();

                            foreach (var selection in options.GetOptions())
                            {
                                RadioButton radio = new RadioButton();
                                radio.Children().FirstOrDefault().style.flexGrow = 0;

                                Label lbRadio = new Label();
                                lbRadio.style.fontSize = _fontSize;
                                lbRadio.text = selection;
                                lbRadio.style.marginLeft = 5;
                                radio.Add(lbRadio);

                                radioButtonGroup.Add(radio);
                            }

                            radioButtonGroup.RegisterValueChangedCallback(evt => { setValue(this, evt.newValue); });
                            break;
                        case EType.Toggle:
                            Toggle toggle = new Toggle();
                            toggle.style.width = StyleKeyword.Auto;
                            toggle.Children().FirstOrDefault().style.flexGrow = 0;

                            toggle.RegisterValueChangedCallback(evt => { setValue(this, evt.newValue); });

                            box.Add(toggle);

                            subLabel = GenerateSubLabel(subName);
                            box.Add(subLabel);
                            subLabel.style.marginLeft = 5;
                            toggle.Add(subLabel);
                            break;
                        case EType.Slider:
                            if (!isWrap)
                            {
                                box.style.flexDirection = FlexDirection.Row;
                            }

                            subLabel = GenerateSubLabel(subName);
                            box.Add(subLabel);

                            //用于包裹进度条和进度条数值的 UI 元素
                            VisualElement sliderBox = new VisualElement();
                            sliderBox.style.flexDirection = FlexDirection.Row;
                            sliderBox.style.flexGrow = 1;

                            box.Add(sliderBox);

                            dataType = member.GetCustomAttribute<E_DataType>();
                            bool isInt = dataType != null && dataType.GetDataType() == DataType.Int;

                            E_Range eRange = member.GetCustomAttribute<E_Range>();

                            Label num = new Label();
                            num.style.fontSize = _fontSize;
                            num.style.width = _fontSize * 3;
                            num.style.unityTextAlign = TextAnchor.MiddleRight;

                            if (isInt)
                            {
                                SliderInt slider = new SliderInt((int)eRange.GetStart(), (int)eRange.GetEnd());

                                num.text = eRange.GetStart().ToString();

                                slider.value = 0;
                                slider.style.flexGrow = 1;

                                slider.RegisterValueChangedCallback(evt =>
                                {
                                    setValue(this, evt.newValue);
                                    num.text = evt.newValue.ToString();
                                });

                                sliderBox.Add(slider);
                            }
                            else
                            {
                                Slider slider = new Slider(eRange.GetStart(), eRange.GetEnd());

                                num.text = eRange.GetStart().ToString();

                                slider.value = 0;
                                slider.style.flexGrow = 1;

                                slider.RegisterValueChangedCallback(evt =>
                                {
                                    setValue(this, evt.newValue);
                                    num.text = evt.newValue.ToString();
                                });

                                sliderBox.Add(slider);
                            }

                            sliderBox.Add(num);

                            break;
                        case EType.Object:
                            // box.style.flexDirection = FlexDirection.Row;

                            subLabel = GenerateSubLabel(subName);
                            box.Add(subLabel);


                            VisualElement obj = new VisualElement();

                            ES_Size size = member.GetCustomAttribute<ES_Size>();

                            //没选中时的提示
                            Label lbNone;

                            //显示的贴图
                            Image img = new Image();
                            img.style.justifyContent = Justify.Center;
                            obj.Add(img);

                            Label pickedObjName = new Label();
                            img.Add(pickedObjName);

                            if (size != null)
                            {
                                //有 size 的情况下，size 改变的是包裹该 UI 的父容器大小，因此此处宽高填满父容器
                                box.style.height = box.style.height.value.value + _fontSize * 1.25f;
                                obj.style.flexGrow = 1;
                                img.style.flexGrow = 1;
                                lbNone = GenerateSubLabel("None\n(Texture)");
                                lbNone.style.position = Position.Absolute;
                                pickedObjName.style.marginLeft = size.GetWidth() + 5;
                            }
                            else
                            {
                                //没有 size 的情况下，设置默认宽高，让父容器自适应大小
                                obj.style.width = Length.Percent(100);
                                obj.style.height = _fontSize * 1.65f;
                                img.style.width = obj.style.height.value.value * 0.8f;
                                img.style.height = obj.style.height.value.value * 0.8f;
                                img.style.alignSelf = Align.FlexStart;
                                lbNone = GenerateSubLabel("None(Texture)");
                                pickedObjName.style.marginLeft = obj.style.height.value.value + 5;
                            }


                            obj.style.overflow = Overflow.Hidden;
                            obj.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1);

                            VEStyleUtils.SetBorderColor(obj.style, Color.black);
                            VEStyleUtils.SetBorder(obj.style, 2);
                            VEStyleUtils.SetRadius(obj.style, 5);

                            obj.Add(lbNone);

                            Action callback = () =>
                            {
                                Object res = GetPickedObject<Object>();

                                if (res != null)
                                {
                                    GUIContent content = EditorGUIUtility.ObjectContent(res, res.GetType());
                                    img.image = content.image;
                                    pickedObjName.text = content.text;
                                    img.visible = true;
                                }
                                else
                                {
                                    img.visible = false;
                                }

                                //选择贴图后移除 None 提示，否则显示 None 提示
                                if (obj.childCount <= 1)
                                {
                                    if (res == null)
                                    {
                                        obj.Add(lbNone);
                                    }
                                }
                                else
                                {
                                    if (res != null)
                                    {
                                        obj.Remove(lbNone);
                                    }
                                }

                                setValue(this, res);
                            };

                            dataType = member.GetCustomAttribute<E_DataType>();

                            //设置监听
                            obj.RegisterCallback<ClickEvent>(evt =>
                            {
                                if (dataType != null)
                                {
                                    switch (dataType.GetDataType())
                                    {
                                        case DataType.Texture:
                                            ShowPicker<Texture2D>();
                                            break;
                                        case DataType.GameObject:
                                            ShowPicker<GameObject>();
                                            break;
                                    }
                                }
                                else
                                {
                                    ShowPicker<Object>();
                                }

                                onPickerClosed += callback;
                            });

                            box.Add(obj);

                            if (sf != null && sf.GetField() != StyleFieldType.Outer)
                            {
                                if (InitInnerStyle(member, obj))
                                {
                                    box.style.height = StyleKeyword.Auto;
                                }
                            }

                            break;
                    }
                }
                else if (isCreate)
                {
                    var attrs = member.GetCustomAttributes();

                    string newBoxName = (member as FieldInfo).GetValue(this).ToString();

                    VisualElement newBox = new VisualElement();
                    VEStyleUtils.SetMargin(newBox.style, 5, 0, 0, 0);
                    newBox.name = newBoxName;
                    InitStyle(newBox, attrs);

                    if (!string.IsNullOrEmpty(veBox.GetName()))
                    {
                        //添加元素到 Box
                        VisualElement createdBox;
                        _boxes.TryGetValue(veBox.GetName(), out createdBox);
                        if (createdBox != null)
                        {
                            createdBox.Add(newBox);
                        }
                        else
                        {
                            Debug.LogError(veBox.GetName() + " 不存在");
                        }
                    }
                    else
                    {
                        rootView.Add(newBox);
                    }

                    _boxes.Add(newBoxName, newBox);
                }
            }
        }

        private void OnGUI()
        {
            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                onPickerClosed?.Invoke();
                //执行完后清除所有监听
                onPickerClosed = null;
            }
        }

        /// <summary>
        /// 创建辅助标签
        /// </summary>
        /// <param name="subName"></param>
        /// <returns></returns>
        private Label GenerateSubLabel(string subName)
        {
            Label label = new Label();
            label.style.fontSize = _fontSize;
            label.style.height = _fontSize * 1.25f;
            label.text = subName;
            return label;
        }

        /// <summary>
        /// 显示选择器
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        private void ShowPicker<TValue>() where TValue : Object
        {
            EditorGUIUtility.ShowObjectPicker<TValue>(null, false, "", 0);
        }

        /// <summary>
        /// 获取选择器上一次选择的对象
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        private Object GetPickedObject<TValue>()
        {
            Object res = EditorGUIUtility.GetObjectPickerObject();
            return res;
        }

        /// <summary>
        /// 初始化样式
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="attrs"></param>
        private void InitStyle(VisualElement ele, dynamic attrs)
        {
            //遍历 Attribute 设置容器样式
            foreach (var attr in attrs)
            {
                switch (attr)
                {
                    case ES_Size size:
                        switch (size.GetSizeType())
                        {
                            case ESPercent.All:
                                ele.style.width = Length.Percent(size.GetWidth());
                                ele.style.height = Length.Percent(size.GetHeight());
                                break;
                            case ESPercent.Height:
                                ele.style.width = size.GetWidth();
                                ele.style.height = Length.Percent(size.GetHeight());
                                break;
                            case ESPercent.Width:
                                ele.style.width = Length.Percent(size.GetWidth());
                                ele.style.height = size.GetHeight();
                                break;
                            default:
                                ele.style.width = size.GetWidth();
                                ele.style.height = size.GetHeight();
                                break;
                        }

                        break;
                    case ES_BgColor bgColor:
                        ele.style.backgroundColor = bgColor.GetColor();
                        break;
                    case ES_Radius radius:
                        if (radius.IsSame())
                        {
                            VEStyleUtils.SetRadius(ele.style, radius.GetRadius());
                        }
                        else
                        {
                            IntVec4 all = radius.GetAllRadius();
                            VEStyleUtils.SetRadius(ele.style, all.x, all.y, all.z, all.w);
                        }

                        break;
                    case ES_Border border:
                        if (border.IsSame())
                        {
                            VEStyleUtils.SetBorder(ele.style, border.GetWidth());
                        }
                        else
                        {
                            IntVec4 all = border.GetAllBorder();
                            VEStyleUtils.SetBorder(ele.style, all.x, all.y, all.z, all.w);
                        }

                        break;
                    case ES_BorderColor borderColor:
                        VEStyleUtils.SetBorderColor(ele.style, borderColor.GetColor());
                        break;
                    case ES_Font:
                        break;
                    case ES_FontColor fontColor:
                        VEStyleUtils.SetFontColor(ele.style, fontColor.GetColor());
                        break;
                }
            }
        }

        /// <summary>
        /// 设置内联样式
        /// </summary>
        /// <param name="member"></param>
        /// <param name="style"></param>
        private bool InitInnerStyle(MemberInfo member, VisualElement ele)
        {
            bool isSet = true;

            int count = 0;

            VisualElement inputListener = null;

            bool isFocus = false;

            List<object> attrs = new List<object>()
            {
                member.GetCustomAttribute<ES_BgColor>(),
                member.GetCustomAttribute<ES_FontColor>(),
                member.GetCustomAttribute<ES_Border>(),
                member.GetCustomAttribute<ES_BorderColor>(),
                member.GetCustomAttribute<ES_Radius>(),
                member.GetCustomAttribute<ES_Size>()
            };

            Action onMouseDown = null;
            Action onMouseUp = null;
            Action onMouseHover = null;
            Action onMouseOut = null;
            Action onFocus = null;
            Action onNotFocus = null;

            foreach (var attr in attrs)
            {
                if (attr == null)
                {
                    count++;
                    continue;
                }

                switch (attr)
                {
                    case ES_BgColor bgColor:
                        ele.style.backgroundColor = bgColor.GetColor();
                        onMouseDown += () =>
                        {
                            Color buttonColor = bgColor.GetColor() * 0.8f;
                            buttonColor.a = 1;
                            ele.style.backgroundColor = buttonColor;
                        };

                        onMouseUp += () => { ele.style.backgroundColor = bgColor.GetColor(); };

                        onMouseHover += () =>
                        {
                            if (_isLeftMouseDown)
                            {
                                Color buttonColor = bgColor.GetColor() * 0.8f;
                                buttonColor.a = 1;
                                ele.style.backgroundColor = buttonColor;
                            }
                            else
                            {
                                Color buttonColor = bgColor.GetColor() + new Color(0.5f, 0.5f, 0.5f);
                                ele.style.backgroundColor = buttonColor;
                            }
                        };

                        onMouseOut += () => { ele.style.backgroundColor = bgColor.GetColor(); };

                        break;
                    case ES_Border border:
                        if (border.IsSame())
                        {
                            VEStyleUtils.SetBorder(ele.style, border.GetWidth());
                        }
                        else
                        {
                            IntVec4 all = border.GetAllBorder();
                            VEStyleUtils.SetBorder(ele.style, all.x, all.y, all.z, all.w);
                        }

                        break;
                    case ES_BorderColor borderColor:
                        VEStyleUtils.SetBorderColor(ele.style, borderColor.GetColor());

                        onFocus += () =>
                        {
                            VEStyleUtils.SetBorderColor(ele.style, new Color(0.23f, 0.47f, 0.73f));
                            isFocus = true;
                        };

                        onNotFocus += () => { VEStyleUtils.SetBorderColor(ele.style, borderColor.GetColor()); };

                        inputListener = ele.Children().FirstOrDefault() as TextElement;

                        if (inputListener != null)
                        {
                            onMouseDown += () =>
                            {
                                VEStyleUtils.SetBorderColor(ele.style, new Color(0.23f, 0.47f, 0.73f));
                            };

                            onMouseUp += () =>
                            {
                                if (!isFocus)
                                {
                                    VEStyleUtils.SetBorderColor(ele.style, borderColor.GetColor());
                                }
                            };

                            onMouseHover += () =>
                            {
                                if (isFocus)
                                {
                                    VEStyleUtils.SetBorderColor(ele.style, new Color(0.23f, 0.47f, 0.73f));
                                }
                                else
                                {
                                    VEStyleUtils.SetBorderColor(ele.style, new Color(0.40f, 0.40f, 0.40f));
                                }
                            };

                            onMouseOut += () =>
                            {
                                if (isFocus)
                                {
                                    VEStyleUtils.SetBorderColor(ele.style, new Color(0.23f, 0.47f, 0.73f));
                                }
                                else
                                {
                                    VEStyleUtils.SetBorderColor(ele.style, borderColor.GetColor());
                                }
                            };
                        }
                        else
                        {
                            onMouseDown += () =>
                            {
                                VEStyleUtils.SetBorderColor(ele.style, new Color(0.23f, 0.47f, 0.73f));
                            };

                            onMouseUp += () => { VEStyleUtils.SetBorderColor(ele.style, borderColor.GetColor()); };

                            onMouseHover += () =>
                            {
                                if (_isLeftMouseDown)
                                {
                                    VEStyleUtils.SetBorderColor(ele.style, new Color(0.23f, 0.47f, 0.73f));
                                }
                                else
                                {
                                    VEStyleUtils.SetBorderColor(ele.style, new Color(0.40f, 0.40f, 0.40f));
                                }
                            };

                            onMouseOut += () => { VEStyleUtils.SetBorderColor(ele.style, borderColor.GetColor()); };
                        }

                        break;
                    case ES_FontColor fontColor:
                        var eleChildren = ele.Children();

                        ele.style.color = fontColor.GetColor();

                        void SetChild(IEnumerable<VisualElement> children)
                        {
                            foreach (var child in children)
                            {
                                SetChild(child.Children());
                                child.style.color = fontColor.GetColor();
                            }
                        }

                        SetChild(eleChildren);

                        break;
                    case ES_Size size:
                        switch (size.GetSizeType())
                        {
                            case ESPercent.All:
                                ele.style.width = Length.Percent(size.GetWidth());
                                ele.style.height = Length.Percent(size.GetHeight());
                                break;
                            case ESPercent.Height:
                                ele.style.width = size.GetWidth();
                                ele.style.height = Length.Percent(size.GetHeight());
                                break;
                            case ESPercent.Width:
                                ele.style.width = Length.Percent(size.GetWidth());
                                ele.style.height = size.GetHeight();
                                break;
                            default:
                                ele.style.width = size.GetWidth();
                                ele.style.height = size.GetHeight();
                                break;
                        }

                        break;
                }
            }

            if (count == attrs.Count)
            {
                isSet = false;
            }

            if (isSet)
            {
                ele.RegisterCallback<MouseDownEvent>(evt => { onMouseDown(); });

                ele.RegisterCallback<MouseUpEvent>(evt => { onMouseUp(); });

                ele.RegisterCallback<MouseOverEvent>(evt => { onMouseHover(); });

                ele.RegisterCallback<MouseOutEvent>(evt => { onMouseOut(); });

                if (inputListener != null)
                {
                    inputListener.RegisterCallback<FocusInEvent>(evt => { onFocus(); });

                    inputListener.RegisterCallback<FocusOutEvent>(evt => { onNotFocus(); });
                }
                else
                {
                    ele.RegisterCallback<FocusInEvent>(evt => { onFocus(); });

                    ele.RegisterCallback<FocusOutEvent>(evt => { onNotFocus(); });
                }
            }

            return isSet;
        }
    }
}