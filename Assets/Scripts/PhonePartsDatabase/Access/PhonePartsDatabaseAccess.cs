using UnityEngine;

/// <summary>
/// Доступ к <see cref="PhonePartsDatabase"/> в игре (<see cref="TryGetRuntime"/>): ассет в папке Resources с именем <see cref="PhonePartsDatabase.ResourcesAssetName"/>.
/// Поиск по проекту без Resources — только в редакторе: <see cref="TryGetForEditor"/>.
/// </summary>
public static class PhonePartsDatabaseAccess
{
    /// <summary>
    /// Кэш после первого успешного разрешения (без повторных <see cref="Resources.Load"/>).
    /// </summary>
    private static PhonePartsDatabase _cachedRuntime;

    /// <summary>
    /// Загрузка для рантайма и билда: один раз <see cref="Resources.Load"/> по <see cref="PhonePartsDatabase.ResourcesAssetName"/>.
    /// </summary>
    public static PhonePartsDatabase TryGetRuntime()
    {
        if (_cachedRuntime != null)
            return _cachedRuntime;

        _cachedRuntime = Resources.Load<PhonePartsDatabase>(PhonePartsDatabase.ResourcesAssetName);
        return _cachedRuntime;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Только редактор: Resources, иначе первый asset в проекте. В билде игры отсутствует.
    /// </summary>
    public static PhonePartsDatabase TryGetForEditor()
    {
        var r = TryGetRuntime();
        if (r != null)
            return r;

        var guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(PhonePartsDatabase)}");
        if (guids.Length == 0)
            return null;

        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
        var db = UnityEditor.AssetDatabase.LoadAssetAtPath<PhonePartsDatabase>(path);
        if (db != null)
            _cachedRuntime = db;

        return db;
    }
#endif
}
