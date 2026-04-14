/// <summary>
/// Реализация синхронизации превью слота при перетаскивании запчасти.
/// </summary>
public sealed class PhonePartSlotInstallPreviewSync : IPhonePartSlotInstallPreviewSync
{
    /// <inheritdoc />
    public void Clear(ref PhonePartSlotPreviewCursor cursor)
    {
        if (cursor.Phone != null && cursor.SlotIndex >= 0)
            cursor.Phone.Slots.SetInstallPreview(cursor.SlotIndex, false, SlotInstallFit.None, null);

        cursor.Phone = null;
        cursor.SlotIndex = -1;
    }

    /// <inheritdoc />
    public void ApplyOrRefresh(
        ref PhonePartSlotPreviewCursor cursor,
        PhoneController phone,
        int slotIndex,
        SlotInstallFit fit,
        PhoneRepairPart part)
    {
        if (phone == cursor.Phone && slotIndex == cursor.SlotIndex)
        {
            phone.Slots.SetInstallPreview(slotIndex, true, fit, part);
            return;
        }

        Clear(ref cursor);
        cursor.Phone = phone;
        cursor.SlotIndex = slotIndex;
        phone.Slots.SetInstallPreview(slotIndex, true, fit, part);
    }
}
