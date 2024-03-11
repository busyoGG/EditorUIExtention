using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class CreateUIToolkitElement : EditorWindow
{
    [MenuItem("Window/Create UI Toolkit Element")]
    public static void ShowWindow()
    {
        GetWindow<CreateUIToolkitElement>("Create UI Toolkit Element");
    }

    //private void OnEnable()
    //{
    //    // 创建一个新的按钮元素
    //    Button button = new Button();
    //    button.text = "Click Me";

    //    // 添加按钮的点击事件处理程序
    //    button.clicked += () => Debug.Log("Button Clicked!");

    //    // 将按钮添加到窗口的根VisualElement中
    //    rootVisualElement.Add(button);
    //}

    private void OnGUI()
    {
        if (GUILayout.Button("新按钮"))
        {
            Debug.Log("点击");
        }
    }
}