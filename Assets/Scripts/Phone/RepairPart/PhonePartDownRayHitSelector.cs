using UnityEngine;

/// <summary>
/// Реализация отбора попадания луча вниз для перетаскиваемой запчасти.
/// </summary>
public sealed class PhonePartDownRayHitSelector : IPhonePartDownRayHitSelector
{
    /// <summary>
    /// Минимальная ёмкость буфера, при которой имеет смысл избегать перераспределения.
    /// </summary>
    private const int MinReusableBufferSize = 8;

    /// <summary>
    /// Базовая ёмкость буфера RaycastNonAlloc для старта без частых realoc.
    /// </summary>
    private const int DefaultBufferSize = 16;

    /// <inheritdoc />
    public bool TrySelectHitPointBelow(
        Rigidbody partRigidbody,
        Collider probeColliderToIgnore,
        Vector3 origin,
        LayerMask layerMask,
        float maxDistance,
        ref RaycastHit[] hitBuffer,
        out Vector3 hitPoint)
    {
        hitPoint = default;
        if (hitBuffer == null || hitBuffer.Length < MinReusableBufferSize)
            hitBuffer = new RaycastHit[DefaultBufferSize];

        var count = Physics.RaycastNonAlloc(
            origin,
            Vector3.down,
            hitBuffer,
            maxDistance,
            layerMask,
            QueryTriggerInteraction.Ignore);

        var bestDist = float.MaxValue;
        var found = false;

        for (var i = 0; i < count; i++)
        {
            var h = hitBuffer[i];
            if (h.collider != null && h.collider.attachedRigidbody == partRigidbody)
                continue;

            if (probeColliderToIgnore != null && h.collider == probeColliderToIgnore)
                continue;

            if (h.distance >= bestDist)
                continue;

            bestDist = h.distance;
            hitPoint = h.point;
            found = true;
        }

        return found;
    }
}
