using UnityEngine;
using UnityEditor;

[EName("≤‚ ‘¥∞ø⁄")]
public class Test : BaseEditor<Test>
{
    [MenuItem("Test/test")]
    public static void ShowWindow()
    {
        GetWindow<Test>().Show();
    }
    [ELabel]
    public string label = "≤‚ ‘Label";

    [EButton("≤‚ ‘∞¥≈•")]
    public void ShowHello()
    {
        Debug.Log("µ„ª˜≤‚ ‘∞¥≈•");
    }


    [ELabel]
    public string label2 = "≤‚ ‘Label2";
}
