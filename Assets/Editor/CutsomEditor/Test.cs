using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[EName("≤‚ ‘¥∞ø⁄")]
public class Test : BaseEditor<Test>
{
    [MenuItem("Test/Test1")]
    public static void ShowWindow()
    {
        GetWindow<Test>().Show();
        //GetWindow<Test>().Close();
    }

    [E_Label(true), EL_Horizontal(true)]
    public string label = "≤‚ ‘Label";

    [E_Input(50, false), ES_Size(40, 40, ESPercent.Width)]
    public string strInput;

    [E_Button("≤‚ ‘∞¥≈•"), ES_Size(50, 40, ESPercent.Width), EL_Horizontal(false)]
    public void ShowHello()
    {
        //Debug.Log("µ„ª˜≤‚ ‘∞¥≈•");
        label = "–ﬁ∏ƒlabel";
    }

    [E_Label, EL_Horizontal(true)]
    public string label2 = "≤‚ ‘Label2";

    [E_Texture, ES_Size(70, 70), EL_Horizontal(false)]
    public Texture texture;

    [E_Texture, ES_Size(70, 70), EL_List(EL_ListType.Flex, true, 100, 200, ESPercent.Width)]
    public List<Texture> tex = new List<Texture>() { null, null, null, null, null, null };

    [E_Label, ES_Size(70, 70), EL_List(EL_ListType.Flex)]
    public List<string> labels = new List<string>() { "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa" };
}
