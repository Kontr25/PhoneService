using System;
using UI.Attributes;

#if UNITY_EDITOR
#endif

namespace UI.StateSwitcher
{
    /// <summary>
    /// Класс элемента состояния.
    /// Можно при наследовании расширять функционал для добавления управляемых элементов
    /// </summary>
    [Serializable]
#if UNITY_EDITOR
    [FoldoutClass("$DropDownItemName")]
#endif
    public abstract class StateItem
    {
        public abstract void Set();

        public abstract void DefaultState();

#if UNITY_EDITOR
        public abstract string DropDownItemName { get; }
        public abstract string DropDownGroupName { get; }
        public abstract StateItem Clone();
#endif

    }
}