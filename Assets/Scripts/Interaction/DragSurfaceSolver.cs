using UnityEngine;

/// <summary>
/// Проекция луча/курсора на плоскость по <see cref="Transform.up"/> через <see cref="Transform.position"/>,
/// с ограничением точки в локальных XZ границах меша (Unity Plane в локале лежит в плоскости Y=0).
/// </summary>
public sealed class DragSurfaceSolver
{
    /// <summary>
    /// Корень плоскости.
    /// </summary>
    private readonly Transform _root;

    /// <summary>
    /// Локальные границы меша для клампа; если меша нет — кламп отключён.
    /// </summary>
    private readonly Bounds _meshLocalBounds;

    /// <summary>
    /// True, если ограничивать точку габаритами меша.
    /// </summary>
    private readonly bool _clampToMesh;

    /// <summary>
    /// Создаёт решатель для заданного корня (ожидается объект с <see cref="MeshFilter"/> для клампа).
    /// </summary>
    /// <param name="planeRoot">Трансформ плоскости; не null.</param>
    public DragSurfaceSolver(Transform planeRoot)
    {
        _root = planeRoot;

        var meshFilter = planeRoot.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            _meshLocalBounds = meshFilter.sharedMesh.bounds;
            _clampToMesh = true;
        }
        else
        {
            _meshLocalBounds = default;
            _clampToMesh = false;
        }
    }

    /// <summary>
    /// Строит мировую плоскость на текущий кадр (трансформ может двигаться).
    /// </summary>
    /// <returns>Плоскость с нормалью <see cref="Transform.up"/>.</returns>
    private Plane BuildWorldPlane()
    {
        return new Plane(_root.up, _root.position);
    }

    /// <summary>
    /// Проецирует луч на плоскость, при необходимости клампит в границах меша, затем добавляет смещение вдоль нормали поверхности.
    /// В отличие от <see cref="Plane.Raycast"/>, пересечение ищется по всей прямой луча (в т.ч. «за камерой»), чтобы курсор у края экрана
    /// по-прежнему задавал точку на плоскости и объект скользил по границе меша, а не терял цель.
    /// </summary>
    /// <param name="ray">Луч в мировых координатах.</param>
    /// <param name="holdHeightAlongSurfaceNormal">Смещение после проекции вдоль <see cref="Transform.up"/> корня плоскости.</param>
    /// <param name="target">Итоговая целевая точка для силы.</param>
    /// <returns>False только в патологических случаях (например NaN).</returns>
    public bool TryProjectRay(Ray ray, float holdHeightAlongSurfaceNormal, out Vector3 target)
    {
        var plane = BuildWorldPlane();
        if (!TryLinePlaneIntersection(ray, plane, out var onPlane))
        {
            target = default;
            return false;
        }

        onPlane = ClampOnSurface(onPlane);
        target = onPlane + _root.up * holdHeightAlongSurfaceNormal;
        return true;
    }

    /// <summary>
    /// Пересечение бесконечной прямой (луч как линия) с плоскостью. При параллели — ближайшая к началу луча точка на плоскости.
    /// </summary>
    private static bool TryLinePlaneIntersection(Ray ray, Plane plane, out Vector3 pointOnPlane)
    {
        var n = plane.normal;
        var denom = Vector3.Dot(n, ray.direction);
        const float eps = 1e-5f;

        if (Mathf.Abs(denom) < eps)
        {
            pointOnPlane = plane.ClosestPointOnPlane(ray.origin);
            return true;
        }

        var t = (-Vector3.Dot(n, ray.origin) - plane.distance) / denom;
        pointOnPlane = ray.GetPoint(t);
        return true;
    }

    /// <summary>
    /// Проецирует позицию экрана через камеру на плоскость, затем кламп и смещение вдоль нормали.
    /// </summary>
    /// <param name="camera">Камера.</param>
    /// <param name="screenPoint">Пиксели.</param>
    /// <param name="holdHeightAlongSurfaceNormal">Смещение вдоль нормали поверхности (<see cref="Transform.up"/>).</param>
    /// <param name="target">Целевая точка.</param>
    /// <returns>False только при ошибке геометрии.</returns>
    public bool TryProjectScreen(Camera camera, Vector2 screenPoint, float holdHeightAlongSurfaceNormal, out Vector3 target)
    {
        return TryProjectRay(camera.ScreenPointToRay(screenPoint), holdHeightAlongSurfaceNormal, out target);
    }

    /// <summary>
    /// Переводит точку на бесконечной плоскости в локальные координаты корня, клампит XZ по bounds меша, возвращает в мир.
    /// </summary>
    /// <param name="worldOnPlane">Точка уже лежащая на плоскости (в мире).</param>
    /// <returns>Точка на плоскости с учётом клампа.</returns>
    private Vector3 ClampOnSurface(Vector3 worldOnPlane)
    {
        if (!_clampToMesh)
            return worldOnPlane;

        var local = _root.InverseTransformPoint(worldOnPlane);
        var b = _meshLocalBounds;
        local.x = Mathf.Clamp(local.x, b.min.x, b.max.x);
        local.z = Mathf.Clamp(local.z, b.min.z, b.max.z);
        local.y = 0f;
        return _root.TransformPoint(local);
    }
}
