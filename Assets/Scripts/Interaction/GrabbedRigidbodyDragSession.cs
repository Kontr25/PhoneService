using UnityEngine;

/// <summary>
/// Сессия перетаскивания: динамическое <see cref="Rigidbody"/>, силы к точке на плоскости в фиксированном шаге,
/// опциональный срыв по расстоянию до цели (в конфиге по умолчанию выключен).
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
    /// Именованная поверхность: проекция луча и кламп по мешу.
    /// </summary>
    private DragSurfaceSolver _dragSurface;

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
    /// Начинает удержание: цель на именованной поверхности, тело остаётся/становится динамическим для коллизий.
    /// </summary>
    /// <param name="grabbable">Контракт.</param>
    /// <param name="pickRay">Луч для начальной цели на плоскости.</param>
    /// <param name="dragSurface">Поверхность из <see cref="DragSurfaceRegistry"/>.</param>
    public void Begin(IGrabbable grabbable, Ray pickRay, DragSurfaceSolver dragSurface)
    {
        if (IsActive)
            End(false);

        var rb = grabbable.PhysicsBody;
        _held = grabbable;
        _heldBody = rb;

        _dragSurface = dragSurface;

        if (grabbable is IGrabLifecycle grabLife)
            grabLife.OnGrabSessionStarting(pickRay);

        _prevKinematic = rb.isKinematic;
        _prevUseGravity = rb.useGravity;
        _prevConstraints = rb.constraints;

        rb.isKinematic = false;
        rb.useGravity = _prevUseGravity;

        if (grabbable.FreezeRotationWhileHeld)
            rb.constraints |= RigidbodyConstraints.FreezeRotation;

        _grabTime = Time.time;

        if (_dragSurface.TryProjectRay(pickRay, _held.HoldHeightOffset, out var target))
        {
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

        if (!_dragSurface.TryProjectScreen(camera, screenPoint, _held.HoldHeightOffset, out var target))
            return;
        var bodyPoint = _heldBody.worldCenterOfMass;
        var error = target - bodyPoint;

        if (_config.BreakGrabWhenTargetTooFar &&
            Time.time >= _grabTime + _config.BreakGrabGraceTime &&
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
        var held = _held;

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

        if (held is IGrabLifecycle grabLife)
            grabLife.OnGrabSessionEnded(preserveVelocities);

        _held = null;
        _heldBody = null;
        _dragSurface = null;
    }
}
