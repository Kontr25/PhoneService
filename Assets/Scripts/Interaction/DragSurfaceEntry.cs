using System;
using UnityEngine;

/// <summary>
/// Одна именованная поверхность drag: корень трансформа (нормаль = <see cref="Transform.up"/>, точка на плоскости = позиция) и опционально кламп по <see cref="Mesh.bounds"/> на том же объекте.
/// </summary>
[Serializable]
public struct DragSurfaceEntry
{
    /// <summary>
    /// Уникальный идентификатор (совпадает с <see cref="IGrabbable.DragSurfaceId"/>).
    /// </summary>
    [SerializeField]
    private string _id;

    /// <summary>
    /// Объект сцены (например примитив Plane): ориентация задаёт плоскость, масштаб/меш — границы.
    /// </summary>
    [SerializeField]
    private Transform _planeRoot;

    /// <summary>
    /// Идентификатор поверхности.
    /// </summary>
    public readonly string Id => _id;

    /// <summary>
    /// Корень плоскости в сцене.
    /// </summary>
    public readonly Transform PlaneRoot => _planeRoot;

    /// <summary>
    /// Создаёт решатель проекции для этой записи.
    /// </summary>
    /// <returns>Экземпляр <see cref="DragSurfaceSolver"/>.</returns>
    public readonly DragSurfaceSolver CreateSolver()
    {
        return new DragSurfaceSolver(_planeRoot);
    }
}
