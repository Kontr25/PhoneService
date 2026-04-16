using System;
using UnityEngine;
using Zenject;

/// <summary>
/// Реализация <see cref="IPhoneSlotService"/> на объекте телефона (рядом с <see cref="PhoneController"/>).
/// </summary>
[RequireComponent(typeof(PhoneController))]
[DisallowMultipleComponent]
public sealed class PhoneSlotService : MonoBehaviour, IPhoneSlotService
{
    /// <summary>
    /// Слоты по индексу (совпадают с <see cref="PhoneRepairSlotMarker.SlotIndex"/>).
    /// </summary>
    [Tooltip("Порядок = индекс слота на маркере.")]
    [SerializeField]
    private PhonePartInstallSlot[] _slots = Array.Empty<PhonePartInstallSlot>();

    /// <summary>
    /// Деталь в каждом слоте.
    /// </summary>
    private PhoneRepairPart[] _occupants = Array.Empty<PhoneRepairPart>();

    /// <summary>
    /// Корпус телефона (владелец идентичности модели).
    /// </summary>
    [SerializeField]
    private PhoneController _host = null!;

    /// <summary>
    /// Политика соответствия детали слоту.
    /// </summary>
    private IPhoneInstallFitEvaluator _fitEvaluator = null!;

    /// <summary>
    /// Внедряет оценщик соответствия.
    /// </summary>
    /// <param name="fitEvaluator">Оценщик.</param>
    [Inject]
    private void Construct(IPhoneInstallFitEvaluator fitEvaluator)
    {
        _fitEvaluator = fitEvaluator;
    }

    /// <summary>
    /// Кэширует владельца и массив занятости.
    /// </summary>
    private void Awake()
    {
        if (_host == null)
            throw new InvalidOperationException();

        RebuildOccupantsArray();
        ValidateSlotReferences();
    }

    /// <inheritdoc />
    public int SlotCount => _slots != null ? _slots.Length : 0;

    /// <inheritdoc />
    public PhoneRepairPart GetOccupant(int slotIndex)
    {
        if (_occupants == null || slotIndex < 0 || slotIndex >= _occupants.Length)
            return null;

        return _occupants[slotIndex];
    }

    /// <inheritdoc />
    public Transform GetSlotSocket(int slotIndex)
    {
        if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length)
            return null;

        return _slots[slotIndex].Socket;
    }

    /// <inheritdoc />
    public SlotInstallFit EvaluateSlotInstallFit(int slotIndex, PhoneRepairPart part)
    {
        if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length)
            return SlotInstallFit.None;

        return _fitEvaluator.Evaluate(_host, _slots[slotIndex], part);
    }

    /// <inheritdoc />
    public void SetInstallPreview(int slotIndex, bool visible, SlotInstallFit fit, PhoneRepairPart part)
    {
        if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length)
            return;

        _slots[slotIndex].SetInstallPreview(visible, fit, part);
    }

    /// <inheritdoc />
    public void HideAllInstallPreviews()
    {
        if (_slots == null)
            return;

        for (var i = 0; i < _slots.Length; i++)
            _slots[i].SetInstallPreview(false, SlotInstallFit.None, null);
    }

    /// <inheritdoc />
    public bool PartMatchesPhoneModel(PhoneRepairPart part)
    {
        if (part == null || !_host.HasPhoneModelSpecified || !part.HasModelSpecified)
            return false;

        return string.Equals(_host.PhoneName, part.PartPhoneName, StringComparison.Ordinal)
               && string.Equals(_host.PhoneModelName, part.PartModelName, StringComparison.Ordinal);
    }

    /// <inheritdoc />
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

            var part = _slots[i].GetInitialInstalledPart();
            if (part == null)
                continue;

            _occupants[i] = part;
            part.BindInstalled(_host);
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

        if (!_host.HasPhoneModelSpecified)
            return false;

        if (!part.HasModelSpecified)
            return false;

        if (!slot.AcceptsPartCategory(part.PartCategoryId))
            return false;

        for (var j = 0; j < _occupants.Length; j++)
        {
            if (_occupants[j] == part)
                _occupants[j] = null;
        }

        part.InstallIntoPhone(_host, slot.Socket);
        _occupants[slotIndex] = part;
        return true;
    }

    /// <summary>
    /// Пересоздаёт массив <see cref="_occupants"/> по числу слотов.
    /// </summary>
    private void RebuildOccupantsArray()
    {
        var n = _slots != null ? _slots.Length : 0;
        _occupants = n > 0 ? new PhoneRepairPart[n] : Array.Empty<PhoneRepairPart>();
    }

    /// <summary>
    /// Проверяет, что все элементы массива слотов назначены.
    /// </summary>
    private void ValidateSlotReferences()
    {
        if (_slots == null)
            return;

        for (var i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
                throw new InvalidOperationException();
        }
    }
}
