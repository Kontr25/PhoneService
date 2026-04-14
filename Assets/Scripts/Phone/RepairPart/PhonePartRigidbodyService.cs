using UnityEngine;

/// <summary>
/// Политика переключения состояний <see cref="Rigidbody"/> для запчасти телефона.
/// </summary>
public sealed class PhonePartRigidbodyService : IPhonePartRigidbodyService
{
    /// <inheritdoc />
    public void SetFreeState(Rigidbody rigidbody)
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
    }

    /// <inheritdoc />
    public void SetInstallTweenState(Rigidbody rigidbody)
    {
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    /// <inheritdoc />
    public void SetInstalledState(Rigidbody rigidbody)
    {
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    /// <inheritdoc />
    public void ApplyHorizontalBounce(Rigidbody rigidbody, float impulseMagnitude)
    {
        if (rigidbody.isKinematic)
            return;

        var push = Random.insideUnitSphere;
        push.y = 0f;
        if (push.sqrMagnitude < 1e-6f)
            push = Vector3.right;

        push.Normalize();
        rigidbody.AddForce(push * impulseMagnitude, ForceMode.Impulse);
    }
}
