using UnityEngine;

/// <summary>
/// Поиск <see cref="IGrabbable"/> по заранее зарегистрированному коллайдеру.
/// </summary>
public static class GrabbableParentLookup
{
    /// <summary>
    /// Возвращает <see cref="IGrabbable"/> по коллайдеру из hit-теста.
    /// </summary>
    /// <param name="collider">Коллайдер попадания луча.</param>
    /// <param name="grabbable">Найденный контракт или значение по умолчанию.</param>
    /// <returns>True, если контракт найден.</returns>
    public static bool TryFind(Collider collider, out IGrabbable grabbable)
    {
        return Grabbable.TryResolveByCollider(collider, out grabbable);
    }
}
