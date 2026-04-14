using UnityEngine;

/// <summary>
/// Вызывается из <see cref="GrabbedRigidbodyDragSession.PhysicsStep"/> после сил удержания (каждый FixedUpdate при активном захвате).
/// </summary>
public interface IGrabPhysicsTick
{
    /// <summary>
    /// Шаг физики при удержании объекта.
    /// </summary>
    /// <param name="camera">Камера перетаскивания.</param>
    /// <param name="screenPoint">Позиция указателя на экране.</param>
    void OnGrabPhysicsStep(Camera camera, Vector2 screenPoint);
}
