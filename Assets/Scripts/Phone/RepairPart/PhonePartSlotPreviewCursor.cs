/// <summary>
/// Состояние «на каком телефоне и слоте сейчас показано превью установки» для одной запчасти.
/// </summary>
public struct PhonePartSlotPreviewCursor
{
    /// <summary>
    /// Корпус с активным превью или null.
    /// </summary>
    public PhoneController Phone;

    /// <summary>
    /// Индекс слота или -1, если превью снято.
    /// </summary>
    public int SlotIndex;
}
