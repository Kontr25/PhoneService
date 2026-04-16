using UnityEngine;

namespace Pools
{
    /// <summary>
    /// Контракт фабрики создания элементов пула.
    /// </summary>
    public interface IPoolElementFactory
    {
        /// <summary>
        /// Создает новый экземпляр элемента из префаба в указанном родителе.
        /// </summary>
        /// <param name="prefab">Префаб создаваемого элемента.</param>
        /// <param name="parent">Родительский трансформ инстанса.</param>
        /// <returns>Новый экземпляр элемента пула.</returns>
        ElementInPool Create(ElementInPool prefab, Transform parent);
    }
}
