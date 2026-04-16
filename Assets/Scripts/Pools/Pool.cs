using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Pools
{
    /// <summary>
    /// Универсальный пул для элементов, наследованных от <see cref="ElementInPool"/>.
    /// </summary>
    public sealed class Pool : MonoBehaviour
    {
        /// <summary>
        /// Фабрика создания новых элементов пула через контейнер Zenject.
        /// </summary>
        [Inject]
        private IPoolElementFactory _poolElementFactory;

        /// <summary>
        /// Событие вызывается после изменения состояния элементов пула.
        /// </summary>
        public event Action OnUpdateList;

        /// <summary>
        /// Список доступных префабов для создания новых элементов.
        /// </summary>
        [SerializeField]
        private List<ElementInPool> _prefabs = new();

        /// <summary>
        /// Полный индекс всех элементов, созданных пулом.
        /// </summary>
        private readonly List<ElementInPool> _allElements = new();

        /// <summary>
        /// Стек доступных элементов по типу.
        /// </summary>
        private readonly Dictionary<Type, Stack<ElementInPool>> _availableElementsByType = new();

        /// <summary>
        /// Кеш префабов по их точному типу.
        /// </summary>
        private readonly Dictionary<Type, ElementInPool> _prefabByType = new();

        /// <summary>
        /// Признак завершенной инициализации пула.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Инициализирует кеш префабов при запуске.
        /// </summary>
        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Возвращает все элементы указанного типа, созданные пулом.
        /// </summary>
        /// <typeparam name="T">Тип элемента в пуле.</typeparam>
        /// <returns>Список элементов запрошенного типа.</returns>
        public List<T> GetAllElements<T>() where T : ElementInPool
        {
            EnsureInitialized();
            List<T> result = new();
            for (int i = 0; i < _allElements.Count; i++)
            {
                if (_allElements[i] is T typedElement)
                {
                    result.Add(typedElement);
                }
            }

            return result;
        }

        /// <summary>
        /// Возвращает элемент указанного типа из пула.
        /// </summary>
        /// <typeparam name="T">Тип элемента в пуле.</typeparam>
        /// <returns>Активированный элемент.</returns>
        public T GetElement<T>() where T : ElementInPool
        {
            EnsureInitialized();
            Type requestedType = typeof(T);
            Type concreteType = ResolveConcreteType(requestedType);
            if (_availableElementsByType.TryGetValue(concreteType, out Stack<ElementInPool> stack) && stack.Count > 0)
            {
                ElementInPool pooledElement = stack.Pop();
                pooledElement.ActivateFromPool();
                pooledElement.transform.SetAsLastSibling();
                OnUpdateList?.Invoke();
                return (T)pooledElement;
            }

            T createdElement = CreateElement<T>(requestedType);
            createdElement.ActivateFromPool();
            OnUpdateList?.Invoke();
            return createdElement;
        }

        /// <summary>
        /// Возвращает в пул все активные элементы.
        /// </summary>
        public void Clean()
        {
            EnsureInitialized();
            for (int i = 0; i < _allElements.Count; i++)
            {
                Release(_allElements[i]);
            }
        }

        /// <summary>
        /// Возвращает элемент в пул.
        /// </summary>
        /// <param name="elementInPool">Элемент для возврата.</param>
        public void Release(ElementInPool elementInPool)
        {
            EnsureInitialized();
            if (elementInPool == null)
            {
                throw new InvalidOperationException();
            }

            if (!_allElements.Contains(elementInPool))
            {
                throw new InvalidOperationException();
            }

            if (!elementInPool.IsActive())
            {
                return;
            }

            Type elementType = elementInPool.GetType();
            if (!_availableElementsByType.TryGetValue(elementType, out Stack<ElementInPool> stack))
            {
                stack = new Stack<ElementInPool>();
                _availableElementsByType[elementType] = stack;
            }

            elementInPool.DeactivateToPool();
            stack.Push(elementInPool);
            OnUpdateList?.Invoke();
        }

        /// <summary>
        /// Сортирует только активные элементы согласно переданному компаратору.
        /// </summary>
        /// <param name="comparer">Компаратор для сортировки.</param>
        public void Sort(IComparer<ElementInPool> comparer)
        {
            EnsureInitialized();
            List<ElementInPool> activeElements = new();
            for (int i = 0; i < _allElements.Count; i++)
            {
                if (_allElements[i].IsActive())
                {
                    activeElements.Add(_allElements[i]);
                }
            }

            activeElements.Sort(comparer);
            for (int i = 0; i < activeElements.Count; i++)
            {
                activeElements[i].SetPosition(i);
            }
        }

        /// <summary>
        /// Выполняет ленивую инициализацию внутренних структур пула.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            _allElements.Clear();
            _availableElementsByType.Clear();
            _prefabByType.Clear();

            for (int i = 0; i < _prefabs.Count; i++)
            {
                ElementInPool prefab = _prefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                Type prefabType = prefab.GetType();
                if (!_prefabByType.ContainsKey(prefabType))
                {
                    _prefabByType.Add(prefabType, prefab);
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Создает новый экземпляр элемента указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип создаваемого элемента.</typeparam>
        /// <param name="requestedType">Изначально запрошенный тип.</param>
        /// <returns>Созданный экземпляр элемента.</returns>
        private T CreateElement<T>(Type requestedType) where T : ElementInPool
        {
            ElementInPool prefab = GetPrefab(requestedType);
            ElementInPool instance = _poolElementFactory.Create(prefab, transform);
            instance.SetOwnerPool(this);
            instance.gameObject.name = $"{prefab.name}<{requestedType.Name}>";
            _allElements.Add(instance);
            return (T)instance;
        }

        /// <summary>
        /// Возвращает подходящий тип элемента, доступный в кеше префабов.
        /// </summary>
        /// <param name="requestedType">Изначально запрошенный тип.</param>
        /// <returns>Фактический тип для выдачи из пула.</returns>
        private Type ResolveConcreteType(Type requestedType)
        {
            if (_prefabByType.ContainsKey(requestedType))
            {
                return requestedType;
            }

            foreach (KeyValuePair<Type, ElementInPool> pair in _prefabByType)
            {
                if (pair.Key.IsSubclassOf(requestedType))
                {
                    return pair.Key;
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Возвращает префаб для запрошенного типа.
        /// </summary>
        /// <param name="requestedType">Изначально запрошенный тип.</param>
        /// <returns>Префаб элемента.</returns>
        private ElementInPool GetPrefab(Type requestedType)
        {
            Type concreteType = ResolveConcreteType(requestedType);
            return _prefabByType[concreteType];
        }
    }
}