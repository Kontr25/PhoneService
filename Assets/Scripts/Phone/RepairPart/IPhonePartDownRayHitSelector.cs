using UnityEngine;

/// <summary>
/// Выбор точки попадания луча вниз от детали с исключением собственного тела и коллайдера сферы-пробы.
/// </summary>
public interface IPhonePartDownRayHitSelector
{
    /// <summary>
    /// Ищет ближайшую к <paramref name="origin"/> точку вдоль мирового вниз среди попаданий <see cref="Physics.RaycastNonAlloc"/>.
    /// </summary>
    /// <param name="partRigidbody">Тело детали; попадания с тем же <see cref="Rigidbody"/> игнорируются.</param>
    /// <param name="probeColliderToIgnore">Коллайдер пробы или null.</param>
    /// <param name="origin">Начало луча (например центр масс).</param>
    /// <param name="layerMask">Маска слоёв.</param>
    /// <param name="maxDistance">Максимальная длина.</param>
    /// <param name="hitBuffer">Буфер попаданий; при необходимости расширяется.</param>
    /// <param name="hitPoint">Точка ближайшего подходящего попадания.</param>
    /// <returns>True, если найдено подходящее попадание.</returns>
    bool TrySelectHitPointBelow(
        Rigidbody partRigidbody,
        Collider probeColliderToIgnore,
        Vector3 origin,
        LayerMask layerMask,
        float maxDistance,
        ref RaycastHit[] hitBuffer,
        out Vector3 hitPoint);
}
