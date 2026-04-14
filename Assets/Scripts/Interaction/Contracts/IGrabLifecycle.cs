using UnityEngine;

/// <summary>
/// Опциональные колбэки до/после применения состояния <see cref="Rigidbody"/> в <see cref="GrabbedRigidbodyDragSession"/>.
/// </summary>
public interface IGrabLifecycle
{
    /// <summary>
    /// Вызывается сразу после назначения удерживаемого объекта, до чтения и изменения kinematic/constraints.
    /// Нужен для отсоединения от родителя (запчасть от телефона) до перевода тела в dynamic.
    /// </summary>
    /// <param name="pickRay">Луч захвата.</param>
    void OnGrabSessionStarting(in Ray pickRay);

    /// <summary>
    /// Вызывается перед обнулением ссылок сессии, после восстановления <see cref="Rigidbody"/> и коррекции скорости.
    /// </summary>
    /// <param name="preserveVelocities">Тот же флаг, что у <see cref="GrabbedRigidbodyDragSession.End"/>.</param>
    void OnGrabSessionEnded(bool preserveVelocities);
}
