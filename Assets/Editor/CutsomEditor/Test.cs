using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[EName("���Դ���")]
public class Test : BaseEditor<Test>
{
    [MenuItem("Test/Test1")]
    public static void ShowWindow()
    {
        GetWindow<Test>().Show();
    }

    [E_Label, EL_Horizontal(true)]
    public string label = "����Label";

    [E_Input(50, false), ES_Size(20, 40, ESPercent.Width)]
    public string strInput;

    [E_Button("���԰�ť"), ES_Size(20, 40, ESPercent.Width), EL_Horizontal(false)]
    public void ShowHello()
    {
        //Debug.Log("������԰�ť");
        //label = "�޸�label";
        tex.Add(null);
        Refresh();
    }

    [E_Label, EL_Horizontal(true)]
    public string label2 = "����Label2";

    [E_Texture, ES_Size(70, 70), EL_Horizontal(false)]
    public Texture texture;

    [E_Texture, ES_Size(70, 70), EL_Foldout(true, "�����۵�"), EL_List(true, EL_ListType.Flex, true, true, 100, 200, ESPercent.Width)]
    public List<Texture> tex = new List<Texture>() { null, null, null, null, null, null };

    [E_Label, ES_Size(70, 70), EL_List(true, EL_ListType.Flex, true), EL_Foldout(false)]
    public List<string> labels = new List<string>() { "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa" };

    [E_Label, ES_Size(70, 70), EL_List(true, EL_ListType.Vertical, false, true, 100, 200, ESPercent.Width)]
    public string labelL1 = "�����б�1";
    [E_Label, ES_Size(70, 100)]
    public string labelL2 = "�����б�2";
    [E_Label, ES_Size(70, 70), EL_List(false, EL_ListType.Vertical, false)]
    public string labelL3 = "�����б�3";

    [E_Button("ˢ�½���")]
    private void Refresh()
    {
        RefreshUIInit();
    }
}
