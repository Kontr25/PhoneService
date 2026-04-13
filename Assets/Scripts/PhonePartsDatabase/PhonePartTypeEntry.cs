using System;
using UnityEngine;

/// <summary>
/// Тип запчасти, общий для всех телефонов (аккумулятор, камера, дисплей…).
/// </summary>
[Serializable]
public sealed class PhonePartTypeEntry
{
    /// <summary>
    /// Стабильный id типа (battery, camera…).
    /// </summary>
    [Tooltip("Ключ; совпадает с полем на детали и в слоте.")]
    [SerializeField]
    private string _typeId;

    /// <summary>
    /// Подпись в UI.
    /// </summary>
    [SerializeField]
    private string _displayName;

    /// <summary>
    /// Иконка типа запчасти.
    /// </summary>
    [SerializeField]
    private Sprite _icon;

    /// <inheritdoc cref="_typeId"/>
    public string TypeId => string.IsNullOrWhiteSpace(_typeId) ? string.Empty : _typeId.Trim();

    /// <summary>
    /// Имя для выпадашек.
    /// </summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(_displayName) ? TypeId : _displayName.Trim();

    /// <summary>
    /// Иконка.
    /// </summary>
    public Sprite Icon => _icon;
}
