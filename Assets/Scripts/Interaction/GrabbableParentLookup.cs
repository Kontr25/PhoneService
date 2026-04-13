using UnityEngine;

/// <summary>
/// Поиск <see cref="IGrabbable"/> на объекте коллайдера и его предках через <see cref="Component.TryGetComponent{T}(out T)"/>.
/// </summary>
public static class GrabbableParentLookup
{
    /// <summary>
    /// Поднимается от <paramref name="collider"/> к корню и возвращает первый найденный <see cref="IGrabbable"/> на том же <see cref="GameObject"/>.
    /// </summary>
    /// <param name="collider">Коллайдер попадания луча.</param>
    /// <param name="grabbable">Найденный контракт или значение по умолчанию.</param>
    /// <returns>True, если контракт найден.</returns>
    public static bool TryFind(Collider collider, out IGrabbable grabbable)
    {
        var transform = collider.transform;

        while (transform != null)
        {
            if (transform.TryGetComponent(out grabbable))
                return true;

            transform = transform.parent;
        }

        grabbable = null;
        return false;
    }
}
