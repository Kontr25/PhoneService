using UnityEngine;

/// <summary>
/// Доступ к <see cref="PhonePartsDatabase"/> в игре (<see cref="TryGetRuntime"/>).
/// Поиск по проекту без Resources — только в редакторе: <see cref="TryGetForEditor"/>.
/// </summary>
public static class PhonePartsDatabaseAccess
{
    /// <summary>
    /// Загрузка для рантайма и билда: только <see cref="PhonePartsDatabase.ResourcesAssetName"/> в Resources.
    /// </summary>
    public static PhonePartsDatabase TryGetRuntime()
    {
        return Resources.Load<PhonePartsDatabase>(PhonePartsDatabase.ResourcesAssetName);
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
        return UnityEditor.AssetDatabase.LoadAssetAtPath<PhonePartsDatabase>(path);
    }
#endif
}
