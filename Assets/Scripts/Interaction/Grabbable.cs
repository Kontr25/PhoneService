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
    private Rigidbody _rigidbody;

    /// <inheritdoc />
    public Rigidbody PhysicsBody => _rigidbody;

    /// <inheritdoc />
    public float HoldHeightOffset => _holdHeightOffset;

    /// <inheritdoc />
    public bool FreezeRotationWhileHeld => _freezeRotationWhileHeld;

    /// <inheritdoc />
    public string DragSurfaceId => _dragSurfaceId;

    /// <summary>
    /// Кэширует <see cref="Rigidbody"/> для <see cref="PhysicsBody"/>.
    /// </summary>
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
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
