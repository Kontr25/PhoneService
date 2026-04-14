using DG.Tweening;

/// <summary>
/// Реализация твина установки и отскока запчасти.
/// </summary>
public sealed class PhonePartInstallMotion : IPhonePartInstallMotion
{
    /// <inheritdoc />
    public void BeginTweenToSocket(PhoneRepairPart part, PhoneController phone, int slotIndex, float duration, Ease ease)
    {
        if (part == null || phone == null)
            return;

        var socket = phone.Slots.GetSlotSocket(slotIndex);
        if (socket == null)
            return;

        part.DoRigidbodySetInstallTweenState();

        var transform = part.transform;
        transform.DOKill();
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(socket.position, duration).SetEase(ease));
        seq.Join(transform.DORotateQuaternion(socket.rotation, duration).SetEase(ease));
        seq.OnComplete(() =>
        {
            if (phone != null)
                phone.Slots.TryInstall(part, slotIndex);
        });
    }

    /// <inheritdoc />
    public void ApplyWrongTypeBounce(PhoneRepairPart part, float impulseMagnitude)
    {
        if (part == null)
            return;

        part.DoRigidbodyApplyWrongTypeBounce(impulseMagnitude);
    }
}
