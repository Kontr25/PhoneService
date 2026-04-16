using System;
using UnityEngine;

/// <summary>
/// Полная запись запчасти в базе.
/// </summary>
[Serializable]
public sealed class PartRecordEntry
{
    /// <summary>
    /// Уникальный id записи.
    /// </summary>
    [SerializeField]
    private string _recordId;

    /// <summary>
    /// Id категории запчасти.
    /// </summary>
    [SerializeField]
    private string _partCategoryId;

    /// <summary>
    /// Название телефона, для которого предназначена запчасть.
    /// </summary>
    [SerializeField]
    private string _phoneName;

    /// <summary>
    /// Модель телефона.
    /// </summary>
    [SerializeField]
    private string _phoneModelName;

    /// <summary>
    /// Качество/тип запчасти.
    /// </summary>
    [SerializeField]
    private PartQualityType _partQualityType = PartQualityType.Original;

    /// <summary>
    /// Стоимость запчасти.
    /// </summary>
    [SerializeField]
    private int _cost;

    /// <summary>
    /// Описание.
    /// </summary>
    [SerializeField]
    [TextArea]
    private string _description;

    /// <summary>
    /// Префаб запчасти.
    /// </summary>
    [SerializeField]
    private GameObject _partPrefab;

    /// <summary>
    /// Меш запчасти.
    /// </summary>
    [SerializeField]
    private Mesh _partMesh;

    /// <summary>
    /// Материал запчасти.
    /// </summary>
    [SerializeField]
    private Material _partMaterial;

    /// <summary>
    /// Id записи.
    /// </summary>
    public string RecordId => string.IsNullOrWhiteSpace(_recordId) ? string.Empty : _recordId.Trim();

    /// <summary>
    /// Id категории.
    /// </summary>
    public string PartCategoryId => string.IsNullOrWhiteSpace(_partCategoryId) ? string.Empty : _partCategoryId.Trim();

    /// <summary>
    /// Название телефона.
    /// </summary>
    public string PhoneName => string.IsNullOrWhiteSpace(_phoneName) ? string.Empty : _phoneName.Trim();

    /// <summary>
    /// Модель телефона.
    /// </summary>
    public string PhoneModelName => string.IsNullOrWhiteSpace(_phoneModelName) ? string.Empty : _phoneModelName.Trim();

    /// <summary>
    /// Качество запчасти.
    /// </summary>
    public PartQualityType PartQualityType => _partQualityType;

    /// <summary>
    /// Стоимость.
    /// </summary>
    public int Cost => _cost;

    /// <summary>
    /// Описание.
    /// </summary>
    public string Description => _description ?? string.Empty;

    /// <summary>
    /// Префаб.
    /// </summary>
    public GameObject PartPrefab => _partPrefab;

    /// <summary>
    /// Меш.
    /// </summary>
    public Mesh PartMesh => _partMesh;

    /// <summary>
    /// Материал.
    /// </summary>
    public Material PartMaterial => _partMaterial;

    /// <summary>
    /// Проверяет минимальную валидность записи.
    /// </summary>
    /// <returns>True, если запись валидна.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(_recordId)
               && !string.IsNullOrWhiteSpace(_partCategoryId)
               && !string.IsNullOrWhiteSpace(_phoneName)
               && !string.IsNullOrWhiteSpace(_phoneModelName)
               && _partPrefab != null
               && _partMesh != null
               && _partMaterial != null;
    }
}
