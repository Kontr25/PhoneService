using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.StateSwitcher
{
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class RaycastSwitch : StateItem
    {
        [SerializeField][HideLabel][HorizontalGroup("h1", 20f)]
        private bool _toggle;

        [HorizontalGroup("h1")][HideLabel][SerializeField]
        private Graphic _object;

        public override void Set()
        {
            _object.raycastTarget = _toggle;
        }

        public override void DefaultState()
        {
        }

#if UNITY_EDITOR
        public override StateItem Clone()
        {
            return new RaycastSwitch
            {
                _toggle = _toggle,
                _object = _object
            };
        }

        public override string DropDownItemName => "Raycast";
        public override string DropDownGroupName => "Raycast Switch";
#endif
    }
}