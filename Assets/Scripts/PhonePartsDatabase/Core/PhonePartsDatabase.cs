using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// База: бренды с моделями телефонов и отдельный список типов запчастей.
/// У телефона задаётся модель; у слота — тип запчасти; у детали — тип + модель (бренд + модель), с чем сравнивается корпус.
/// </summary>
[CreateAssetMenu(fileName = "PhonePartsDatabase", menuName = "PhoneService/Parts Database/Phone Parts Database")]
public sealed class PhonePartsDatabase : ScriptableObject
{
    /// <summary>
    /// Имя ресурса в Resources.
    /// </summary>
    public const string ResourcesAssetName = "PhonePartsDatabase";

    /// <summary>
    /// Бренды и их модели.
    /// </summary>
    [SerializeField]
    private PhoneBrandEntry[] _brands = Array.Empty<PhoneBrandEntry>();

    /// <summary>
    /// Типы запчастей (для всех телефонов).
    /// </summary>
    [SerializeField]
    private PhonePartTypeEntry[] _partTypes = Array.Empty<PhonePartTypeEntry>();

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
    /// Бренды (только чтение).
    /// </summary>
    public IReadOnlyList<PhoneBrandEntry> Brands => _brands;

    /// <summary>
    /// Типы запчастей (только чтение).
    /// </summary>
    public IReadOnlyList<PhonePartTypeEntry> PartTypes => _partTypes;

    /// <summary>
    /// Материал превью «подходит».
    /// </summary>
    public Material ValidSlotPreviewMaterial => _validSlotPreviewMaterial;

    /// <summary>
    /// Материал превью «не подходит».
    /// </summary>
    public Material InvalidSlotPreviewMaterial => _invalidSlotPreviewMaterial;

    /// <summary>
    /// Id брендов по порядку.
    /// </summary>
    public IEnumerable<string> EnumerateBrandIds()
    {
        if (_brands == null)
            yield break;

        for (var i = 0; i < _brands.Length; i++)
        {
            var id = _brands[i] != null ? _brands[i].BrandId : string.Empty;
            if (string.IsNullOrEmpty(id))
                continue;

            yield return id;
        }
    }

    /// <summary>
    /// Модели для бренда.
    /// </summary>
    public IEnumerable<string> EnumerateModels(string brandId)
    {
        var b = FindBrand(brandId);
        if (b == null)
            yield break;

        foreach (var m in b.EnumerateModelsTrimmed())
            yield return m;
    }

    /// <summary>
    /// Id типов запчастей.
    /// </summary>
    public IEnumerable<string> EnumeratePartTypeIds()
    {
        if (_partTypes == null)
            yield break;

        for (var i = 0; i < _partTypes.Length; i++)
        {
            var id = _partTypes[i] != null ? _partTypes[i].TypeId : string.Empty;
            if (string.IsNullOrEmpty(id))
                continue;

            yield return id;
        }
    }

    /// <summary>
    /// Тип запчасти существует в базе.
    /// </summary>
    public bool ContainsPartType(string typeId)
    {
        return FindPartType(typeId) != null;
    }

    /// <summary>
    /// Модель у бренда есть в базе.
    /// </summary>
    public bool ContainsPhoneModel(string brandId, string modelName)
    {
        var b = FindBrand(brandId);
        return b != null && b.HasModel(modelName);
    }

    /// <summary>
    /// Иконка типа запчасти.
    /// </summary>
    public bool TryGetPartTypeIcon(string typeId, out Sprite icon)
    {
        icon = null;
        var t = FindPartType(typeId);
        if (t == null)
            return false;

        icon = t.Icon;
        return icon != null;
    }

    /// <summary>
    /// Отображаемое имя типа запчасти.
    /// </summary>
    public bool TryGetPartTypeDisplayName(string typeId, out string displayName)
    {
        displayName = null;
        var t = FindPartType(typeId);
        if (t == null)
            return false;

        displayName = t.DisplayName;
        return true;
    }

    /// <summary>
    /// Деталь согласована с базой: тип и пара бренд+модель валидны.
    /// </summary>
    public bool IsValidPart(string partTypeId, string brandId, string modelName)
    {
        if (string.IsNullOrWhiteSpace(partTypeId) || string.IsNullOrWhiteSpace(brandId) ||
            string.IsNullOrWhiteSpace(modelName))
            return false;

        return ContainsPartType(partTypeId) && ContainsPhoneModel(brandId, modelName);
    }

    private PhoneBrandEntry FindBrand(string brandId)
    {
        if (string.IsNullOrWhiteSpace(brandId) || _brands == null)
            return null;

        var key = brandId.Trim();
        for (var i = 0; i < _brands.Length; i++)
        {
            var b = _brands[i];
            if (b == null)
                continue;

            if (string.Equals(b.BrandId, key, StringComparison.Ordinal))
                return b;
        }

        return null;
    }

    private PhonePartTypeEntry FindPartType(string typeId)
    {
        if (string.IsNullOrWhiteSpace(typeId) || _partTypes == null)
            return null;

        var key = typeId.Trim();
        for (var i = 0; i < _partTypes.Length; i++)
        {
            var t = _partTypes[i];
            if (t == null)
                continue;

            if (string.Equals(t.TypeId, key, StringComparison.Ordinal))
                return t;
        }

        return null;
    }

}
