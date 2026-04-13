using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Слот телефона: сокет и ожидаемый <b>тип запчасти</b> (модель задаётся на <see cref="PhoneController"/>).
/// Пустой тип — принимает любую запчасть по типу (проверка только модели детали и телефона).
/// </summary>
[System.Serializable]
public sealed class PhonePartSlot
{
    /// <summary>
    /// Родитель для установленной детали.
    /// </summary>
    [SerializeField]
    private Transform _socket;

    /// <summary>
    /// Type Id из базы; пусто — любой тип (остаётся проверка «деталь для этой модели телефона»).
    /// </summary>
    [Tooltip("Тип запчасти из базы (battery, camera…). Пусто — любой тип.")]
    [ValueDropdown(nameof(EditorPartTypeItems), IsUniqueList = false, DropdownTitle = "Тип запчасти")]
    [SerializeField]
    private string _acceptedPartTypeId;

    /// <summary>
    /// Точка крепления.
    /// </summary>
    public Transform Socket => _socket;

    /// <summary>
    /// Ожидаемый тип запчасти или пусто.
    /// </summary>
    public string AcceptedPartTypeId =>
        string.IsNullOrWhiteSpace(_acceptedPartTypeId) ? string.Empty : _acceptedPartTypeId.Trim();

    /// <summary>
    /// Слот принимает тип детали (пустой accepted = любой).
    /// </summary>
    public bool AcceptsPartType(string partTypeId)
    {
        var req = AcceptedPartTypeId;
        if (string.IsNullOrEmpty(req))
            return true;

        if (string.IsNullOrWhiteSpace(partTypeId))
            return false;

        return string.Equals(req, partTypeId.Trim(), System.StringComparison.Ordinal);
    }

#if UNITY_EDITOR
    private IEnumerable<ValueDropdownItem<string>> EditorPartTypeItems() =>
        PhonePartsDatabaseDropdowns.PartTypeItems(includeAnyOption: true);
#else
    private IEnumerable<ValueDropdownItem<string>> EditorPartTypeItems()
    {
        yield break;
    }
#endif
}
