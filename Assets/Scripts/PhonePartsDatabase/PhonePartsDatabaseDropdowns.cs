using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Выпадающие списки Odin (только редактор). В билде методы возвращают пустую последовательность.
/// </summary>
public static class PhonePartsDatabaseDropdowns
{
    /// <summary>
    /// Бренды телефонов.
    /// </summary>
    public static IEnumerable<ValueDropdownItem<string>> BrandItems(bool includeAnyOption)
    {
#if UNITY_EDITOR
        if (includeAnyOption)
            yield return new ValueDropdownItem<string>("(любой)", string.Empty);

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null)
            yield break;

        var list = db.Brands;
        for (var i = 0; i < list.Count; i++)
        {
            var b = list[i];
            if (b == null)
                continue;

            var id = b.BrandId;
            if (string.IsNullOrEmpty(id))
                continue;

            yield return new ValueDropdownItem<string>(b.DisplayName, id);
        }
#else
        yield break;
#endif
    }

    /// <summary>
    /// Модели выбранного бренда.
    /// </summary>
    public static IEnumerable<ValueDropdownItem<string>> ModelItemsForBrand(string brandId, bool includeAnyOption)
    {
#if UNITY_EDITOR
        if (includeAnyOption)
            yield return new ValueDropdownItem<string>("(любая)", string.Empty);

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null || string.IsNullOrWhiteSpace(brandId))
            yield break;

        foreach (var m in db.EnumerateModels(brandId))
            yield return new ValueDropdownItem<string>(m, m);
#else
        yield break;
#endif
    }

    /// <summary>
    /// Типы запчастей.
    /// </summary>
    public static IEnumerable<ValueDropdownItem<string>> PartTypeItems(bool includeAnyOption)
    {
#if UNITY_EDITOR
        if (includeAnyOption)
            yield return new ValueDropdownItem<string>("(любой)", string.Empty);

        var db = PhonePartsDatabaseAccess.TryGetForEditor();
        if (db == null)
            yield break;

        var list = db.PartTypes;
        for (var i = 0; i < list.Count; i++)
        {
            var t = list[i];
            if (t == null)
                continue;

            var id = t.TypeId;
            if (string.IsNullOrEmpty(id))
                continue;

            yield return new ValueDropdownItem<string>(t.DisplayName, id);
        }
#else
        yield break;
#endif
    }
}
