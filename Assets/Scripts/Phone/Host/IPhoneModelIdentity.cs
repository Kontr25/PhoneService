/// <summary>
/// Идентичность модели телефона (название телефона + модель из базы).
/// </summary>
public interface IPhoneModelIdentity
{
    /// <summary>
    /// Название телефона.
    /// </summary>
    string PhoneName { get; }

    /// <summary>
    /// Имя модели телефона.
    /// </summary>
    string PhoneModelName { get; }

    /// <summary>
    /// Заданы ли название телефона и модель.
    /// </summary>
    bool HasPhoneModelSpecified { get; }
}
