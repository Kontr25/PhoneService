using UnityEngine;
using Zenject;

namespace Pools
{
    /// <summary>
    /// Реализация фабрики элементов пула через контейнер Zenject.
    /// </summary>
    public sealed class ZenjectPoolElementFactory : IPoolElementFactory
    {
        /// <summary>
        /// Контейнер текущего контекста Zenject.
        /// </summary>
        private readonly DiContainer _container;

        /// <summary>
        /// Создает фабрику на основе контейнера Zenject.
        /// </summary>
        /// <param name="container">Контейнер текущего контекста.</param>
        public ZenjectPoolElementFactory(DiContainer container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public ElementInPool Create(ElementInPool prefab, Transform parent)
        {
            return _container.InstantiatePrefabForComponent<ElementInPool>(prefab, parent);
        }
    }
}
