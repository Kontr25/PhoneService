using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Корпус телефона: идентичность модели из базы и доступ к подсистемам (слоты, в будущем экран, разбор, пулы и т.д.).
/// </summary>
public sealed class PhoneController : MonoBehaviour, IPhoneModelIdentity
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
    /// Сервис слотов запчастей на этом корпусе.
    /// </summary>
    [SerializeField]
    private PhoneSlotService _slotService = null!;

    /// <inheritdoc />
    public string PhoneBrandId =>
        string.IsNullOrWhiteSpace(_phoneBrandId) ? string.Empty : _phoneBrandId.Trim();

    /// <inheritdoc />
    public string PhoneModelName =>
        string.IsNullOrWhiteSpace(_phoneModelName) ? string.Empty : _phoneModelName.Trim();

    /// <inheritdoc />
    public bool HasPhoneModelSpecified =>
        !string.IsNullOrWhiteSpace(_phoneBrandId) && !string.IsNullOrWhiteSpace(_phoneModelName);

    /// <summary>
    /// Слоты и установка запчастей.
    /// </summary>
    public IPhoneSlotService Slots => _slotService;

#if UNITY_EDITOR
    /// <summary>
    /// Элементы выпадающего списка брендов для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems() =>
        PhonePartsDatabaseDropdowns.BrandItems(includeAnyOption: false);

    /// <summary>
    /// Элементы выпадающего списка моделей для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorModelItems() =>
        PhonePartsDatabaseDropdowns.ModelItemsForBrand(_phoneBrandId, includeAnyOption: false);
#else
    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems()
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
