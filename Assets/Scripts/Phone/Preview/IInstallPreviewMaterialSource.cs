using UnityEngine;

/// <summary>
/// Материалы превью установки по результату <see cref="SlotInstallFit"/>.
/// </summary>
public interface IInstallPreviewMaterialSource
{
    /// <summary>
    /// Материал для указанного соответствия.
    /// </summary>
    /// <param name="fit">Результат оценки.</param>
    /// <returns>Материал или null, если в источнике не задано.</returns>
    Material GetMaterialForFit(SlotInstallFit fit);
}
