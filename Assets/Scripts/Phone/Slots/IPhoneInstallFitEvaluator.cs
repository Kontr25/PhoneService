/// <summary>
/// Оценка соответствия детали слоту с учётом модели телефона.
/// </summary>
public interface IPhoneInstallFitEvaluator
{
    /// <summary>
    /// Возвращает категорию соответствия.
    /// </summary>
    /// <param name="phone">Модель телефона.</param>
    /// <param name="slot">Слот.</param>
    /// <param name="part">Деталь.</param>
    /// <returns>Категория соответствия.</returns>
    SlotInstallFit Evaluate(IPhoneModelIdentity phone, IPhoneRepairSlot slot, PhoneRepairPart part);
}
