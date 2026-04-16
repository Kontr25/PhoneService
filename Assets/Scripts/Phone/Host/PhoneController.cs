using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Корпус телефона: идентичность модели из базы и доступ к подсистемам (слоты, в будущем экран, разбор, пулы и т.д.).
/// </summary>
public sealed class PhoneController : MonoBehaviour, IPhoneModelIdentity
{
    /// <summary>
    /// Название телефона из базы.
    /// </summary>
    [FormerlySerializedAs("_phoneBrandId")]
    [Tooltip("Название телефона из Phone Parts Database.")]
    [ValueDropdown(nameof(EditorPhoneNameItems), IsUniqueList = false, DropdownTitle = "Название телефона")]
    [SerializeField]
    private string _phoneName;

    /// <summary>
    /// Модель телефона в рамках бренда.
    /// </summary>
    [Tooltip("Модель для выбранного бренда.")]
    [ValueDropdown(nameof(EditorModelItems), IsUniqueList = false, DropdownTitle = "Модель")]
    [SerializeField]
    private string _phoneModelName;

    /// <summary>
    /// Сервис слотов запчастей на этом корпусе.
    /// </summary>
    [SerializeField]
    private PhoneSlotService _slotService = null!;

    /// <inheritdoc />
    public string PhoneName =>
        string.IsNullOrWhiteSpace(_phoneName) ? string.Empty : _phoneName.Trim();

    /// <inheritdoc />
    public string PhoneModelName =>
        string.IsNullOrWhiteSpace(_phoneModelName) ? string.Empty : _phoneModelName.Trim();

    /// <inheritdoc />
    public bool HasPhoneModelSpecified =>
        !string.IsNullOrWhiteSpace(_phoneName) && !string.IsNullOrWhiteSpace(_phoneModelName);

    /// <summary>
    /// Слоты и установка запчастей.
    /// </summary>
    public IPhoneSlotService Slots => _slotService;

#if UNITY_EDITOR
    /// <summary>
    /// Элементы выпадающего списка названий телефонов для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPhoneNameItems() =>
        PhonePartsDatabaseDropdowns.PhoneNameItems(includeAnyOption: false);

    /// <summary>
    /// Элементы выпадающего списка моделей для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorModelItems() =>
        PhonePartsDatabaseDropdowns.ModelItemsForPhone(_phoneName, includeAnyOption: false);
#else
    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPhoneNameItems()
    {
        yield break;
    }

    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorModelItems()
    {
        yield break;
    }
#endif

    /// <summary>
    /// Проверяет обязательную ссылку на сервис слотов.
    /// </summary>
    private void Awake()
    {
        if (_slotService == null)
            throw new System.InvalidOperationException();
    }

    /// <summary>
    /// Гасит превью слотов при отключении корпуса.
    /// </summary>
    private void OnDisable()
    {
        _slotService.HideAllInstallPreviews();
    }

    /// <summary>
    /// Синхронизирует учёт установленных деталей с иерархией сокетов.
    /// </summary>
    private void Start()
    {
        _slotService.ResyncInstalledPartsFromHierarchy();
    }
}
