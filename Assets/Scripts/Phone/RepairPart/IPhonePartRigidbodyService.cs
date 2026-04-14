using UnityEngine;

/// <summary>
/// Централизованное управление состоянием <see cref="Rigidbody"/> запчасти.
/// </summary>
public interface IPhonePartRigidbodyService
{
    /// <summary>
    /// Подготавливает тело для свободного перетаскивания/падения.
    /// </summary>
    /// <param name="rigidbody">Тело детали.</param>
    void SetFreeState(Rigidbody rigidbody);

    /// <summary>
    /// Подготавливает тело для твина установки (кинетика, без гравитации, без скоростей).
    /// </summary>
    /// <param name="rigidbody">Тело детали.</param>
    void SetInstallTweenState(Rigidbody rigidbody);

    /// <summary>
    /// Фиксирует тело в установленном состоянии в сокете.
    /// </summary>
    /// <param name="rigidbody">Тело детали.</param>
    void SetInstalledState(Rigidbody rigidbody);

    /// <summary>
    /// Добавляет горизонтальный импульс для реакции на неверный тип.
    /// </summary>
    /// <param name="rigidbody">Тело детали.</param>
    /// <param name="impulseMagnitude">Величина импульса.</param>
    void ApplyHorizontalBounce(Rigidbody rigidbody, float impulseMagnitude);
}
