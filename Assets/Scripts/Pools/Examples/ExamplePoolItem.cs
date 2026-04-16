using UnityEngine;

namespace Pools.Examples
{
    /// <summary>
    /// Пример элемента, который может быть создан и переиспользован пулом.
    /// </summary>
    public sealed class ExamplePoolItem : ElementInPool
    {
        /// <summary>
        /// Рендерер, которому назначаются тестовые цвета активности.
        /// </summary>
        [SerializeField]
        private Renderer _renderer;

        /// <summary>
        /// Цвет объекта в активном состоянии.
        /// </summary>
        [SerializeField]
        private Color _activeColor = Color.green;

        /// <summary>
        /// Цвет объекта в состоянии хранения в пуле.
        /// </summary>
        [SerializeField]
        private Color _inactiveColor = Color.gray;

        /// <summary>
        /// Вызывается после получения объекта из пула.
        /// </summary>
        public override void Init()
        {
            _renderer.material.color = _activeColor;
        }

        /// <summary>
        /// Вызывается перед возвратом объекта в пул.
        /// </summary>
        public override void DeInit()
        {
            _renderer.material.color = _inactiveColor;
        }
    }
}
