using UnityEngine;

/// <summary>
/// Выбор <see cref="IGrabbable"/> лучом из камеры по настройкам <see cref="GrabInteractionConfig"/>.
/// </summary>
public sealed class GrabTargetSelector
{
    /// <summary>
    /// Конфигурация дистанции и масок слоёв.
    /// </summary>
    private readonly GrabInteractionConfig _config;

    /// <summary>
    /// Создаёт селектор с заданным конфигом.
    /// </summary>
    /// <param name="config">Настройки; не должен быть null.</param>
    public GrabTargetSelector(GrabInteractionConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Выполняет raycast из камеры в экранную точку и ищет <see cref="IGrabbable"/> на попадании.
    /// </summary>
    /// <param name="camera">Камера луча.</param>
    /// <param name="screenPoint">Позиция в пикселях.</param>
    /// <param name="ray">Построенный луч захвата.</param>
    /// <param name="hit">Попадание физики.</param>
    /// <param name="grabbable">Найденный контракт или null.</param>
    /// <returns>True, если луч попал и на коллайдере или предке есть <see cref="IGrabbable"/>.</returns>
    public bool TrySelect(Camera camera, Vector2 screenPoint, out Ray ray, out RaycastHit hit, out IGrabbable grabbable)
    {
        ray = camera.ScreenPointToRay(screenPoint);

        if (!Physics.Raycast(ray, out hit, _config.MaxPickDistance, _config.GrabbableLayers, QueryTriggerInteraction.Ignore))
        {
            grabbable = null;
            return false;
        }

        if (!GrabbableParentLookup.TryFind(hit.collider, out grabbable))
        {
            grabbable = null;
            return false;
        }

        return true;
    }
}
