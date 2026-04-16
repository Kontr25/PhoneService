using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Выпадающие списки Odin (только редактор). В билде методы возвращают пустую последовательность.
/// </summary>
public static class PhonePartsDatabaseDropdowns
{
    /// <summary>
    /// Названия телефонов.
    /// </summary>
    public static IEnumerable<ValueDropdownItem<string>> PhoneNameItems(bool includeAnyOption)
    {
#if UNITY_EDITOR
        if (includeAnyOption)
            yield return new ValueDropdownItem<string>("(любой)", string.Empty);

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null)
            yield break;

        var list = db.PhoneCatalog;
        for (var i = 0; i < list.Count; i++)
        {
            var b = list[i];
            if (b == null)
                continue;

            var phoneName = b.PhoneName;
            if (string.IsNullOrEmpty(phoneName))
                continue;

            yield return new ValueDropdownItem<string>(b.DisplayName, phoneName);
        }
#else
        yield break;
#endif
    }

    /// <summary>
    /// Модели выбранного названия телефона.
    /// </summary>
    public static IEnumerable<ValueDropdownItem<string>> ModelItemsForPhone(string phoneName, bool includeAnyOption)
    {
#if UNITY_EDITOR
        if (includeAnyOption)
            yield return new ValueDropdownItem<string>("(любая)", string.Empty);

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null || string.IsNullOrWhiteSpace(phoneName))
            yield break;

        foreach (var m in db.EnumerateModels(phoneName))
            yield return new ValueDropdownItem<string>(m, m);
#else
        yield break;
#endif
    }

    /// <summary>
    /// Категории запчастей.
    /// </summary>
    public static IEnumerable<ValueDropdownItem<string>> PartCategoryItems(bool includeAnyOption)
    {
#if UNITY_EDITOR
        if (includeAnyOption)
            yield return new ValueDropdownItem<string>("(любой)", string.Empty);

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null)
            yield break;

        var list = db.PartCategories;
        for (var i = 0; i < list.Count; i++)
        {
            var t = list[i];
            if (t == null)
                continue;

            var id = t.CategoryId;
            if (string.IsNullOrEmpty(id))
                continue;

            yield return new ValueDropdownItem<string>(t.DisplayName, id);
        }
#else
        yield break;
#endif
    }

    /// <summary>
    /// Записи запчастей.
    /// </summary>
    public static IEnumerable<ValueDropdownItem<string>> PartRecordItems(bool includeAnyOption)
    {
#if UNITY_EDITOR
        if (includeAnyOption)
            yield return new ValueDropdownItem<string>("(не выбрано)", string.Empty);

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null)
            yield break;

        var list = db.PartRecords;
        for (var i = 0; i < list.Count; i++)
        {
            var record = list[i];
            if (record == null || string.IsNullOrEmpty(record.RecordId))
                continue;

            var label = $"{record.RecordId} | {record.PhoneName} {record.PhoneModelName} | {record.PartCategoryId} | {record.PartQualityType}";
            yield return new ValueDropdownItem<string>(label, record.RecordId);
        }
#else
        yield break;
#endif
    }
}
