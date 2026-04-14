/// <summary>
/// Синхронизация превью установки на слоте телефона при перетаскивании одной запчасти.
/// </summary>
public interface IPhonePartSlotInstallPreviewSync
{
    /// <summary>
    /// Снимает превью с текущего курсора и обнуляет его.
    /// </summary>
    /// <param name="cursor">Курсор превью.</param>
    void Clear(ref PhonePartSlotPreviewCursor cursor);

    /// <summary>
    /// Обновляет превью: при смене телефона/слота гасит старое и включает новое.
    /// </summary>
    /// <param name="cursor">Текущий курсор; обновляется при смене цели.</param>
    /// <param name="phone">Телефон под пробой.</param>
    /// <param name="slotIndex">Индекс слота.</param>
    /// <param name="fit">Категория соответствия.</param>
    /// <param name="part">Запчасть (меш превью).</param>
    void ApplyOrRefresh(
        ref PhonePartSlotPreviewCursor cursor,
        PhoneController phone,
        int slotIndex,
        SlotInstallFit fit,
        PhoneRepairPart part);
}
