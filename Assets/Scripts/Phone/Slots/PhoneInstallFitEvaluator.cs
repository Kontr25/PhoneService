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

        if (!slot.AcceptsPartCategory(part.PartCategoryId))
            return SlotInstallFit.WrongCategory;

        if (!phone.HasPhoneModelSpecified || !part.HasModelSpecified)
            return SlotInstallFit.WrongCategory;

        return PartMatchesPhoneModel(phone, part) ? SlotInstallFit.FullMatch : SlotInstallFit.WrongModel;
    }

    /// <summary>
    /// Совпадение названия телефона и модели детали с телефоном.
    /// </summary>
    private static bool PartMatchesPhoneModel(IPhoneModelIdentity phone, PhoneRepairPart part)
    {
        return string.Equals(phone.PhoneName, part.PartPhoneName, StringComparison.Ordinal)
               && string.Equals(phone.PhoneModelName, part.PartModelName, StringComparison.Ordinal);
    }
}
