using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Реализация <see cref="IGrabbable"/> на базе <see cref="MonoBehaviour"/>; маркирует объект для <see cref="GrabInteractionController"/>.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour, IGrabbable
{
    /// <summary>
    /// Смещение вдоль мирового вверх от плоскости перетаскивания в инспекторе.
    /// </summary>
    [Tooltip("Смещение вдоль мирового вверх от плоскости перетаскивания, пока объект удерживается.")]
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

    /// <summary>
    /// Кэширует <see cref="Rigidbody"/> для <see cref="PhysicsBody"/>.
    /// </summary>
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
