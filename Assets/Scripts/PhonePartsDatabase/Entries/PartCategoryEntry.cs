using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Категория запчасти, общая для всех телефонов (аккумулятор, камера, дисплей).
/// </summary>
[Serializable]
public sealed class PartCategoryEntry
{
    /// <summary>
    /// Стабильный id категории (battery, camera).
    /// </summary>
    [FormerlySerializedAs("_typeId")]
    [SerializeField]
    private string _categoryId;

    /// <summary>
    /// Подпись в UI.
    /// </summary>
    [SerializeField]
    private string _displayName;

    /// <summary>
    /// Иконка категории запчасти.
    /// </summary>
    [SerializeField]
    private Sprite _icon;

    /// <summary>
    /// Id категории.
    /// </summary>
    public string CategoryId => string.IsNullOrWhiteSpace(_categoryId) ? string.Empty : _categoryId.Trim();

    /// <summary>
    /// Имя для отображения.
    /// </summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(_displayName) ? CategoryId : _displayName.Trim();

    /// <summary>
    /// Иконка категории.
    /// </summary>
    public Sprite Icon => _icon;
}
