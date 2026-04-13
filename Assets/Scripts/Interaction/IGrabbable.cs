using UnityEngine;

/// <summary>
/// Контракт объекта, который система захвата может удерживать и перемещать по плоскости drag.
/// </summary>
public interface IGrabbable
{
    /// <summary>
    /// Тело, к которому применяется кинематическое перемещение при удержании.
    /// </summary>
    Rigidbody PhysicsBody { get; }

    /// <summary>
    /// Смещение цели от плоскости вдоль нормали поверхности (<see cref="UnityEngine.Transform.up"/> корня плоскости в реестре).
    /// </summary>
    float HoldHeightOffset { get; }

    /// <summary>
    /// Если true, при удержании к телу добавляется заморозка вращения.
    /// </summary>
    bool FreezeRotationWhileHeld { get; }

    /// <summary>
    /// Идентификатор поверхности в <see cref="DragSurfaceRegistry"/> (проекция курсора и границы drag).
    /// </summary>
    string DragSurfaceId { get; }
}
