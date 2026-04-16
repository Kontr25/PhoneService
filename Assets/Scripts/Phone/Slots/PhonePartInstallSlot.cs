using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

/// <summary>
/// Слот на сцене: сокет, фильтр категории запчасти, превью меша и материала; учёт слотов — в <see cref="PhoneSlotService"/>.
/// </summary>
[DisallowMultipleComponent]
public sealed class PhonePartInstallSlot : MonoBehaviour, IPhoneRepairSlot
{
    /// <summary>
    /// Родитель установленной детали; если null — используется transform этого объекта.
    /// </summary>
    [SerializeField]
    private Transform _socket;

    /// <summary>
    /// Ожидаемый Category Id запчасти; пустая строка — любая категория.
    /// </summary>
    [FormerlySerializedAs("_acceptedPartTypeId")]
    [Tooltip("Категория запчасти из базы (battery, camera…). Пусто — любая категория.")]
    [ValueDropdown(nameof(EditorPartCategoryItems), IsUniqueList = false, DropdownTitle = "Категория запчасти")]
    [SerializeField]
    private string _acceptedPartCategoryId;

    /// <summary>
    /// Фильтр меша объекта превью установки.
    /// </summary>
    [Tooltip("MeshFilter объекта превью (назначьте вручную).")]
    [SerializeField]
    private MeshFilter _installPreviewMeshFilter;

    /// <summary>
    /// Рендерер объекта превью установки.
    /// </summary>
    [SerializeField]
    private MeshRenderer _installPreviewRenderer;

    /// <summary>
    /// Деталь, изначально установленная в сокет этого слота (для Resync без поиска компонентов).
    /// </summary>
    [SerializeField]
    private PhoneRepairPart _initialInstalledPart;

    /// <summary>
    /// Источник материалов превью (Zenject).
    /// </summary>
    private IInstallPreviewMaterialSource _materials = null!;

    /// <summary>
    /// Внедряет источник материалов превью.
    /// </summary>
    /// <param name="materials">Источник материалов.</param>
    [Inject]
    private void Construct(IInstallPreviewMaterialSource materials)
    {
        _materials = materials;
    }

    /// <summary>
    /// Нормализует сокет и проверяет превью.
    /// </summary>
    private void Awake()
    {
        if (_socket == null)
            _socket = transform;

        ValidatePreviewConfiguration();
    }

    /// <summary>
    /// Проверяет согласованность полей превью.
    /// </summary>
    private void ValidatePreviewConfiguration()
    {
        if (_installPreviewRenderer == null)
            return;

        if (_installPreviewMeshFilter == null)
            throw new InvalidOperationException();
    }

    /// <inheritdoc />
    public Transform Socket => _socket != null ? _socket : transform;

    /// <inheritdoc />
    public bool AcceptsPartCategory(string partCategoryId)
    {
        var req = string.IsNullOrWhiteSpace(_acceptedPartCategoryId) ? string.Empty : _acceptedPartCategoryId.Trim();
        if (string.IsNullOrEmpty(req))
            return true;

        if (string.IsNullOrWhiteSpace(partCategoryId))
            return false;

        return string.Equals(req, partCategoryId.Trim(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Возвращает изначально привязанную деталь для синхронизации.
    /// </summary>
    /// <returns>Деталь или null.</returns>
    public PhoneRepairPart GetInitialInstalledPart()
    {
        return _initialInstalledPart;
    }

    /// <inheritdoc />
    public void SetInstallPreview(bool visible, SlotInstallFit fit, PhoneRepairPart part)
    {
        if (_installPreviewRenderer == null)
            return;

        _installPreviewRenderer.gameObject.SetActive(visible);
        if (!visible)
            return;

        if (part != null && _installPreviewMeshFilter != null)
        {
            var mesh = part.GetInstallPreviewMesh();
            if (mesh != null)
                _installPreviewMeshFilter.sharedMesh = mesh;
        }

        var mat = _materials.GetMaterialForFit(fit);
        if (mat == null)
            return;

        var shared = _installPreviewRenderer.sharedMaterials;
        if (shared != null && shared.Length > 0)
        {
            shared[0] = mat;
            _installPreviewRenderer.sharedMaterials = shared;
        }
        else
            _installPreviewRenderer.sharedMaterial = mat;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Элементы выпадающего списка категорий запчастей для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPartCategoryItems() =>
        PhonePartsDatabaseDropdowns.PartCategoryItems(includeAnyOption: true);
#else
    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPartCategoryItems()
    {
        yield break;
    }
#endif
}
