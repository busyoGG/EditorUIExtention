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
    //    // ����һ���µİ�ťԪ��
    //    Button button = new Button();
    //    button.text = "Click Me";

    //    // ��Ӱ�ť�ĵ���¼��������
    //    button.clicked += () => Debug.Log("Button Clicked!");

    //    // ����ť��ӵ����ڵĸ�VisualElement��
    //    rootVisualElement.Add(button);
    //}

    private void OnGUI()
    {
        if (GUILayout.Button("�°�ť"))
        {
            Debug.Log("���");
        }
    }
}