using System;
using UnityEngine.UIElements;

namespace EditorUIExtension
{
    public class ES_List: Attribute
    {
        private FlexDirection _direction;

        private Wrap _Wrap;

        public ES_List(FlexDirection direction, Wrap wrap = Wrap.Wrap)
        {
            _direction = direction;
            _Wrap = wrap;
        }

        public FlexDirection GetDirection()
        {
            return _direction;
        }

        public Wrap GetWrap()
        {
            return _Wrap;
        }
    }
}