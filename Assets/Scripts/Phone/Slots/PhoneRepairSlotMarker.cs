using UnityEngine;

/// <summary>
/// Вешается на объект с триггер-коллайдером слота; сообщает сфере-пробу, какой слот <see cref="PhoneController"/> он представляет.
/// </summary>
public sealed class PhoneRepairSlotMarker : MonoBehaviour
{
    /// <summary>
    /// Связь триггер-коллайдера с маркером слота.
    /// </summary>
    private static readonly System.Collections.Generic.Dictionary<Collider, PhoneRepairSlotMarker> MarkerByCollider =
        new System.Collections.Generic.Dictionary<Collider, PhoneRepairSlotMarker>(64);

    /// <summary>
    /// Телефон-владелец слота.
    /// </summary>
    [SerializeField]
    private PhoneController _phone;

    /// <summary>
    /// Индекс слота в массиве <see cref="PhoneController"/> (тот же, что в инспекторе телефона).
    /// </summary>
    [SerializeField]
    private int _slotIndex;

    /// <summary>
    /// Коллайдер триггера этого маркера.
    /// </summary>
    [SerializeField]
    private Collider _triggerCollider;

    /// <summary>
    /// Телефон слота.
    /// </summary>
    public PhoneController Phone => _phone;

    /// <summary>
    /// Индекс слота.
    /// </summary>
    public int SlotIndex => _slotIndex;

    /// <summary>
    /// Регистрирует коллайдер маркера в кэше.
    /// </summary>
    private void OnEnable()
    {
        if (_triggerCollider == null)
            throw new System.InvalidOperationException();

        MarkerByCollider[_triggerCollider] = this;
    }

    /// <summary>
    /// Удаляет коллайдер маркера из кэша.
    /// </summary>
    private void OnDisable()
    {
        if (_triggerCollider != null)
            MarkerByCollider.Remove(_triggerCollider);
    }

    /// <summary>
    /// Возвращает маркер по коллайдеру триггера.
    /// </summary>
    /// <param name="collider">Коллайдер из события физики.</param>
    /// <param name="marker">Найденный маркер.</param>
    /// <returns>True, если маркер найден.</returns>
    public static bool TryResolveByCollider(Collider collider, out PhoneRepairSlotMarker marker)
    {
        return MarkerByCollider.TryGetValue(collider, out marker);
    }
}
