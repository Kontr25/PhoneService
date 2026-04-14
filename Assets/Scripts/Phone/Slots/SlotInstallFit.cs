/// <summary>
/// Насколько перетаскиваемая деталь соответствует слоту (превью и логика отпускания).
/// </summary>
public enum SlotInstallFit
{
    /// <summary>
    /// Слот не выбран или нет оценки.
    /// </summary>
    None,

    /// <summary>
    /// Тип и модель совпадают с телефоном.
    /// </summary>
    FullMatch,

    /// <summary>
    /// Тип подходит, модель другая; установка разрешена.
    /// </summary>
    WrongModel,

    /// <summary>
    /// Тип не подходит — отскок при отпускании.
    /// </summary>
    WrongType
}
