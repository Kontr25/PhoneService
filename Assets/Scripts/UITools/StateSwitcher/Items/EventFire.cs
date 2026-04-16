using UnityEngine;
using UnityEngine.Events;

namespace UI.StateSwitcher
{
    /// <summary>
    /// Запуск функции при включении стейта
    /// </summary>
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class EventFire : StateItem
    {
        [SerializeField] private UnityEvent events;
        public override void Set() => events.Invoke();

        public override void DefaultState()
        {
        }

#if UNITY_EDITOR
        public override StateItem Clone()
        {
            var clone = new EventFire
            {
                events = events,
            };
            return clone;
        }
        
        public override string DropDownGroupName => "Events";
        
        public override string DropDownItemName => "FireEvent";
#endif
    }
}