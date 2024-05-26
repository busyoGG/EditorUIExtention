using UnityEditor;
using UnityEngine;

namespace EditorUIExtension.CutsomEditor
{
    [E_Name("VisualElement编辑器")]
    public class VETest: BaseEditorVE<VETest>
    {
        [MenuItem("Test/VisualElement")]
        public static void ShowWindow()
        {
            GetWindow<VETest>().Show();
        }
        
        [E_Editor(EType.Label)]
        private string _label = "测试Label";

        [E_Editor(EType.Input),E_Wrap]
        [ES_BgColor(0,1,0,1),ES_Border(3),ES_BorderColor(1,0,0,1),ES_FontColor(0,0,0,1)]
        [StyleField(StyleFieldType.Inner)]
        private string _input;

        [E_Editor(EType.Button)]
        [ES_BgColor(0,1,0,1),ES_Border(3),ES_BorderColor(1,0,0,1),ES_FontColor(0,0,0,1)]
        private void Log()
        {
            Debug.Log("Input ===> " + _input);
        }

        [VE_Box(true)]
        [ES_BgColor(0.5f,0.5f,0.5f,1),ES_Radius(10),ES_Border(4),ES_BorderColor(0,0,0,1),ES_FontColor(0,0,0,1)]
        private string _group = "group1";

        [E_Editor(EType.Enum),E_Name("枚举")]
        [ES_BgColor(0,1,0,1),ES_Border(3),ES_BorderColor(1,0,0,1),ES_FontColor(0,0,0,1)]
        [VE_Box("group1")]
        [StyleField(StyleFieldType.Inner)]
        private EType _type;

        [E_Editor(EType.Radio),E_Options("选项1","选项2","选项3")]
        [ES_BgColor(0,1,0,1),ES_Border(3),ES_BorderColor(1,0,0,1),ES_FontColor(0,0,0,1)]
        private int _radio;

        [E_Editor(EType.Toggle),E_Name("开关")]
        [ES_BgColor(0,1,0,1),ES_Border(3),ES_BorderColor(1,0,0,1),ES_FontColor(0,0,0,1)]
        private bool _toggle;

        [E_Editor(EType.Slider),E_Range(0,10),E_Name("浮点进度条"),E_Wrap]
        [ES_BgColor(0,1,0,1),ES_Border(3),ES_BorderColor(1,0,0,1),ES_FontColor(0,0,0,1)]
        private float _slider;
        
        [E_Editor(EType.Slider),E_Range(0,10),E_Name("整数进度条"),E_DataType(DataType.Int)]
        private float _sliderInt;

        [E_Editor(EType.Object),E_DataType(DataType.Texture),ES_Size(70,70)]
        [ES_BgColor(0,1,0,1),ES_Border(3),ES_BorderColor(1,0,0,1),ES_FontColor(0,0,0,1)]
        [StyleField(StyleFieldType.Inner)]
        private Texture _texture;
    }
}