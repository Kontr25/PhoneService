using UnityEngine;

/// <summary>
/// Учёт слотов запчастей на корпусе: сокеты, превью, установка и синхронизация с иерархией.
/// </summary>
public interface IPhoneSlotService
{
    /// <summary>
    /// Число слотов по индексу.
    /// </summary>
    int SlotCount { get; }

    /// <summary>
    /// Деталь в слоте или null.
    /// </summary>
    /// <param name="slotIndex">Индекс слота.</param>
    /// <returns>Установленная деталь или null.</returns>
    PhoneRepairPart GetOccupant(int slotIndex);

    /// <summary>
    /// Сокет слота по индексу.
    /// </summary>
    /// <param name="slotIndex">Индекс.</param>
    /// <returns>Transform сокета или null.</returns>
    Transform GetSlotSocket(int slotIndex);

    /// <summary>
    /// Оценивает деталь относительно слота.
    /// </summary>
    /// <param name="slotIndex">Индекс слота.</param>
    /// <param name="part">Деталь.</param>
    /// <returns>Категория соответствия.</returns>
    SlotInstallFit EvaluateSlotInstallFit(int slotIndex, PhoneRepairPart part);

    /// <summary>
    /// Показывает или скрывает превью на слоте.
    /// </summary>
    /// <param name="slotIndex">Индекс слота.</param>
    /// <param name="visible">Показать или скрыть.</param>
    /// <param name="fit">Категория соответствия.</param>
    /// <param name="part">Деталь для меша превью.</param>
    void SetInstallPreview(int slotIndex, bool visible, SlotInstallFit fit, PhoneRepairPart part);

    /// <summary>
    /// Выключает превью на всех слотах.
    /// </summary>
    void HideAllInstallPreviews();

    /// <summary>
    /// Совпадение бренда и модели детали с телефоном-владельцем.
    /// </summary>
    /// <param name="part">Деталь.</param>
    /// <returns>True при полном совпадении.</returns>
    bool PartMatchesPhoneModel(PhoneRepairPart part);

    /// <summary>
    /// Регистрирует детали под сокетами из иерархии.
    /// </summary>
    void ResyncInstalledPartsFromHierarchy();

    /// <summary>
    /// Снимает деталь из учёта слота.
    /// </summary>
    /// <param name="part">Деталь.</param>
    void NotifyPartDetached(PhoneRepairPart part);

    /// <summary>
    /// Устанавливает деталь в слот при допустимом типе.
    /// </summary>
    /// <param name="part">Деталь.</param>
    /// <param name="slotIndex">Индекс слота.</param>
    /// <returns>True при успехе.</returns>
    bool TryInstall(PhoneRepairPart part, int slotIndex);
}
