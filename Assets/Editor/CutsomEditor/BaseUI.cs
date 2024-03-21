using UnityEditor;
using UnityEngine;

[EName("����UI")]
public class BaseUI : BaseEditor<BaseUI>
{
    [MenuItem("Test/BaseUI")]
    public static void ShowWindow()
    {
        GetWindow<BaseUI>().Show();
    }

    [E_Label]
    public string label = "Label��ʽ";

    [E_Input]
    public string defInput;

    [E_Input(50, false)]
    public string strInput;

    [E_Texture]
    public Texture texture;

    [E_Button("��ť")]
    public void Button()
    {

    }
}
