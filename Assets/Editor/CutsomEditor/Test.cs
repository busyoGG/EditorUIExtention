using UnityEngine;
using UnityEditor;

[EName("���Դ���")]
public class Test : BaseEditor<Test>
{
    [MenuItem("Test/test")]
    public static void ShowWindow()
    {
        GetWindow<Test>().Show();
    }
    [ELabel]
    public string label = "����Label";

    [EButton("���԰�ť")]
    public void ShowHello()
    {
        Debug.Log("������԰�ť");
    }


    [ELabel]
    public string label2 = "����Label2";
}
