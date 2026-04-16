using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.StateSwitcher
{
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class ButtonsUnlockSwitch : StateItem
    {
        [SerializeField] private List<Button> _buttons;

        public override void Set()
        {
            foreach (var button in _buttons)
            {
                button.interactable = false;
            }
        }

        public override void DefaultState()
        {
            foreach (var button in _buttons)
            {
                button.interactable = true;
            }
        }

#if UNITY_EDITOR

        public override StateItem Clone()
        {
            return new ButtonsUnlockSwitch
            {
                _buttons = _buttons,
            };
        }

        public override string DropDownItemName => "Switch Unlock";
        public override string DropDownGroupName => "Buttons";
#endif
    }
}