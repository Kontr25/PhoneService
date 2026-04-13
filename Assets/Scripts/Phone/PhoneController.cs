using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Корпус телефона: <b>модель</b> (бренд + модель из базы), слоты с типом запчасти.
/// Установка: тип детали ↔ слот, модель детали ↔ модель этого телефона.
/// </summary>
public sealed class PhoneController : MonoBehaviour
{
    /// <summary>
    /// Бренд телефона (Brand Id из базы).
    /// </summary>
    [Tooltip("Бренд из Phone Parts Database.")]
    [ValueDropdown(nameof(EditorBrandItems), IsUniqueList = false, DropdownTitle = "Бренд")]
    [SerializeField]
    private string _phoneBrandId;

    /// <summary>
    /// Модель телефона в рамках бренда.
    /// </summary>
    [Tooltip("Модель для выбранного бренда.")]
    [ValueDropdown(nameof(EditorModelItems), IsUniqueList = false, DropdownTitle = "Модель")]
    [SerializeField]
    private string _phoneModelName;

    /// <summary>
    /// Слоты в порядке индекса для <see cref="TryInstall"/>.
    /// </summary>
    [SerializeField]
    private PhonePartSlot[] _slots = Array.Empty<PhonePartSlot>();

    /// <summary>
    /// Текущая деталь в слоте.
    /// </summary>
    private PhoneRepairPart[] _occupants;

    /// <summary>
    /// Brand Id этого телефона.
    /// </summary>
    public string PhoneBrandId =>
        string.IsNullOrWhiteSpace(_phoneBrandId) ? string.Empty : _phoneBrandId.Trim();

    /// <summary>
    /// Модель этого телефона.
    /// </summary>
    public string PhoneModelName =>
        string.IsNullOrWhiteSpace(_phoneModelName) ? string.Empty : _phoneModelName.Trim();

    /// <summary>
    /// Задана ли модель телефона (нужно для установки деталей).
    /// </summary>
    public bool HasPhoneModelSpecified =>
        !string.IsNullOrWhiteSpace(_phoneBrandId) && !string.IsNullOrWhiteSpace(_phoneModelName);

    /// <summary>
    /// Число слотов.
    /// </summary>
    public int SlotCount => _slots != null ? _slots.Length : 0;

    /// <summary>
    /// Деталь в слоте или null.
    /// </summary>
    public PhoneRepairPart GetOccupant(int slotIndex)
    {
        if (_occupants == null || slotIndex < 0 || slotIndex >= _occupants.Length)
            return null;

        return _occupants[slotIndex];
    }

#if UNITY_EDITOR
    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems() =>
        PhonePartsDatabaseDropdowns.BrandItems(includeAnyOption: false);

    private IEnumerable<ValueDropdownItem<string>> EditorModelItems() =>
        PhonePartsDatabaseDropdowns.ModelItemsForBrand(_phoneBrandId, includeAnyOption: false);
#else
    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems()
    {
        yield break;
    }

    private IEnumerable<ValueDropdownItem<string>> EditorModelItems()
    {
        yield break;
    }
#endif

    private void Awake()
    {
        RebuildOccupantsArray();
    }

    private void Start()
    {
        ResyncInstalledPartsFromHierarchy();
    }

    private void RebuildOccupantsArray()
    {
        var n = _slots != null ? _slots.Length : 0;
        _occupants = n > 0 ? new PhoneRepairPart[n] : Array.Empty<PhoneRepairPart>();
    }

    /// <summary>
    /// Деталь подходит под модель этого телефона.
    /// </summary>
    public bool PartMatchesPhoneModel(PhoneRepairPart part)
    {
        if (part == null || !HasPhoneModelSpecified || !part.HasModelSpecified)
            return false;

        return string.Equals(PhoneBrandId, part.PartBrandId, StringComparison.Ordinal)
               && string.Equals(PhoneModelName, part.PartModelName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Находит детали под сокетами и регистрирует их.
    /// </summary>
    public void ResyncInstalledPartsFromHierarchy()
    {
        if (_slots == null || _slots.Length == 0)
            return;

        if (_occupants == null || _occupants.Length != _slots.Length)
            RebuildOccupantsArray();

        for (var i = 0; i < _slots.Length; i++)
        {
            var socket = _slots[i].Socket;
            if (socket == null)
                continue;

            var part = socket.GetComponentInChildren<PhoneRepairPart>(true);
            if (part == null)
                continue;

            _occupants[i] = part;
            part.BindInstalled(this);

#if UNITY_EDITOR
            var slot = _slots[i];
            if (!slot.AcceptsPartType(part.PartTypeId))
                Debug.LogWarning(
                    $"{nameof(PhoneController)} '{name}': слот [{i}] — тип детали '{part.PartTypeId}' не совпадает с ожидаемым '{slot.AcceptedPartTypeId}'.",
                    socket);

            if (HasPhoneModelSpecified && part.HasModelSpecified && !PartMatchesPhoneModel(part))
                Debug.LogWarning(
                    $"{nameof(PhoneController)} '{name}': слот [{i}] — деталь для {part.PartBrandId}/{part.PartModelName}, телефон {PhoneBrandId}/{PhoneModelName}.",
                    socket);
#endif
        }
    }

    /// <summary>
    /// Слот освобождён.
    /// </summary>
    public void NotifyPartDetached(PhoneRepairPart part)
    {
        if (part == null || _occupants == null)
            return;

        for (var i = 0; i < _occupants.Length; i++)
        {
            if (_occupants[i] != part)
                continue;

            _occupants[i] = null;
            return;
        }
    }

    /// <summary>
    /// Устанавливает деталь: тип ↔ слот, модель детали ↔ модель телефона.
    /// </summary>
    public bool TryInstall(PhoneRepairPart part, int slotIndex)
    {
        if (part == null || _slots == null || _occupants == null)
            return false;

        if (slotIndex < 0 || slotIndex >= _slots.Length)
            return false;

        if (_occupants[slotIndex] != null)
            return false;

        var slot = _slots[slotIndex];
        if (slot.Socket == null)
            return false;

        if (!HasPhoneModelSpecified)
            return false;

        if (!part.HasModelSpecified)
            return false;

        if (!slot.AcceptsPartType(part.PartTypeId))
            return false;

        if (!PartMatchesPhoneModel(part))
            return false;

        for (var j = 0; j < _occupants.Length; j++)
        {
            if (_occupants[j] == part)
                _occupants[j] = null;
        }

        part.InstallIntoPhone(this, slot.Socket);
        _occupants[slotIndex] = part;
        return true;
    }
}
