using UnityEngine;

/// <summary>
/// Сессия перетаскивания: динамическое <see cref="Rigidbody"/>, силы к точке на плоскости в фиксированном шаге,
/// срыв при превышении расстояния до цели (застревание / сильный увод курсора).
/// </summary>
public sealed class GrabbedRigidbodyDragSession
{
    /// <summary>
    /// Настройки сил и срыва.
    /// </summary>
    private readonly GrabInteractionConfig _config;

    /// <summary>
    /// Удерживаемый контракт или null.
    /// </summary>
    private IGrabbable _held;

    /// <summary>
    /// Тело.
    /// </summary>
    private Rigidbody _heldBody;

    /// <summary>
    /// Плоскость перетаскивания.
    /// </summary>
    private Plane _dragPlane;

    /// <summary>
    /// Время захвата (<see cref="Time.time"/>) для grace до проверки срыва.
    /// </summary>
    private float _grabTime;

    /// <summary>
    /// Сохранённый kinematic до захвата.
    /// </summary>
    private bool _prevKinematic;

    /// <summary>
    /// Сохранённый useGravity до захвата.
    /// </summary>
    private bool _prevUseGravity;

    /// <summary>
    /// Сохранённые constraints до захвата.
    /// </summary>
    private RigidbodyConstraints _prevConstraints;

    /// <summary>
    /// Создаёт сессию.
    /// </summary>
    /// <param name="config">Конфигурация; не null.</param>
    public GrabbedRigidbodyDragSession(GrabInteractionConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// True, если сессия активна.
    /// </summary>
    public bool IsActive => _held != null;

    /// <summary>
    /// Начинает удержание: плоскость по точке попадания, тело остаётся/становится динамическим для коллизий.
    /// </summary>
    /// <param name="hit">Попадание луча.</param>
    /// <param name="grabbable">Контракт.</param>
    /// <param name="pickRay">Луч для начальной цели на плоскости.</param>
    public void Begin(in RaycastHit hit, IGrabbable grabbable, Ray pickRay)
    {
        if (IsActive)
            End(false);

        var rb = grabbable.PhysicsBody;
        _held = grabbable;
        _heldBody = rb;

        _dragPlane = new Plane(Vector3.up, hit.point);

        _prevKinematic = rb.isKinematic;
        _prevUseGravity = rb.useGravity;
        _prevConstraints = rb.constraints;

        rb.isKinematic = false;
        rb.useGravity = _prevUseGravity;

        if (grabbable.FreezeRotationWhileHeld)
            rb.constraints |= RigidbodyConstraints.FreezeRotation;

        _grabTime = Time.time;

        if (_dragPlane.Raycast(pickRay, out var enter))
        {
            var target = pickRay.GetPoint(enter) + Vector3.up * _held.HoldHeightOffset;
            var delta = target - rb.worldCenterOfMass;
            if (delta.sqrMagnitude > 1e-6f)
                rb.AddForce(delta * _config.SpringStrength, ForceMode.Force);
        }
    }

    /// <summary>
    /// Один шаг физики: цель из луча камеры, силы, опционально срыв по расстоянию.
    /// </summary>
    /// <param name="camera">Камера.</param>
    /// <param name="screenPoint">Текущая позиция указателя.</param>
    public void PhysicsStep(Camera camera, Vector2 screenPoint)
    {
        if (!IsActive)
            return;

        var ray = camera.ScreenPointToRay(screenPoint);

        if (!_dragPlane.Raycast(ray, out var enter))
            return;

        var target = ray.GetPoint(enter) + Vector3.up * _held.HoldHeightOffset;
        var bodyPoint = _heldBody.worldCenterOfMass;
        var error = target - bodyPoint;

        if (Time.time >= _grabTime + _config.BreakGrabGraceTime &&
            error.magnitude > _config.BreakGrabDistance)
        {
            End(true);
            return;
        }

        var v = _heldBody.linearVelocity;
        var force = _config.SpringStrength * error - _config.DragVelocityDamping * v;
        _heldBody.AddForce(force, ForceMode.Force);

        if (_config.AngularDragWhileHeld > 0f && _heldBody.angularVelocity.sqrMagnitude > 1e-8f)
            _heldBody.AddTorque(-_heldBody.angularVelocity * _config.AngularDragWhileHeld, ForceMode.Force);
    }

    /// <summary>
    /// Завершает сессию и восстанавливает состояние <see cref="Rigidbody"/>.
    /// Повторный вызов при неактивной сессии — без эффекта (идемпотентность).
    /// </summary>
    /// <param name="preserveVelocities">True при срыве захвата: не менять линейную/угловую скорость.</param>
    public void End(bool preserveVelocities = false)
    {
        if (!IsActive)
            return;

        var rb = _heldBody;

        rb.isKinematic = _prevKinematic;
        rb.useGravity = _prevUseGravity;
        rb.constraints = _prevConstraints;

        if (!preserveVelocities)
        {
            rb.angularVelocity = Vector3.zero;

            if (!_prevKinematic)
            {
                var mult = _config.ReleaseVelocityMultiplier;
                if (Mathf.Abs(mult) < 1e-5f)
                    rb.linearVelocity = Vector3.zero;
                else
                    rb.linearVelocity *= mult;
            }
        }

        _held = null;
        _heldBody = null;
    }
}
