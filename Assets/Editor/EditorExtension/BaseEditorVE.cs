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

        /// <summary>
        /// 保存所有元素
        /// </summary>
        private Dictionary<string, VisualElement> _elements = new Dictionary<string, VisualElement>();

        private Action onPickerClosed;

        /// <summary>
        /// 鼠标左键是否按下
        /// </summary>
        private bool _isLeftMouseDown;

        /// <summary>
        /// 拖拽元素
        /// </summary>
        private VisualElement _dragElement;

        /// <summary>
        /// 拖拽默认坐标
        /// </summary>
        private Vector2 _defPos;

        /// <summary>
        /// 默认上左坐标
        /// </summary>
        private Vector2 _defTL;

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

        /// <summary>
        /// 获取元素
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected VisualElement GetElement(string id)
        {
            VisualElement element;
            _elements.TryGetValue(id, out element);
            return element;
        }

        /// <summary>
        /// 添加列表元素
        /// </summary>
        /// <param name="id"></param>
        protected void ListAdd(string id)
        {
            VisualElement element = GetElement(id);
            VisualElement list = element.Children().ElementAt(1);

            FieldInfo field = _type.GetField(id, _flag);

            E_Editor editor = field.GetCustomAttribute<E_Editor>();

            IList data = field.GetValue(this) as IList;
            data.Add(null);

            Action<Object> setData = o => { data[data.Count - 1] = o; };

            VisualElement listItem = GenerateListItem(field, editor.GetEType(), null, setData);

            RegisterDrag(listItem, list,data);

            list.Add(listItem);
        }

        /// <summary>
        /// 移除最后一个数组元素
        /// </summary>
        /// <param name="id"></param>
        protected void ListRemove(string id)
        {
            VisualElement element = GetElement(id);
            VisualElement list = element.Children().ElementAt(1);

            if (list.childCount == 0)
            {
                Debug.LogWarning("没有元素了");
                return;
            }

            list.RemoveAt(list.childCount - 1);

            FieldInfo field = _type.GetField(id, _flag);
            IList data = field.GetValue(this) as IList;
            data.RemoveAt(data.Count - 1);
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

                    bool isField = member.MemberType == MemberTypes.Field;

                    VisualElement box = new VisualElement();
                    VEStyleUtils.SetMargin(box.style, 5, 0, 0, 0);

                    rootView.Add(box);

                    _elements.Add(member.Name, box);

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

                    //辅助标签
                    Label subLabel;

                    //是否换行
                    E_Wrap eWrap = member.GetCustomAttribute<E_Wrap>();
                    bool isWrap = eWrap != null;

                    //数据类型
                    E_DataType dataType;

                    //子元素
                    VisualElement child;

                    bool isList = (member as FieldInfo) != null &&
                                  (member as FieldInfo).FieldType.Name.IndexOf("List") != -1;

                    // ----- 辅助对象 ----- end

                    if (isList)
                    {
                        subLabel = GenerateSubLabel(subName);

                        box.Add(subLabel);

                        VisualElement list = new VisualElement();
                        list.style.flexDirection = FlexDirection.Row;
                        list.style.flexWrap = Wrap.Wrap;

                        box.Add(list);

                        for (int i = 0; i < (value as IList).Count; i++)
                        {
                            Object data = (value as IList)[i] as Object;
                            int index = i;

                            Action<Object> setData = (Object res) => { (value as IList)[index] = res; };

                            VisualElement listItem = GenerateListItem(member, eType, data, setData);
                            list.Add(listItem);
                            RegisterDrag(listItem, list,value as IList);
                        }
                    }
                    else
                    {
                        switch (eType)
                        {
                            case EType.Label:
                                Label label = new Label();
                                label.style.fontSize = _fontSize;
                                label.text = value.ToString();

                                InitInnerStyle(member, label);

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
                                // text.style.height = _fontSize * 1.2f;

                                var fontChild = text.Children().FirstOrDefault().Children().FirstOrDefault();
                                fontChild.style.fontSize = _fontSize;

                                text.RegisterValueChangedCallback(evt => { setValue(this, evt.newValue); });
                                box.Add(text);

                                InitInnerStyle(member, text.Children().FirstOrDefault());

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

                                child = enumField.Children().FirstOrDefault();
                                if (InitInnerStyle(member, child))
                                {
                                    child.pickingMode = PickingMode.Position;
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

                                    Label lbRadio = GenerateSubLabel(selection);
                                    lbRadio.style.marginLeft = 5;
                                    radio.Add(lbRadio);

                                    radioButtonGroup.Add(radio);

                                    InitInnerStyle(member, lbRadio);
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

                                InitInnerStyle(member, subLabel);
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

                                TextField num = new TextField();
                                var numChild = num.Children().FirstOrDefault().Children().FirstOrDefault();
                                numChild.style.fontSize = _fontSize;
                                num.style.width = _fontSize * 3;
                                num.style.unityTextAlign = TextAnchor.MiddleRight;

                                if (isInt)
                                {
                                    SliderInt slider = new SliderInt((int)eRange.GetStart(), (int)eRange.GetEnd());

                                    num.value = eRange.GetStart().ToString();

                                    slider.value = 0;
                                    slider.style.flexGrow = 1;

                                    slider.RegisterValueChangedCallback(evt =>
                                    {
                                        setValue(this, evt.newValue);
                                        num.value = evt.newValue.ToString();
                                    });

                                    num.RegisterValueChangedCallback(evt =>
                                    {
                                        slider.value = int.Parse(evt.newValue);
                                    });

                                    sliderBox.Add(slider);
                                }
                                else
                                {
                                    Slider slider = new Slider(eRange.GetStart(), eRange.GetEnd());

                                    num.value = eRange.GetStart().ToString();

                                    slider.value = 0;
                                    slider.style.flexGrow = 1;

                                    slider.RegisterValueChangedCallback(evt =>
                                    {
                                        setValue(this, evt.newValue);
                                        num.value = evt.newValue.ToString();
                                    });

                                    num.RegisterValueChangedCallback(evt =>
                                    {
                                        slider.value = float.Parse(evt.newValue);
                                    });

                                    sliderBox.Add(slider);
                                }

                                InitInnerStyle(member, num.Children().FirstOrDefault());

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

                                if (InitInnerStyle(member, obj))
                                {
                                    box.style.height = StyleKeyword.Auto;
                                }

                                break;
                        }
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

                    VisualElement subBox;
                    E_Name eName = member.GetCustomAttribute<E_Name>();

                    if (veBox.IsFold())
                    {
                        subBox = new Foldout();
                        (subBox as Foldout).text = eName.GetName();
                    }
                    else
                    {
                        if (eName != null)
                        {
                            Label desc = new Label();
                            desc.text = eName.GetName();
                            desc.style.fontSize = _fontSize * 0.8f;
                            desc.style.marginTop = 3;
                            desc.style.alignSelf = Align.Center;
                            newBox.Add(desc);
                        }

                        subBox = new VisualElement();
                    }

                    newBox.Add(subBox);

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

                    _boxes.Add(newBoxName, subBox);
                }
            }
        }

        private void OnGUI()
        {
            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                //执行完后清除所有监听
                onPickerClosed = null;
            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                onPickerClosed?.Invoke();
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
            // label.style.height = _fontSize * 1.25f;
            label.text = subName;
            return label;
        }

        /// <summary>
        /// 生成列表元素
        /// </summary>
        /// <param name="member"></param>
        /// <param name="eType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private VisualElement GenerateListItem(MemberInfo member, EType eType, object data, Action<Object> setData)
        {
            switch (eType)
            {
                case EType.Object:
                    VisualElement obj = new VisualElement();
                    obj.name = "Texture";
                    VEStyleUtils.SetMargin(obj.style, 5);

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
                        obj.style.height = size.GetHeight() + _fontSize * 1.25f;
                        // obj.style.width = obj.style.width;
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

                    lbNone.name = "None";


                    obj.style.overflow = Overflow.Hidden;
                    obj.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1);

                    VEStyleUtils.SetBorderColor(obj.style, Color.black);
                    VEStyleUtils.SetBorder(obj.style, 2);
                    VEStyleUtils.SetRadius(obj.style, 5);

                    obj.Add(lbNone);

                    obj.RegisterCallback<MouseOverEvent>(evt =>
                    {
                        VEStyleUtils.SetBorderColor(obj.style, new Color(0.40f, 0.40f, 0.40f));
                    });

                    obj.RegisterCallback<MouseOutEvent>(evt =>
                    {
                        VEStyleUtils.SetBorderColor(obj.style, Color.black);
                    });

                    if (!InitInnerStyle(member, obj))
                    {
                    }

                    E_DataType dataType = member.GetCustomAttribute<E_DataType>();

                    //初始化
                    if (data != null)
                    {
                        GUIContent content =
                            EditorGUIUtility.ObjectContent(data as Object, data.GetType());
                        img.image = content.image;
                        pickedObjName.text = content.text;
                        img.style.display = DisplayStyle.Flex;
                    }

                    //有贴图则移除 None 提示，否则显示 None 提示
                    if (data == null)
                    {
                        // obj.Add(lbNone);
                        lbNone.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        lbNone.style.display = DisplayStyle.None;
                    }

                    Action callback = () =>
                    {
                        Object res = GetPickedObject<Object>();

                        if (res != null)
                        {
                            GUIContent content = EditorGUIUtility.ObjectContent(res, res.GetType());
                            img.image = content.image;
                            pickedObjName.text = content.text;
                            img.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            img.style.display = DisplayStyle.None;
                        }

                        //选择贴图后移除 None 提示，否则显示 None 提示
                        if (res == null)
                        {
                            // obj.Add(lbNone);
                            lbNone.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            lbNone.style.display = DisplayStyle.None;
                        }

                        // setValue(this, res);
                        // data = res;
                        setData(res);
                    };

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

                    return obj;
            }

            return null;
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
                ele.RegisterCallback<MouseDownEvent>(evt => { onMouseDown?.Invoke(); });

                ele.RegisterCallback<MouseUpEvent>(evt => { onMouseUp?.Invoke(); });

                ele.RegisterCallback<MouseOverEvent>(evt => { onMouseHover?.Invoke(); });

                ele.RegisterCallback<MouseOutEvent>(evt => { onMouseOut?.Invoke(); });

                if (inputListener != null)
                {
                    inputListener.RegisterCallback<FocusInEvent>(evt => { onFocus?.Invoke(); });

                    inputListener.RegisterCallback<FocusOutEvent>(evt => { onNotFocus?.Invoke(); });
                }
                else
                {
                    ele.RegisterCallback<FocusInEvent>(evt => { onFocus?.Invoke(); });

                    ele.RegisterCallback<FocusOutEvent>(evt => { onNotFocus?.Invoke(); });
                }
            }

            return isSet;
        }

        /// <summary>
        /// 注册拖拽
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="parent"></param>
        private void RegisterDrag(VisualElement ele, VisualElement parent,IList data)
        {
            VisualElement empty = new VisualElement();

            int defIndex = 0;

            bool isCapture = false;

            ele.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0) // 左键
                {
                    _dragElement = ele;

                    _defPos = evt.position;
                    _defTL = new Vector2(_dragElement.resolvedStyle.left - _dragElement.resolvedStyle.marginLeft,
                        _dragElement.resolvedStyle.top - _dragElement.resolvedStyle.marginTop);

                }
            });

            ele.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!isCapture && _dragElement != null)
                {
                    ele.CapturePointer(evt.pointerId);
                    isCapture = true;
                    foreach (var child in parent.Children())
                    {
                        child.style.position = Position.Absolute;
                        // Debug.Log(child.resolvedStyle.left);
                        child.style.left = child.resolvedStyle.left - child.resolvedStyle.marginLeft;
                        child.style.top = child.resolvedStyle.top - child.resolvedStyle.marginTop;
                    }

                    //占位
                    defIndex = parent.IndexOf(_dragElement);
                    parent.Insert(defIndex, empty);
                    _dragElement.BringToFront();
                }

                if (_dragElement != null && ele.HasPointerCapture(evt.pointerId))
                {
                    Vector2 delta = new Vector2(evt.position.x, evt.position.y) - _defPos;
                    _dragElement.style.top = _defTL.y + delta.y;
                    _dragElement.style.left = _defTL.x + delta.x;
                }
            });

            ele.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (_dragElement != null)
                {
                    foreach (var child in parent.Children())
                    {
                        if (child != _dragElement && child.worldBound.Contains(evt.position))
                        {
                            //UI 交换
                            int insertIndex = parent.IndexOf(child);
                            parent.Remove(_dragElement);
                            parent.Insert(insertIndex, _dragElement);
                            parent.Insert(defIndex,child);
                            
                            //数据交换
                            (data[defIndex], data[insertIndex]) = (data[insertIndex], data[defIndex]);

                            break;
                        }
                    }

                    foreach (var child in parent.Children())
                    {
                        child.style.position = Position.Relative;
                        child.style.left = 0;
                        child.style.top = 0;
                    }

                    if (parent.IndexOf(empty) != -1)
                    {
                        parent.Remove(empty);
                    }

                    _dragElement.ReleasePointer(evt.pointerId);
                    _dragElement = null;
                    isCapture = false;
                }
            });
        }
    }
}