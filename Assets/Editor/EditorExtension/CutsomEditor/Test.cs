using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using EditorUIExtension;

[E_Name("测试窗口")]
public class Test : BaseEditor<Test>
{
    [MenuItem("Test/Test1")]
    public static void ShowWindow()
    {
        GetWindow<Test>().Show();
    }

    [E_Editor(EType.Label), EL_Horizontal(true)]
    public string label = "测试Label";

    [E_Editor(EType.Input), ES_Size(20, 40, ESPercent.Width),E_Name("测试Input")]
    public string strInput;

    [E_Editor(EType.Button), ES_Size(20, 40, ESPercent.Width), EL_Horizontal(false)]
    public void ShowHello()
    {
        //Debug.Log("点击测试按钮");
        //label = "修改label";
        // tex.Add(null);
        Debug.Log("是否选择框 ==> " + _isToggle);
        Debug.Log("滑动条值 ==> " + _testSlider);
        Debug.Log("输入框 ==> " + strInput);
        // Refresh();
    }

    [E_Editor(EType.Label), EL_Horizontal(true)]
    public string label2 = "测试Label2";

    [E_Editor(EType.Texture), ES_Size(70, 70), EL_Horizontal(false)]
    public Texture texture;

    [E_Editor(EType.Texture), ES_Size(70, 70), EL_Foldout(true, "测试折叠"), EL_List(true, EL_ListType.Flex, true, true, 100, 200, ESPercent.Width)]
    public List<Texture> tex = new List<Texture>() { null, null, null, null, null, null };

    [E_Editor(EType.Label), ES_Size(70, 70), EL_List(true, EL_ListType.Flex, true), EL_Foldout(false)]
    public List<string> labels = new List<string>() { "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa" };

    [E_Editor(EType.Label), ES_Size(70, 70), EL_List(true, EL_ListType.Vertical, false, true, 100, 200, ESPercent.Width)]
    public string labelL1 = "测凉列表1";
    [E_Editor(EType.Label), ES_Size(70, 100)]
    public string labelL2 = "测试列表2";
    [E_Editor(EType.Label), ES_Size(70, 70), EL_List(false, EL_ListType.Vertical, false)]
    public string labelL3 = "测试列表3";

    [E_Editor(EType.Button)]
    private void Refresh()
    {
        RefreshUIInit();
    }

    [E_Editor(EType.Enum),E_Name("测试Enum"),E_Width(20,WidthType.Percent),E_Wrap(false)]
    public EType _testType = EType.Enum;

    [E_Editor(EType.Slider),E_Name("测试Slider"),E_Range(-10,10),E_DataType(DataType.Int)]
    public float _testSlider = 0;

    [E_Editor(EType.Radio),E_Options("选项1","选项2","选项3")]
    public int _radioSelection = 0;

    [E_Editor(EType.Toggle),E_Name("是否选择框")]
    public bool _isToggle = false;
}
