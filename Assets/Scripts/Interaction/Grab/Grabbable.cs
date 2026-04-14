using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Реализация <see cref="IGrabbable"/> на базе <see cref="MonoBehaviour"/>; маркирует объект для <see cref="GrabInteractionController"/>.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour, IGrabbable
{
    /// <summary>
    /// Реестр коллайдеров захвата для быстрого поиска контракта.
    /// </summary>
    private static readonly Dictionary<Collider, IGrabbable> GrabbableByCollider =
        new Dictionary<Collider, IGrabbable>(128);

    /// <summary>
    /// Имя поверхности из <see cref="DragSurfaceRegistry"/> на сцене (список собирается в редакторе).
    /// </summary>
    [Tooltip("Id поверхности из Drag Surface Registry (Table, Wall, …).")]
    [ValueDropdown(nameof(EditorDragSurfaceIds), IsUniqueList = false, DropdownTitle = "Поверхности")]
    [SerializeField]
    private string _dragSurfaceId;

    /// <summary>
    /// Смещение вдоль мирового вверх от плоскости перетаскивания в инспекторе.
    /// </summary>
    [Tooltip("Смещение цели от плоскости вдоль её нормали (Transform.up корня в Drag Surface Registry).")]
    [FormerlySerializedAs("holdHeightOffset")]
    [SerializeField]
    private float _holdHeightOffset;

    /// <summary>
    /// Замораживать вращение <see cref="Rigidbody"/> на время удержания (инспектор).
    /// </summary>
    [FormerlySerializedAs("freezeRotationWhileHeld")]
    [SerializeField]
    private bool _freezeRotationWhileHeld = true;

    /// <summary>
    /// Кэш компонента <see cref="Rigidbody"/> на этом объекте.
    /// </summary>
    [SerializeField]
    private Rigidbody _rigidbody;

    /// <summary>
    /// Коллайдеры, относящиеся к этому grabbable-объекту.
    /// </summary>
    [SerializeField]
    private Collider[] _grabColliders = Array.Empty<Collider>();

    /// <inheritdoc />
    public Rigidbody PhysicsBody => _rigidbody;

    /// <inheritdoc />
    public float HoldHeightOffset => _holdHeightOffset;

    /// <inheritdoc />
    public bool FreezeRotationWhileHeld => _freezeRotationWhileHeld;

    /// <inheritdoc />
    public string DragSurfaceId => _dragSurfaceId;

    /// <summary>
    /// Проверяет ссылку на <see cref="Rigidbody"/> и вызывает <see cref="OnGrabbableAwake"/>.
    /// </summary>
    private void Awake()
    {
        OnGrabbableAwake();
    }

    /// <summary>
    /// Регистрирует коллайдеры в глобальном словаре выбора объекта.
    /// </summary>
    private void OnEnable()
    {
        for (var i = 0; i < _grabColliders.Length; i++)
        {
            var collider = _grabColliders[i];
            if (collider == null)
                throw new System.InvalidOperationException();

            GrabbableByCollider[collider] = this;
        }
    }

    /// <summary>
    /// Удаляет коллайдеры из глобального словаря выбора объекта.
    /// </summary>
    private void OnDisable()
    {
        if (_grabColliders == null)
            return;

        for (var i = 0; i < _grabColliders.Length; i++)
        {
            var collider = _grabColliders[i];
            if (collider != null)
                GrabbableByCollider.Remove(collider);
        }
    }

    /// <summary>
    /// Возвращает контракт grabbable по коллайдеру.
    /// </summary>
    /// <param name="collider">Коллайдер из hit-теста.</param>
    /// <param name="grabbable">Найденный контракт.</param>
    /// <returns>True, если коллайдер зарегистрирован.</returns>
    internal static bool TryResolveByCollider(Collider collider, out IGrabbable grabbable)
    {
        return GrabbableByCollider.TryGetValue(collider, out grabbable);
    }

    /// <summary>
    /// Хук после кеширования <see cref="Rigidbody"/>; переопределять в наследниках.
    /// </summary>
    protected virtual void OnGrabbableAwake()
    {
    }

#if UNITY_EDITOR
    /// <summary>
    /// Источник имён для Odin: все <see cref="DragSurfaceRegistry"/> в открытых сценах.
    /// </summary>
    private static IEnumerable<string> EditorDragSurfaceIds() => DragSurfaceRegistry.EditorEnumerateIds();
#else
    private static IEnumerable<string> EditorDragSurfaceIds()
    {
        yield break;
    }
#endif
}
