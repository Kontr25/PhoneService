using System.Collections.Generic;
using UnityEngine;

namespace Pools.Examples
{
    /// <summary>
    /// Пример контроллера, демонстрирующего базовое использование пула.
    /// </summary>
    public sealed class PoolUsageExample : MonoBehaviour
    {
        /// <summary>
        /// Ссылка на компонент пула в сцене.
        /// </summary>
        [SerializeField]
        private Pool _pool;

        /// <summary>
        /// Точка, где создаются новые элементы.
        /// </summary>
        [SerializeField]
        private Transform _spawnPoint;

        /// <summary>
        /// Смещение между элементами при последовательном спавне.
        /// </summary>
        [SerializeField]
        private float _step = 1.5f;

        /// <summary>
        /// Выданные пулом элементы, которые сейчас активны.
        /// </summary>
        private readonly List<ExamplePoolItem> _activeItems = new();

        /// <summary>
        /// Создает один новый элемент из пула и размещает его в позиции спавна.
        /// </summary>
        [ContextMenu("Spawn Item")]
        public void SpawnItem()
        {
            ExamplePoolItem item = _pool.GetElement<ExamplePoolItem>();
            int index = _activeItems.Count;
            item.transform.SetParent(_spawnPoint, false);
            item.transform.localPosition = new Vector3(index * _step, 0f, 0f);
            _activeItems.Add(item);
        }

        /// <summary>
        /// Возвращает в пул последний выданный элемент.
        /// </summary>
        [ContextMenu("Release Last Item")]
        public void ReleaseLastItem()
        {
            if (_activeItems.Count == 0)
            {
                return;
            }

            int index = _activeItems.Count - 1;
            ExamplePoolItem item = _activeItems[index];
            _activeItems.RemoveAt(index);
            _pool.Release(item);
        }

        /// <summary>
        /// Возвращает в пул все выданные элементы.
        /// </summary>
        [ContextMenu("Release All Items")]
        public void ReleaseAllItems()
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                _pool.Release(_activeItems[i]);
            }

            _activeItems.Clear();
        }

        /// <summary>
        /// Очищает внутренний список при отключении компонента.
        /// </summary>
        private void OnDisable()
        {
            _activeItems.Clear();
        }
    }
}
