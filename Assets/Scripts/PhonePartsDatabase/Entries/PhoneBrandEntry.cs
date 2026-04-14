using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Бренд телефона и список моделей этого бренда (в базе <see cref="PhonePartsDatabase"/>).
/// </summary>
[Serializable]
public sealed class PhoneBrandEntry
{
    /// <summary>
    /// Стабильный id бренда (apple, samsung…).
    /// </summary>
    [Tooltip("Ключ для сейвов и сравнения (латиница без пробелов).")]
    [SerializeField]
    private string _brandId;

    /// <summary>
    /// Подпись в UI.
    /// </summary>
    [SerializeField]
    private string _displayName;

    /// <summary>
    /// Названия моделей (iPhone15, iPhone14…).
    /// </summary>
    [SerializeField]
    private string[] _models = Array.Empty<string>();

    /// <inheritdoc cref="_brandId"/>
    public string BrandId => string.IsNullOrWhiteSpace(_brandId) ? string.Empty : _brandId.Trim();

    /// <summary>
    /// Имя для выпадашек.
    /// </summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(_displayName) ? BrandId : _displayName.Trim();

    /// <summary>
    /// Сырые модели (как в инспекторе).
    /// </summary>
    public IReadOnlyList<string> Models => _models ?? Array.Empty<string>();

    /// <summary>
    /// Есть ли модель у бренда (после trim, строгое сравнение).
    /// </summary>
    public bool HasModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName) || _models == null)
            return false;

        var key = modelName.Trim();
        for (var i = 0; i < _models.Length; i++)
        {
            var m = _models[i];
            if (string.IsNullOrWhiteSpace(m))
                continue;

            if (string.Equals(m.Trim(), key, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Уникальные непустые модели по порядку.
    /// </summary>
    public IEnumerable<string> EnumerateModelsTrimmed()
    {
        if (_models == null)
            yield break;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < _models.Length; i++)
        {
            var m = _models[i];
            if (string.IsNullOrWhiteSpace(m))
                continue;

            var t = m.Trim();
            if (!seen.Add(t))
                continue;

            yield return t;
        }
    }
}
