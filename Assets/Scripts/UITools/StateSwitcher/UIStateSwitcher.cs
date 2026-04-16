using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using System.Reflection;
#endif
using Sirenix.OdinInspector;
using UI.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.StateSwitcher
{
    /// <summary>
    /// Контроллер переключения состояний UI 
    /// </summary>
    [HideMonoScript]
    public class UIStateSwitcher : MonoBehaviour
    {
        [OnInspectorGUI("OnInspectorGUI")]

        #region inner Classes

#if UNITY_EDITOR
        [FoldoutClass("$_name")]
#endif
        [Serializable, HideReferenceObjectPicker]
        public class StateData
        {
            [FormerlySerializedAs("name")]
            [HorizontalGroup("top", 0.8f)]
            [SerializeField, HideLabel]
            [LabelText("Состояние")]
            private string _name;

            [SerializeReference] [LabelText("Элементы")] [HideReferenceObjectPicker]
            //[ValueDropdown("$GetItems", AppendNextDrawer = true)]
            private StateItem[] _stateItems = { };


            public void Set()
            {
                if (_stateItems == null) return;
                foreach (var stateItem in _stateItems)
                {
                    stateItem.Set();
                }
            }

            public void DefaultState()
            {
                if (_stateItems == null) return;
                foreach (var stateItem in _stateItems)
                {
                    stateItem.DefaultState();
                }
            }

            public string Name => _name;
            public StateItem[] StateItems => _stateItems;
#if UNITY_EDITOR


            internal void AddStateItem(StateItem stateItem)
            {
                var list = _stateItems.ToList();
                list.Add(stateItem);
                _stateItems = list.ToArray();
            }

            private ValueDropdownList<StateItem> GetItems()
            {
                var list = new ValueDropdownList<StateItem>();
                foreach (var type in
                         GetDerivedTypes<StateItem>())
                {
                    var instance = (StateItem)Activator.CreateInstance(type);
                    list.Add($"{instance.DropDownGroupName}/{instance.DropDownItemName}",
                        (StateItem)Activator.CreateInstance(type));
                }

                return list;
            }

            public static IEnumerable<Type> GetDerivedTypes<T>() where T : StateItem
            {
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                Type baseType = typeof(T);

                return callingAssembly.GetTypes()
                    .Where(type => baseType.IsAssignableFrom(type) && type != baseType);
            }

            internal UIStateSwitcher owner;

            [HorizontalGroup("top", 0.2f), GUIColor("@Color.green")]
            [Button]
            private void SetSwitcher() => owner.Set(this);
#endif
        }

        #endregion

        #region Params

        [ToggleGroup("_setOnEnableState", "Enable State")]
        [LabelText("Состояние на OnEnable")]
        [SerializeField]
        [LabelWidth(180f)]
        private bool _setOnEnableState;

        [ToggleGroup("_setOnEnableState", "Enable State")]
        [HideLabel]
        [SerializeField]
        [ValueDropdown("$GetDropDownStatesList")]
        private int _stateOnEnable;

#if UNITY_EDITOR
        [LabelText("Дублировать")] [SerializeField]
        private bool _duplicateOnCreate = true;
#endif
        [FormerlySerializedAs("states")]
        [LabelText("Состояния")]
        [ListDrawerSettings(OnBeginListElementGUI = "OnBeginListElementGUI",
            OnEndListElementGUI = "OnEndListElementGUI", CustomAddFunction = "CustomAddFunction")]
        [SerializeField]
        private StateData[] _states = Array.Empty<StateData>();

        public StateData[] States => _states;

        private int _state = -1;

        /// <summary>
        /// Служебная переменная. Хранит путь до слоя на случай исключений связанных с удалением объекта
        /// </summary>
        private string _goName;

        public int State
        {
            get => _state;
            private set
            {
                if (_states.Length == 0 || value == _state)
                    return;

                try
                {
                    // Проверка на выход за границы массива
                    if (value < -1 || value >= _states.Length)
                    {
                        Debug.LogError($"[UIStateSwitcher] Попытка установить некорректный индекс состояния: {value}");
                        return;
                    }

                    foreach (var state in _states)
                        state.DefaultState();

                    if (value != -1)
                        _states[value].Set();

                    _state = value;
                    OnStateSwitch?.Invoke(value == -1 ? null : States[value]);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{_goName}: {ex}");
                }
            }
        }

        /// <summary>
        /// Ивент на смену состояния. Передаётся текущее состояние
        /// </summary>
        public event Action<StateData> OnStateSwitch;

        #endregion

        #region Public

        /// <summary>
        /// Сброс состояний свитчера и переключения его в позицию на старте
        /// </summary>
        [Button]
        public void Reset()
        {
            if (_states.Length == 0)
                return;
            foreach (var state in _states)
                state.DefaultState();

            State = -1;
            if (_setOnEnableState)
                Set(_stateOnEnable);
        }

        /// <summary>
        /// Выставление указанного состояния
        /// </summary>
        /// <param name="state">состояниe в виде строкового названия, дананого в <see cref="StateData"/></param>
        public void Set(string state)
        {
            for (var i = 0; i < _states.Length; i++)
            {
                if (_states[i].Name == state)
                {
                    Set(i);
                    return;
                }
            }

            throw new Exception($"В UIStateSwitcher '{gameObject.name}' остуствует состояние '{state}'");
        }

        /// <summary>
        /// Выставление указанного состояния
        /// </summary>
        /// <param name="state">состояниe в виде элемента перечисления. Используется Хэш код параметра</param>
        public void Set(Enum state) => Set(state.GetHashCode());


        /// <summary>
        /// Выставление указанного состояния
        /// </summary>
        /// <param name="stateNumber">номер состояния в списке</param>
        public void Set(byte stateNumber) => Set((int)stateNumber);

        /// <summary>
        /// Выставление указанного состояния
        /// </summary>
        /// <param name="stateNumber">номер состояния в списке</param>
        public void Set(int stateNumber)
        {
            State = stateNumber;
        }

        public ValueDropdownList<int> GetDropDownStatesList()
        {
            var list = new ValueDropdownList<int>();
            for (var i = 0; i < _states.Length; i++)
            {
                list.Add(_states[i].Name, i);
            }

            return list;
        }

        #endregion

        #region Private

#if UNITY_EDITOR

        private void OnBeginListElementGUI(int index)
        {
            if (index == State)
                GUI.color = new Color(0.7f, 1f, 0.7f);
        }

        private void OnEndListElementGUI(int index)
        {
            GUI.color = Color.white;
        }

        private StateData CustomAddFunction()
        {
            var stateData = new StateData();
            if (!_duplicateOnCreate || _states.Length == 0) return stateData;
            var lastStateData = _states.Last();
            foreach (var stateItem in lastStateData.StateItems)
            {
                stateData.AddStateItem(stateItem.Clone());
            }

            return stateData;
        }

        private void Set(StateData stateData)
        {
            for (var i = _states.Length - 1; i >= 0; i--)
            {
                var state = _states[i];
                if (state != stateData) continue;
                State = i;
                return;
            }
        }
#endif

        private void Awake()
        {
            _goName = name;
            var parent = transform.parent;
            while (parent != null)
            {
                _goName = parent.name + "/" + _goName;
                parent = parent.parent;
            }
        }

        private void OnEnable()
        {
            if (_setOnEnableState)
                Set(_stateOnEnable);
        }

#if UNITY_EDITOR
        private void OnInspectorGUI()
        {
            foreach (var state in _states)
                state.owner = this;
        }
#endif

        #endregion
    }
}