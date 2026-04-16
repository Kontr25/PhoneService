using System;
using UnityEngine;

/// <summary>
/// Модель телефона в каталоге: имя и визуальные ссылки для превью/геймплея.
/// </summary>
[Serializable]
public sealed class PhoneModelEntry
{
    /// <summary>
    /// Внутреннее имя модели (ключ совпадает с полем модели в записях запчастей).
    /// </summary>
    [SerializeField]
    private string _modelName;

    /// <summary>
    /// Ссылка на меш для визуализации этой модели телефона.
    /// </summary>
    [SerializeField]
    private Mesh _mesh;

    /// <summary>
    /// Ссылка на материал для меша этой модели телефона.
    /// </summary>
    [SerializeField]
    private Material _material;

    /// <summary>
    /// Создаёт пустую запись модели (для сериализации Unity).
    /// </summary>
    public PhoneModelEntry()
    {
    }

    /// <summary>
    /// Создаёт запись модели с именем (миграция из legacy-массива строк).
    /// </summary>
    /// <param name="modelName">Имя модели.</param>
    public PhoneModelEntry(string modelName)
    {
        _modelName = modelName ?? string.Empty;
    }

    /// <summary>
    /// Название модели.
    /// </summary>
    public string ModelName => string.IsNullOrWhiteSpace(_modelName) ? string.Empty : _modelName.Trim();

    /// <summary>
    /// Меш модели телефона.
    /// </summary>
    public Mesh Mesh => _mesh;

    /// <summary>
    /// Материал модели телефона.
    /// </summary>
    public Material Material => _material;
}
