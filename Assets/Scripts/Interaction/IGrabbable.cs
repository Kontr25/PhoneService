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
    /// Дополнительное смещение вдоль мирового вверх от плоскости перетаскивания при удержании.
    /// </summary>
    float HoldHeightOffset { get; }

    /// <summary>
    /// Если true, при удержании к телу добавляется заморозка вращения.
    /// </summary>
    bool FreezeRotationWhileHeld { get; }
}
