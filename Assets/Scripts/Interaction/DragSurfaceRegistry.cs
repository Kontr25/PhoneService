using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Реестр именованных плоскостей drag на сцене. Ссылки на объекты Plane (или аналог) задаются в инспекторе.
/// </summary>
public sealed class DragSurfaceRegistry : MonoBehaviour
{
    /// <summary>
    /// Зарегистрированные поверхности.
    /// </summary>
    [SerializeField]
    private DragSurfaceEntry[] _surfaces = Array.Empty<DragSurfaceEntry>();

    /// <summary>
    /// Кэш id → решатель, собирается в <see cref="Awake"/>.
    /// </summary>
    private Dictionary<string, DragSurfaceSolver> _solvers;

    /// <summary>
    /// Читает список записей (для редактора / Odin).
    /// </summary>
    public IReadOnlyList<DragSurfaceEntry> Surfaces => _surfaces;

    /// <summary>
    /// Собирает словарь решателей; пустые id пропускаются; дубликат или null-корень — исключение.
    /// </summary>
    private void Awake()
    {
        var cap = _surfaces != null ? _surfaces.Length : 0;
        _solvers = new Dictionary<string, DragSurfaceSolver>(Mathf.Max(1, cap), StringComparer.Ordinal);
        if (_surfaces == null)
            return;

        for (var i = 0; i < _surfaces.Length; i++)
        {
            var entry = _surfaces[i];
            var id = entry.Id;
            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (entry.PlaneRoot == null)
                throw new InvalidOperationException(
                    $"{nameof(DragSurfaceRegistry)} на '{name}': у записи с id '{id}' не назначен корень плоскости.");

            if (_solvers.ContainsKey(id))
                throw new InvalidOperationException(
                    $"{nameof(DragSurfaceRegistry)} на '{name}': дубликат id '{id}'.");

            _solvers.Add(id, entry.CreateSolver());
        }
    }

    /// <summary>
    /// Возвращает решатель для id или бросает <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="id">Идентификатор поверхности.</param>
    /// <returns>Готовый <see cref="DragSurfaceSolver"/>.</returns>
    public DragSurfaceSolver Resolve(string id)
    {
        if (!_solvers.TryGetValue(id, out var solver))
            throw new InvalidOperationException($"{nameof(DragSurfaceRegistry)}: неизвестный id поверхности '{id}'. Проверьте массив и {nameof(Grabbable)}.");

        return solver;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Список id для выпадающего списка в инспекторе (открытые сцены).
    /// </summary>
    /// <returns>Уникальные идентификаторы.</returns>
    public static IEnumerable<string> EditorEnumerateIds()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var registry in UnityEngine.Object.FindObjectsByType<DragSurfaceRegistry>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var arr = registry._surfaces;
            if (arr == null)
                continue;

            for (var i = 0; i < arr.Length; i++)
            {
                var id = arr[i].Id;
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (seen.Add(id))
                    yield return id;
            }
        }
    }

    private void OnValidate()
    {
        if (_surfaces == null || _surfaces.Length == 0)
            return;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < _surfaces.Length; i++)
        {
            var id = _surfaces[i].Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"{nameof(DragSurfaceRegistry)} '{name}': пустой id у элемента [{i}].", this);
                continue;
            }

            if (!seen.Add(id))
                Debug.LogWarning($"{nameof(DragSurfaceRegistry)} '{name}': дубликат id '{id}'.", this);
        }
    }
#endif
}
