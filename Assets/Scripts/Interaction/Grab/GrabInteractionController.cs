using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Связывает ввод с <see cref="GrabTargetSelector"/> и <see cref="GrabbedRigidbodyDragSession"/>.
/// Перетаскивание — силами в <see cref="FixedUpdate"/>; позиция указателя читается каждый фиксированный шаг.
/// Обязательные ссылки и конфиг должны быть назначены в инспекторе (fail-fast при ошибке).
/// </summary>
public class GrabInteractionController : MonoBehaviour
{
    /// <summary>
    /// Камера для луча pick и drag.
    /// </summary>
    [FormerlySerializedAs("targetCamera")]
    [SerializeField]
    private Camera _targetCamera;

    /// <summary>
    /// Экшен позиции указателя (Vector2).
    /// </summary>
    [Tooltip("Экшен позиции курсора/тача (Value, Vector2).")]
    [SerializeField]
    private InputActionReference _pointerPosition;

    /// <summary>
    /// Экшен нажатия (Button): started / canceled.
    /// </summary>
    [Tooltip("Экшен кнопки: started = нажатие, canceled = отпускание.")]
    [SerializeField]
    private InputActionReference _grabPress;

    /// <summary>
    /// Параметры захвата и физики drag.
    /// </summary>
    [SerializeField]
    private GrabInteractionConfig _config;

    /// <summary>
    /// Реестр именованных плоскостей на сцене (Table, Wall, …).
    /// </summary>
    [Tooltip("Реестр плоскостей: id должны совпадать с Grabbable.DragSurfaceId.")]
    [SerializeField]
    private DragSurfaceRegistry _dragSurfaces;

    /// <summary>
    /// Кэш экшена позиции.
    /// </summary>
    private InputAction _pointAction;

    /// <summary>
    /// Кэш экшена нажатия.
    /// </summary>
    private InputAction _pressAction;

    /// <summary>
    /// Выбор объекта лучом.
    /// </summary>
    private GrabTargetSelector _selector;

    /// <summary>
    /// Сессия физического перетаскивания.
    /// </summary>
    private GrabbedRigidbodyDragSession _session;

    /// <summary>
    /// Создаёт селектор и сессию.
    /// </summary>
    private void Awake()
    {
        _selector = new GrabTargetSelector(_config);
        _session = new GrabbedRigidbodyDragSession(_config);
    }

    /// <summary>
    /// Подписка на ввод и включение экшенов.
    /// </summary>
    private void OnEnable()
    {
        _pointAction = _pointerPosition.action;
        _pressAction = _grabPress.action;

        _pressAction.started += OnPressStarted;
        _pressAction.canceled += OnPressCanceled;

        _pointAction.Enable();
        _pressAction.Enable();
    }

    /// <summary>
    /// Отписка и отключение экшенов.
    /// </summary>
    private void OnDisable()
    {
        _session.End(false);

        _pressAction.started -= OnPressStarted;
        _pressAction.canceled -= OnPressCanceled;
        _pressAction.Disable();
        _pointAction.Disable();

        _pressAction = null;
        _pointAction = null;
    }

    /// <summary>
    /// Физика удержания: пружина к точке курсора на плоскости, проверка срыва по расстоянию.
    /// </summary>
    private void FixedUpdate()
    {
        if (!_session.IsActive || !_pressAction.IsPressed())
            return;

        _session.PhysicsStep(_targetCamera, _pointAction.ReadValue<Vector2>());
    }

    /// <summary>
    /// Начало нажатия: попытка захвата.
    /// </summary>
    /// <param name="_">Контекст (не используется).</param>
    private void OnPressStarted(InputAction.CallbackContext _)
    {
        TryBeginGrab();
    }

    /// <summary>
    /// Отпускание кнопки: завершение с корректировкой скорости по конфигу.
    /// </summary>
    /// <param name="_">Контекст (не используется).</param>
    private void OnPressCanceled(InputAction.CallbackContext _)
    {
        _session.End(false);
    }

    /// <summary>
    /// Захват по лучу (блокировка над UI — отдельно, на уровне проекта).
    /// </summary>
    private void TryBeginGrab()
    {
        var screenPoint = _pointAction.ReadValue<Vector2>();

        if (!_selector.TrySelect(_targetCamera, screenPoint, out var ray, out _, out var grabbable))
            return;

        var surface = _dragSurfaces.Resolve(grabbable.DragSurfaceId);
        _session.Begin(grabbable, ray, surface);
    }
}
