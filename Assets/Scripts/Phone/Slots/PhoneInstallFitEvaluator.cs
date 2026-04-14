using System;

/// <summary>
/// Оценка соответствия без внешних зависимостей (чистая политика).
/// </summary>
public sealed class PhoneInstallFitEvaluator : IPhoneInstallFitEvaluator
{
    /// <inheritdoc />
    public SlotInstallFit Evaluate(IPhoneModelIdentity phone, IPhoneRepairSlot slot, PhoneRepairPart part)
    {
        if (part == null || slot == null || phone == null)
            return SlotInstallFit.None;

        if (!slot.AcceptsPartType(part.PartTypeId))
            return SlotInstallFit.WrongType;

        if (!phone.HasPhoneModelSpecified || !part.HasModelSpecified)
            return SlotInstallFit.WrongType;

        return PartMatchesPhoneModel(phone, part) ? SlotInstallFit.FullMatch : SlotInstallFit.WrongModel;
    }

    /// <summary>
    /// Совпадение бренда и модели детали с телефоном.
    /// </summary>
    private static bool PartMatchesPhoneModel(IPhoneModelIdentity phone, PhoneRepairPart part)
    {
        return string.Equals(phone.PhoneBrandId, part.PartBrandId, StringComparison.Ordinal)
               && string.Equals(phone.PhoneModelName, part.PartModelName, StringComparison.Ordinal);
    }
}
