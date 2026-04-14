using UnityEngine;

/// <summary>
/// Материалы превью из полей <see cref="PhonePartsDatabase"/>.
/// </summary>
public sealed class InstallPreviewMaterialSourceFromDatabase : IInstallPreviewMaterialSource
{
    /// <summary>
    /// Таблица с ссылками на материалы превью.
    /// </summary>
    private readonly PhonePartsDatabase _database;

    /// <summary>
    /// Создаёт источник по базе.
    /// </summary>
    /// <param name="database">База запчастей.</param>
    public InstallPreviewMaterialSourceFromDatabase(PhonePartsDatabase database)
    {
        _database = database;
    }

    /// <inheritdoc />
    public Material GetMaterialForFit(SlotInstallFit fit)
    {
        return fit == SlotInstallFit.FullMatch
            ? _database.ValidSlotPreviewMaterial
            : _database.InvalidSlotPreviewMaterial;
    }
}
