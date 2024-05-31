namespace EditorUIExtension.CutsomEditor
{
    public class VETestClass
    {
        [E_Editor(EType.Label)]
        private string _label = "测试Label";

        [E_Editor(EType.Input)]
        public string input;
    }
}