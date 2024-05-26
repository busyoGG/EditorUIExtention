using EditorUIExtension;
using UnityEditor;
using UnityEngine;

[E_Name("����UI")]
public class BaseUI : BaseEditorIMGUI<BaseUI>
{
    [MenuItem("Test/BaseUI")]
    public static void ShowWindow()
    {
        GetWindow<BaseUI>().Show();
    }

    [E_Editor(EType.Label)]
    public string label = "Label��ʽ";

    [E_Editor(EType.Input)]
    public string defInput;

    [E_Editor(EType.Input)]
    public string strInput;

    [E_Editor(EType.Object)]
    public Texture texture;

    [E_Editor(EType.Button)]
    public void Button()
    {

    }
}
