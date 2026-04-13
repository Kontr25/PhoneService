using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Запчасть: тип из общего списка + модель телефона (бренд + модель), для какой она предназначена.
/// </summary>
public sealed class PhoneRepairPart : Grabbable, IGrabLifecycle
{
    /// <summary>
    /// Type Id (battery, camera…).
    /// </summary>
    [Tooltip("Тип запчасти из базы.")]
    [ValueDropdown(nameof(EditorPartTypeItems), IsUniqueList = false, DropdownTitle = "Тип запчасти")]
    [SerializeField]
    private string _partTypeId;

    /// <summary>
    /// Бренд телефона, под который деталь.
    /// </summary>
    [Tooltip("Бренд из базы.")]
    [ValueDropdown(nameof(EditorBrandItems), IsUniqueList = false, DropdownTitle = "Бренд")]
    [SerializeField]
    private string _partBrandId;

    /// <summary>
    /// Модель телефона.
    /// </summary>
    [Tooltip("Модель для выбранного бренда.")]
    [ValueDropdown(nameof(EditorModelItems), IsUniqueList = false, DropdownTitle = "Модель")]
    [SerializeField]
    private string _partModelName;

    private PhoneController _installedIn;

    /// <summary>
    /// Type Id детали.
    /// </summary>
    public string PartTypeId =>
        string.IsNullOrWhiteSpace(_partTypeId) ? string.Empty : _partTypeId.Trim();

    /// <summary>
    /// Brand Id детали.
    /// </summary>
    public string PartBrandId =>
        string.IsNullOrWhiteSpace(_partBrandId) ? string.Empty : _partBrandId.Trim();

    /// <summary>
    /// Модель детали.
    /// </summary>
    public string PartModelName =>
        string.IsNullOrWhiteSpace(_partModelName) ? string.Empty : _partModelName.Trim();

    /// <summary>
    /// Заданы бренд и модель.
    /// </summary>
    public bool HasModelSpecified =>
        !string.IsNullOrWhiteSpace(_partBrandId) && !string.IsNullOrWhiteSpace(_partModelName);

    /// <summary>
    /// Телефон, куда установлена деталь.
    /// </summary>
    public PhoneController InstalledIn => _installedIn;

    /// <inheritdoc />
    public void OnGrabSessionStarting(in Ray pickRay)
    {
        if (_installedIn == null)
            return;

        _installedIn.NotifyPartDetached(this);
        transform.SetParent(null, true);
        _installedIn = null;
    }

    /// <inheritdoc />
    public void OnGrabSessionEnded(bool preserveVelocities)
    {
    }

    internal void BindInstalled(PhoneController phone)
    {
        _installedIn = phone;
    }

    internal void InstallIntoPhone(PhoneController phone, Transform socket)
    {
        _installedIn = phone;
        transform.SetParent(socket, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        var rb = PhysicsBody;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

#if UNITY_EDITOR
    private IEnumerable<ValueDropdownItem<string>> EditorPartTypeItems() =>
        PhonePartsDatabaseDropdowns.PartTypeItems(includeAnyOption: false);

    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems() =>
        PhonePartsDatabaseDropdowns.BrandItems(includeAnyOption: false);

    private IEnumerable<ValueDropdownItem<string>> EditorModelItems() =>
        PhonePartsDatabaseDropdowns.ModelItemsForBrand(_partBrandId, includeAnyOption: false);

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(_partTypeId))
        {
            Debug.LogWarning($"{nameof(PhoneRepairPart)} '{name}': укажите тип запчасти.", this);
            return;
        }

        if (!HasModelSpecified)
        {
            Debug.LogWarning($"{nameof(PhoneRepairPart)} '{name}': укажите бренд и модель телефона.", this);
            return;
        }

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null)
            return;

        if (!db.IsValidPart(PartTypeId, PartBrandId, PartModelName))
            Debug.LogWarning(
                $"{nameof(PhoneRepairPart)} '{name}': комбинация типа / бренда / модели не найдена в {nameof(PhonePartsDatabase)}.",
                this);
    }
#else
    private IEnumerable<ValueDropdownItem<string>> EditorPartTypeItems()
    {
        yield break;
    }

    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems()
    {
        yield break;
    }

    private IEnumerable<ValueDropdownItem<string>> EditorModelItems()
    {
        yield break;
    }
#endif
}
