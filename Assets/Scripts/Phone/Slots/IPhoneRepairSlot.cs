using UnityEngine;

/// <summary>
/// Слот ремонта на корпусе: сокет, категория запчасти, превью установки.
/// </summary>
public interface IPhoneRepairSlot
{
    /// <summary>
    /// Сокет для родителя установленной детали.
    /// </summary>
    Transform Socket { get; }

    /// <summary>
    /// Принимает ли слот указанную категорию запчасти.
    /// </summary>
    /// <param name="partCategoryId">Category Id детали.</param>
    /// <returns>True, если категория подходит.</returns>
    bool AcceptsPartCategory(string partCategoryId);

    /// <summary>
    /// Показывает или скрывает превью детали в слоте.
    /// </summary>
    /// <param name="visible">Видимость.</param>
    /// <param name="fit">Категория соответствия (материал превью).</param>
    /// <param name="part">Деталь для меша превью.</param>
    void SetInstallPreview(bool visible, SlotInstallFit fit, PhoneRepairPart part);
}
