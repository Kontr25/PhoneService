using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// База: названия телефонов с моделями, категории запчастей и записи запчастей.
/// </summary>
[CreateAssetMenu(fileName = "PhonePartsDatabase", menuName = "PhoneService/Parts Database/Phone Parts Database")]
public sealed class PhonePartsDatabase : ScriptableObject
{
    /// <summary>
    /// Имя ресурса в Resources.
    /// </summary>
    public const string ResourcesAssetName = "PhonePartsDatabase";

    /// <summary>
    /// Названия телефонов и их модели.
    /// </summary>
    [FormerlySerializedAs("_brands")]
    [SerializeField]
    private PhoneCatalogEntry[] _phoneCatalog = Array.Empty<PhoneCatalogEntry>();

    /// <summary>
    /// Категории запчастей (для всех телефонов).
    /// </summary>
    [FormerlySerializedAs("_partTypes")]
    [SerializeField]
    private PartCategoryEntry[] _partCategories = Array.Empty<PartCategoryEntry>();

    /// <summary>
    /// Полные записи запчастей.
    /// </summary>
    [SerializeField]
    private PartRecordEntry[] _partRecords = Array.Empty<PartRecordEntry>();

    /// <summary>
    /// Материал превью установки: деталь подходит слоту (тип и модель).
    /// </summary>
    [SerializeField]
    private Material _validSlotPreviewMaterial;

    /// <summary>
    /// Материал превью установки: неверный тип или другая модель телефона.
    /// </summary>
    [SerializeField]
    private Material _invalidSlotPreviewMaterial;

    /// <summary>
    /// Телефоны (только чтение).
    /// </summary>
    public IReadOnlyList<PhoneCatalogEntry> PhoneCatalog => _phoneCatalog;

    /// <summary>
    /// Категории запчастей (только чтение).
    /// </summary>
    public IReadOnlyList<PartCategoryEntry> PartCategories => _partCategories;

    /// <summary>
    /// Записи запчастей (только чтение).
    /// </summary>
    public IReadOnlyList<PartRecordEntry> PartRecords => _partRecords;

    /// <summary>
    /// Материал превью «подходит».
    /// </summary>
    public Material ValidSlotPreviewMaterial => _validSlotPreviewMaterial;

    /// <summary>
    /// Материал превью «не подходит».
    /// </summary>
    public Material InvalidSlotPreviewMaterial => _invalidSlotPreviewMaterial;

    /// <summary>
    /// В редакторе переносит устаревшие строковые списки моделей в записи <see cref="PhoneModelEntry"/>.
    /// </summary>
    private void OnValidate()
    {
        if (_phoneCatalog == null)
            return;

        for (var i = 0; i < _phoneCatalog.Length; i++)
            _phoneCatalog[i]?.MigrateLegacyIfNeeded();
    }

    /// <summary>
    /// Модели для названия телефона.
    /// </summary>
    public IEnumerable<string> EnumerateModels(string phoneName)
    {
        var phone = FindPhone(phoneName);
        if (phone == null)
            yield break;

        foreach (var model in phone.EnumerateModelsTrimmed())
            yield return model;
    }

    /// <summary>
    /// Категория запчасти существует в базе.
    /// </summary>
    public bool ContainsPartCategory(string categoryId)
    {
        return FindPartCategory(categoryId) != null;
    }

    /// <summary>
    /// Модель у названия телефона есть в базе.
    /// </summary>
    public bool ContainsPhoneModel(string phoneName, string modelName)
    {
        var phone = FindPhone(phoneName);
        return phone != null && phone.HasModel(modelName);
    }

    /// <summary>
    /// Ищет запись телефона по названию.
    /// </summary>
    /// <param name="phoneName">Название телефона.</param>
    /// <returns>Запись телефона или null.</returns>
    private PhoneCatalogEntry FindPhone(string phoneName)
    {
        if (string.IsNullOrWhiteSpace(phoneName) || _phoneCatalog == null)
            return null;

        var key = phoneName.Trim();
        for (var i = 0; i < _phoneCatalog.Length; i++)
        {
            var phone = _phoneCatalog[i];
            if (phone == null)
                continue;

            if (string.Equals(phone.PhoneName, key, StringComparison.Ordinal))
                return phone;
        }

        return null;
    }

    /// <summary>
    /// Ищет категорию запчасти по id.
    /// </summary>
    /// <param name="categoryId">Id категории.</param>
    /// <returns>Категория или null.</returns>
    private PartCategoryEntry FindPartCategory(string categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId) || _partCategories == null)
            return null;

        var key = categoryId.Trim();
        for (var i = 0; i < _partCategories.Length; i++)
        {
            var category = _partCategories[i];
            if (category == null)
                continue;

            if (string.Equals(category.CategoryId, key, StringComparison.Ordinal))
                return category;
        }

        return null;
    }

    /// <summary>
    /// Ищет запись запчасти по id.
    /// </summary>
    /// <param name="recordId">Id записи.</param>
    /// <param name="partRecord">Найденная запись.</param>
    /// <returns>True, если запись найдена.</returns>
    public bool TryGetPartRecord(string recordId, out PartRecordEntry partRecord)
    {
        partRecord = null;
        if (string.IsNullOrWhiteSpace(recordId) || _partRecords == null)
            return false;

        var key = recordId.Trim();
        for (var i = 0; i < _partRecords.Length; i++)
        {
            var record = _partRecords[i];
            if (record == null)
                continue;

            if (!string.Equals(record.RecordId, key, StringComparison.Ordinal))
                continue;

            partRecord = record;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Проверяет консистентность записи запчасти относительно справочников.
    /// </summary>
    /// <param name="record">Запись для проверки.</param>
    /// <returns>True, если запись валидна.</returns>
    public bool IsValidPartRecord(PartRecordEntry record)
    {
        if (record == null || !record.IsValid())
            return false;

        return ContainsPartCategory(record.PartCategoryId)
               && ContainsPhoneModel(record.PhoneName, record.PhoneModelName);
    }
}
