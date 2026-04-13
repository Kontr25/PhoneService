using UnityEngine;

/// <summary>
/// Настройки луча захвата, физики перетаскивания и отпускания для <see cref="GrabInteractionController"/>.
/// </summary>
[CreateAssetMenu(fileName = "GrabInteractionConfig", menuName = "PhoneService/Interaction/Grab Interaction Config")]
public class GrabInteractionConfig : ScriptableObject
{
    /// <summary>
    /// Маска слоёв для raycast (инспектор).
    /// </summary>
    [SerializeField]
    private LayerMask _grabbableLayers = ~0;

    /// <summary>
    /// Макс. длина луча захвата (инспектор).
    /// </summary>
    [SerializeField]
    private float _maxPickDistance = 100f;

    /// <summary>
    /// Множитель линейной скорости при осознанном отпускании кнопки (0 — остановить).
    /// </summary>
    [SerializeField]
    private float _releaseVelocityMultiplier = 1f;

    /// <summary>
    /// Жёсткость притяжения к точке курсора на плоскости (сила ~ k * смещение).
    /// </summary>
    [SerializeField]
    private float _springStrength = 90f;

    /// <summary>
    /// Демпфирование скорости при перетаскивании (чем больше, тем меньше колебаний).
    /// </summary>
    [SerializeField]
    private float _dragVelocityDamping = 14f;

    /// <summary>
    /// Демпфирование вращения на время удержания.
    /// </summary>
    [SerializeField]
    private float _angularDragWhileHeld = 8f;

    /// <summary>
    /// Если расстояние от тела до целевой точки курсора превышает это значение, захват срывается (застревание / увод курсора).
    /// </summary>
    [SerializeField]
    private float _breakGrabDistance = 2.5f;

    /// <summary>
    /// Секунды после захвата без проверки срыва по расстоянию (избегает ложного срыва в первый кадр).
    /// </summary>
    [SerializeField]
    private float _breakGrabGraceTime = 0.12f;

    /// <summary>
    /// Маска слоёв для raycast при pick.
    /// </summary>
    public LayerMask GrabbableLayers => _grabbableLayers;

    /// <summary>
    /// Максимальная дистанция луча при захвате.
    /// </summary>
    public float MaxPickDistance => _maxPickDistance;

    /// <summary>
    /// Множитель линейной скорости при отпускании кнопки.
    /// </summary>
    public float ReleaseVelocityMultiplier => _releaseVelocityMultiplier;

    /// <summary>
    /// Коэффициент пружины к цели на плоскости.
    /// </summary>
    public float SpringStrength => _springStrength;

    /// <summary>
    /// Демпфирование линейной скорости при удержании.
    /// </summary>
    public float DragVelocityDamping => _dragVelocityDamping;

    /// <summary>
    /// Демпфирование угловой скорости при удержании.
    /// </summary>
    public float AngularDragWhileHeld => _angularDragWhileHeld;

    /// <summary>
    /// Порог расстояния для срыва захвата.
    /// </summary>
    public float BreakGrabDistance => _breakGrabDistance;

    /// <summary>
    /// Задержка перед проверкой срыва по расстоянию.
    /// </summary>
    public float BreakGrabGraceTime => _breakGrabGraceTime;
}
