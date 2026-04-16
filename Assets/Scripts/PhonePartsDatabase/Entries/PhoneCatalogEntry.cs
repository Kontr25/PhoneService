using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Название телефона и список доступных моделей.
/// </summary>
[Serializable]
public sealed class PhoneCatalogEntry
{
    /// <summary>
    /// Название телефона (например, iPhone, Galaxy).
    /// </summary>
    [FormerlySerializedAs("_brandId")]
    [SerializeField]
    private string _phoneName;

    /// <summary>
    /// Подпись в UI.
    /// </summary>
    [SerializeField]
    private string _displayName;

    /// <summary>
    /// Устаревший список имён моделей (миграция из string[] _models).
    /// </summary>
    [FormerlySerializedAs("_models")]
    [SerializeField]
    private string[] _legacyModelNames = Array.Empty<string>();

    /// <summary>
    /// Модели телефона с именем, мешем и материалом.
    /// </summary>
    [SerializeField]
    private PhoneModelEntry[] _phoneModels = Array.Empty<PhoneModelEntry>();

    /// <summary>
    /// Название телефона.
    /// </summary>
    public string PhoneName => string.IsNullOrWhiteSpace(_phoneName) ? string.Empty : _phoneName.Trim();

    /// <summary>
    /// Имя для отображения.
    /// </summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(_displayName) ? PhoneName : _displayName.Trim();

    /// <summary>
    /// Модели (имена по порядку).
    /// </summary>
    public IReadOnlyList<string> Models
    {
        get
        {
            if (_phoneModels == null || _phoneModels.Length == 0)
                return Array.Empty<string>();

            var list = new List<string>(_phoneModels.Length);
            for (var i = 0; i < _phoneModels.Length; i++)
            {
                var entry = _phoneModels[i];
                if (entry == null)
                    continue;

                var name = entry.ModelName;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                list.Add(name);
            }

            return list;
        }
    }

    /// <summary>
    /// Записи моделей (только чтение).
    /// </summary>
    public IReadOnlyList<PhoneModelEntry> PhoneModels => _phoneModels ?? Array.Empty<PhoneModelEntry>();

    /// <summary>
    /// Переносит данные из legacy-массива строк в массив записей моделей.
    /// </summary>
    public void MigrateLegacyIfNeeded()
    {
        if (_phoneModels != null && _phoneModels.Length > 0)
        {
            _legacyModelNames = Array.Empty<string>();
            return;
        }

        if (_legacyModelNames == null || _legacyModelNames.Length == 0)
        {
            _phoneModels = Array.Empty<PhoneModelEntry>();
            return;
        }

        var list = new List<PhoneModelEntry>(_legacyModelNames.Length);
        for (var i = 0; i < _legacyModelNames.Length; i++)
        {
            var raw = _legacyModelNames[i];
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            list.Add(new PhoneModelEntry(raw.Trim()));
        }

        _phoneModels = list.Count > 0 ? list.ToArray() : Array.Empty<PhoneModelEntry>();
        _legacyModelNames = Array.Empty<string>();
    }

    /// <summary>
    /// Есть ли указанная модель у телефона.
    /// </summary>
    /// <param name="modelName">Название модели.</param>
    /// <returns>True, если модель найдена.</returns>
    public bool HasModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName) || _phoneModels == null)
            return false;

        var key = modelName.Trim();
        for (var i = 0; i < _phoneModels.Length; i++)
        {
            var entry = _phoneModels[i];
            if (entry == null)
                continue;

            if (string.Equals(entry.ModelName, key, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Уникальные непустые модели по порядку.
    /// </summary>
    /// <returns>Последовательность моделей.</returns>
    public IEnumerable<string> EnumerateModelsTrimmed()
    {
        if (_phoneModels == null)
            yield break;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < _phoneModels.Length; i++)
        {
            var entry = _phoneModels[i];
            if (entry == null)
                continue;

            var trimmed = entry.ModelName;
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            if (!seen.Add(trimmed))
                continue;

            yield return trimmed;
        }
    }
}
