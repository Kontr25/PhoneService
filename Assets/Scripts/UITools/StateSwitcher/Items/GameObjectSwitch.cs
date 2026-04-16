using System.Collections.Generic;
using UnityEngine;

namespace UI.StateSwitcher
{
    /// <summary>
    /// Режим подключения/отключения слоев
    /// </summary>
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class GameObjectSwitch : StateItem
    {
        [SerializeField] private GameObject[] links = { };

        public override void Set()
        {
            foreach (var gameObject in links)
                gameObject.SetActive(true);
        }

        public override void DefaultState()
        {
            foreach (var gameObject in links)
                gameObject.SetActive(false);
        }


#if UNITY_EDITOR
        public override StateItem Clone()
        {
            var temp = new List<GameObject>();
            temp.AddRange(links);
            var clone = new GameObjectSwitch
            {
                links = temp.ToArray(),
            };
            return clone;
        }

        public override string DropDownGroupName => "Objects";
        public override string DropDownItemName => "GameObjects Switch";

#endif
    }
}