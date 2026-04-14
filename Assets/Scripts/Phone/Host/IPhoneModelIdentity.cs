/// <summary>
/// Идентичность модели телефона (бренд + модель из базы).
/// </summary>
public interface IPhoneModelIdentity
{
    /// <summary>
    /// Brand Id телефона.
    /// </summary>
    string PhoneBrandId { get; }

    /// <summary>
    /// Имя модели телефона.
    /// </summary>
    string PhoneModelName { get; }

    /// <summary>
    /// Заданы ли бренд и модель.
    /// </summary>
    bool HasPhoneModelSpecified { get; }
}
