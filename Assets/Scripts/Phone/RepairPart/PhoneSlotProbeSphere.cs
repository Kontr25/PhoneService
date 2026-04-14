using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Невидимая сфера-триггер: позиция — точка под деталью; среди пересекающихся маркеров слотов выбирается ближайший свободный.
/// Пересчёт ближайшего по смещению центра сферы не чаще чем на <see cref="_nearestRecalcMoveDistance"/>; при входе/выходе слота из триггера пересчёт выполняется сразу.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public sealed class PhoneSlotProbeSphere : MonoBehaviour
{
    /// <summary>
    /// Триггер-коллайдер этой сферы.
    /// </summary>
    [SerializeField]
    private SphereCollider _sphereCollider;

    /// <summary>
    /// Тело сферы-пробы.
    /// </summary>
    [SerializeField]
    private Rigidbody _rigidbody;

    /// <summary>
    /// Минимальное смещение центра сферы (метры), после которого снова ищется ближайший слот среди пересечений.
    /// </summary>
    [SerializeField]
    private float _nearestRecalcMoveDistance = 0.1f;

    /// <summary>
    /// Маркеры слотов, сейчас пересекающиеся со сферой.
    /// </summary>
    private readonly List<PhoneRepairSlotMarker> _overlapping = new List<PhoneRepairSlotMarker>(16);

    /// <summary>
    /// Кэш: телефон ближайшего свободного слота.
    /// </summary>
    private PhoneController _pickedPhone;

    /// <summary>
    /// Кэш: индекс ближайшего свободного слота.
    /// </summary>
    private int _pickedSlot = -1;

    /// <summary>
    /// Позиция центра сферы при последнем пересчёте по порогу смещения.
    /// </summary>
    private Vector3 _lastRecalcPosition;

    /// <summary>
    /// Был ли уже задан якорь для порога смещения (после <see cref="ResetMoveThreshold"/> — сбрасывается).
    /// </summary>
    private bool _hasRecalcAnchor;

    /// <summary>
    /// Если true — при сбросе используются <see cref="_authoringDefaultWorldPosition"/> и <see cref="_authoringDefaultWorldEuler"/>; иначе поза покоя берётся из transform в <see cref="Awake"/>.
    /// </summary>
    [SerializeField]
    private bool _useAuthoringDefaultWorldPose;

    /// <summary>
    /// Мировая позиция сферы в покое (при <see cref="_useAuthoringDefaultWorldPose"/>).
    /// </summary>
    [SerializeField]
    private Vector3 _authoringDefaultWorldPosition;

    /// <summary>
    /// Мировые углы Эйлера сферы в покое (при <see cref="_useAuthoringDefaultWorldPose"/>).
    /// </summary>
    [SerializeField]
    private Vector3 _authoringDefaultWorldEuler;

    /// <summary>
    /// Запомненная мировая позиция возврата.
    /// </summary>
    private Vector3 _defaultWorldPosition;

    /// <summary>
    /// Запомненный мировой поворот возврата.
    /// </summary>
    private Quaternion _defaultWorldRotation;

    /// <summary>
    /// Коллайдер сферы-пробы.
    /// </summary>
    public SphereCollider ProbeCollider => _sphereCollider;

    /// <summary>
    /// Настраивает коллайдер, тело и запоминает позу покоя.
    /// </summary>
    private void Awake()
    {
        if (_sphereCollider == null || _rigidbody == null)
            throw new System.InvalidOperationException();

        _sphereCollider.isTrigger = true;
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;

        if (_useAuthoringDefaultWorldPose)
        {
            _defaultWorldPosition = _authoringDefaultWorldPosition;
            _defaultWorldRotation = Quaternion.Euler(_authoringDefaultWorldEuler);
        }
        else
        {
            _defaultWorldPosition = transform.position;
            _defaultWorldRotation = transform.rotation;
        }
    }

    /// <summary>
    /// Следующее перемещение через <see cref="MoveToWorldPoint"/> обязательно пересчитает ближайший слот по порогу.
    /// </summary>
    public void ResetMoveThreshold()
    {
        _hasRecalcAnchor = false;
    }

    /// <summary>
    /// Возвращает сферу в позу покоя (после отпускания детали, установки в слот и т.п.).
    /// </summary>
    public void ResetToDefaultWorldPose()
    {
        transform.SetPositionAndRotation(_defaultWorldPosition, _defaultWorldRotation);
        ResetMoveThreshold();
    }

    /// <summary>
    /// Переносит центр сферы в мировую точку (первое попадание луча вниз от детали).
    /// </summary>
    /// <param name="worldPoint">Точка в мире.</param>
    public void MoveToWorldPoint(Vector3 worldPoint)
    {
        transform.position = worldPoint;
        var threshold = _nearestRecalcMoveDistance;
        if (!_hasRecalcAnchor ||
            (worldPoint - _lastRecalcPosition).sqrMagnitude >= threshold * threshold)
        {
            _hasRecalcAnchor = true;
            _lastRecalcPosition = worldPoint;
            RecalculateNearestFreeSlot();
        }
    }

    /// <summary>
    /// Возвращает последний выбранный ближайший свободный слот.
    /// </summary>
    /// <param name="phone">Телефон или null.</param>
    /// <param name="slotIndex">Индекс слота или -1.</param>
    /// <returns>True, если есть кандидат.</returns>
    public bool TryGetPickedNearestFreeSlot(out PhoneController phone, out int slotIndex)
    {
        phone = _pickedPhone;
        slotIndex = _pickedSlot;
        return _pickedPhone != null && _pickedSlot >= 0;
    }

    /// <summary>
    /// Регистрирует маркер при входе в триггер сферы.
    /// </summary>
    /// <param name="other">Коллайдер другого объекта.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!PhoneRepairSlotMarker.TryResolveByCollider(other, out var marker) || _overlapping.Contains(marker))
            return;

        _overlapping.Add(marker);
        RecalculateNearestFreeSlot();
    }

    /// <summary>
    /// Убирает маркер при выходе из триггера сферы.
    /// </summary>
    /// <param name="other">Коллайдер другого объекта.</param>
    private void OnTriggerExit(Collider other)
    {
        if (!PhoneRepairSlotMarker.TryResolveByCollider(other, out var marker))
            return;

        _overlapping.Remove(marker);
        RecalculateNearestFreeSlot();
    }

    /// <summary>
    /// Выбирает ближайший к центру сферы свободный слот среди <see cref="_overlapping"/>.
    /// </summary>
    private void RecalculateNearestFreeSlot()
    {
        _pickedPhone = null;
        _pickedSlot = -1;
        var bestSq = float.MaxValue;
        var center = transform.position;

        for (var i = 0; i < _overlapping.Count; i++)
        {
            var marker = _overlapping[i];
            if (marker == null)
                continue;

            var phone = marker.Phone;
            if (phone == null)
                continue;

            var slotIndex = marker.SlotIndex;
            if (slotIndex < 0 || phone.Slots.GetOccupant(slotIndex) != null)
                continue;

            var d = (marker.transform.position - center).sqrMagnitude;
            if (d >= bestSq)
                continue;

            bestSq = d;
            _pickedPhone = phone;
            _pickedSlot = slotIndex;
        }
    }
}
