using DG.Tweening;

/// <summary>
/// Движение запчасти при установке (твин к сокету) и отскок при неверной категории.
/// </summary>
public interface IPhonePartInstallMotion
{
    /// <summary>
    /// Запускает перелёт к сокету и по завершении вызывает <see cref="IPhoneSlotService.TryInstall"/>.
    /// </summary>
    /// <param name="part">Запчасть.</param>
    /// <param name="phone">Корпус.</param>
    /// <param name="slotIndex">Слот.</param>
    /// <param name="duration">Длительность.</param>
    /// <param name="ease">Easing.</param>
    void BeginTweenToSocket(PhoneRepairPart part, PhoneController phone, int slotIndex, float duration, Ease ease);

    /// <summary>
    /// Импульс в горизонтальной плоскости для реакции на неверную категорию слота.
    /// </summary>
    /// <param name="part">Запчасть.</param>
    /// <param name="impulseMagnitude">Величина импульса.</param>
    void ApplyWrongCategoryBounce(PhoneRepairPart part, float impulseMagnitude);
}
