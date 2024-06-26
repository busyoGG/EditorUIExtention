using System.Collections;
using System.Collections.Generic;
using EditorUIExtension;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[E_Name("UI???")]
public class Style : BaseEditorIMGUI<Style>
{
    [MenuItem("Test/Style")]
    public static void ShowWindow()
    {
        GetWindow<Style>().Show();
    }

    [E_Editor(EType.Object), ES_Size(70, 70)]
    public Texture size1;

    [E_Editor(EType.Object), ES_Size(70, 10, ESPercent.Height)]
    public Texture size2;

    [E_Editor(EType.Object), ES_Size(50, 70, ESPercent.Width)]
    public Texture size3;

    [E_Editor(EType.Object), ES_Size(50, 10, ESPercent.All)]
    public Texture size4;
}
