using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

/// <summary>
/// Запчасть: идентичность из базы; при перетаскивании оркестрирует пробу, превью слота и установку через сервисы.
/// </summary>
public sealed class PhoneRepairPart : Grabbable, IGrabLifecycle, IGrabPhysicsTick
{
    /// <summary>
    /// MeshFilter с мешем детали для превью, если <see cref="_installPreviewMesh"/> не задан.
    /// </summary>
    [SerializeField]
    private MeshFilter _installPreviewMeshFilter;

    /// <summary>
    /// Type Id (battery, camera…).
    /// </summary>
    [Tooltip("Тип запчасти из базы.")]
    [ValueDropdown(nameof(EditorPartTypeItems), IsUniqueList = false, DropdownTitle = "Тип запчасти")]
    [SerializeField]
    private string _partTypeId;

    /// <summary>
    /// Бренд телефона, под который деталь.
    /// </summary>
    [Tooltip("Бренд из базы.")]
    [ValueDropdown(nameof(EditorBrandItems), IsUniqueList = false, DropdownTitle = "Бренд")]
    [SerializeField]
    private string _partBrandId;

    /// <summary>
    /// Модель телефона.
    /// </summary>
    [Tooltip("Модель для выбранного бренда.")]
    [ValueDropdown(nameof(EditorModelItems), IsUniqueList = false, DropdownTitle = "Модель")]
    [SerializeField]
    private string _partModelName;

    /// <summary>
    /// Маска луча вниз от детали (первое попадание).
    /// </summary>
    [SerializeField]
    private LayerMask _downRayMask = ~0;

    /// <summary>
    /// Длина луча вниз.
    /// </summary>
    [SerializeField]
    private float _downRayLength = 12f;

    /// <summary>
    /// Длительность перелёта в сокет.
    /// </summary>
    [SerializeField]
    private float _installTweenDuration = 0.35f;

    /// <summary>
    /// Easing перелёта в сокет.
    /// </summary>
    [SerializeField]
    private Ease _installEase = Ease.OutQuad;

    /// <summary>
    /// Импульс при отпускании над слотом неверного типа.
    /// </summary>
    [SerializeField]
    private float _wrongTypeBounceImpulse = 3f;
    
    /// <summary>
    /// Сфера-проб для выбора ближайшего слота по триггерам.
    /// </summary>
    private PhoneSlotProbeSphere _slotProbeSphere;

    /// <summary>
    /// Телефон, куда установлена деталь.
    /// </summary>
    private PhoneController _installedIn;

    /// <summary>
    /// Курсор превью на слоте.
    /// </summary>
    private PhonePartSlotPreviewCursor _previewCursor;

    /// <summary>
    /// Буфер луча вниз.
    /// </summary>
    private RaycastHit[] _hitBuffer;

    /// <summary>
    /// Коллайдер сферы-пробы: луч вниз не должен учитывать его.
    /// </summary>
    private SphereCollider _slotProbeCollider;
    
    /// <summary>
    /// Меш для превью в слоте; если null — берётся <see cref="MeshFilter.sharedMesh"/> с этого объекта.
    /// </summary>
    private Mesh _installPreviewMesh;

    /// <summary>
    /// Отбор попадания луча вниз.
    /// </summary>
    private IPhonePartDownRayHitSelector _downRayHitSelector = null!;

    /// <summary>
    /// Синхронизация превью слота.
    /// </summary>
    private IPhonePartSlotInstallPreviewSync _slotInstallPreviewSync = null!;

    /// <summary>
    /// Твин установки и отскок.
    /// </summary>
    private IPhonePartInstallMotion _installMotion = null!;

    /// <summary>
    /// Централизованная политика состояний <see cref="Rigidbody"/>.
    /// </summary>
    private IPhonePartRigidbodyService _rigidbodyService = null!;

    /// <summary>
    /// Type Id детали.
    /// </summary>
    public string PartTypeId =>
        string.IsNullOrWhiteSpace(_partTypeId) ? string.Empty : _partTypeId.Trim();

    /// <summary>
    /// Brand Id детали.
    /// </summary>
    public string PartBrandId =>
        string.IsNullOrWhiteSpace(_partBrandId) ? string.Empty : _partBrandId.Trim();

    /// <summary>
    /// Модель детали.
    /// </summary>
    public string PartModelName =>
        string.IsNullOrWhiteSpace(_partModelName) ? string.Empty : _partModelName.Trim();

    /// <summary>
    /// Заданы ли бренд и модель.
    /// </summary>
    public bool HasModelSpecified =>
        !string.IsNullOrWhiteSpace(_partBrandId) && !string.IsNullOrWhiteSpace(_partModelName);

    /// <summary>
    /// Телефон установки.
    /// </summary>
    public PhoneController InstalledIn => _installedIn;

    /// <summary>
    /// Внедряет сервисы перетаскивания и установки.
    /// </summary>
    /// <param name="downRayHitSelector">Луч вниз.</param>
    /// <param name="slotInstallPreviewSync">Превью слота.</param>
    /// <param name="installMotion">Движение при установке.</param>
    /// <param name="rigidbodyService">Политика состояний тела.</param>
    [Inject]
    private void Construct(
        IPhonePartDownRayHitSelector downRayHitSelector,
        IPhonePartSlotInstallPreviewSync slotInstallPreviewSync,
        IPhonePartInstallMotion installMotion,
        IPhonePartRigidbodyService rigidbodyService,
        PhoneSlotProbeSphere slotProbeSphere)
    {
        _downRayHitSelector = downRayHitSelector;
        _slotInstallPreviewSync = slotInstallPreviewSync;
        _installMotion = installMotion;
        _rigidbodyService = rigidbodyService;
        _slotProbeSphere = slotProbeSphere;
    }

    /// <summary>
    /// Меш для отображения в превью слота.
    /// </summary>
    /// <returns>Меш или null.</returns>
    public Mesh GetInstallPreviewMesh() => _installPreviewMesh;

    /// <inheritdoc />
    protected override void OnGrabbableAwake()
    {
        _installPreviewMesh = _installPreviewMeshFilter.mesh;
        _slotProbeCollider = _slotProbeSphere.ProbeCollider;
        _previewCursor.SlotIndex = -1;
        _previewCursor.Phone = null;
    }

    /// <inheritdoc />
    public void OnGrabSessionStarting(in Ray pickRay)
    {
        transform.DOKill();
        _slotInstallPreviewSync.Clear(ref _previewCursor);

        if (_slotProbeSphere != null)
            _slotProbeSphere.ResetMoveThreshold();

        if (TryGetFirstHitBelow(out var hitPoint))
            _slotProbeSphere.MoveToWorldPoint(hitPoint);

        if (_installedIn != null)
        {
            _installedIn.Slots.NotifyPartDetached(this);
            transform.SetParent(null, true);
            _installedIn = null;
        }

        DoRigidbodySetFreeState();
    }

    /// <inheritdoc />
    public void OnGrabSessionEnded(bool preserveVelocities)
    {
        _slotInstallPreviewSync.Clear(ref _previewCursor);

        try
        {
            if (_slotProbeSphere == null ||
                !_slotProbeSphere.TryGetPickedNearestFreeSlot(out var phone, out var slot))
                return;

            var fit = phone.Slots.EvaluateSlotInstallFit(slot, this);
            switch (fit)
            {
                case SlotInstallFit.FullMatch:
                case SlotInstallFit.WrongModel:
                    _installMotion.BeginTweenToSocket(this, phone, slot, _installTweenDuration, _installEase);
                    break;
                case SlotInstallFit.WrongType:
                    _installMotion.ApplyWrongTypeBounce(this, _wrongTypeBounceImpulse);
                    break;
            }
        }
        finally
        {
            if (_slotProbeSphere != null)
                _slotProbeSphere.ResetToDefaultWorldPose();
        }
    }

    /// <inheritdoc />
    public void OnGrabPhysicsStep(Camera camera, Vector2 screenPoint)
    {
        if (_slotProbeSphere == null)
            return;

        if (!TryGetFirstHitBelow(out var hitPoint))
        {
            _slotInstallPreviewSync.Clear(ref _previewCursor);
            return;
        }

        _slotProbeSphere.MoveToWorldPoint(hitPoint);

        if (!_slotProbeSphere.TryGetPickedNearestFreeSlot(out var phone, out var slot))
        {
            _slotInstallPreviewSync.Clear(ref _previewCursor);
            return;
        }

        var fit = phone.Slots.EvaluateSlotInstallFit(slot, this);
        _slotInstallPreviewSync.ApplyOrRefresh(ref _previewCursor, phone, slot, fit, this);
    }

    /// <summary>
    /// Помечает деталь как установленную без смены родителя.
    /// </summary>
    /// <param name="phone">Телефон.</param>
    internal void BindInstalled(PhoneController phone)
    {
        _installedIn = phone;
    }

    /// <summary>
    /// Прикрепляет к сокету и делает kinematic.
    /// </summary>
    /// <param name="phone">Телефон.</param>
    /// <param name="socket">Сокет.</param>
    internal void InstallIntoPhone(PhoneController phone, Transform socket)
    {
        transform.DOKill();
        _installedIn = phone;
        transform.SetParent(socket, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        DoRigidbodySetInstalledState();

        if (_slotProbeSphere != null)
            _slotProbeSphere.ResetToDefaultWorldPose();
    }

    /// <summary>
    /// Первое попадание луча вниз с учётом фильтров сервиса.
    /// </summary>
    private bool TryGetFirstHitBelow(out Vector3 hitPoint)
    {
        var rb = PhysicsBody;
        return _downRayHitSelector.TrySelectHitPointBelow(
            rb,
            _slotProbeCollider,
            rb.worldCenterOfMass,
            _downRayMask,
            _downRayLength,
            ref _hitBuffer,
            out hitPoint);
    }

    /// <summary>
    /// Переводит тело детали в свободное физическое состояние.
    /// </summary>
    internal void DoRigidbodySetFreeState()
    {
        _rigidbodyService.SetFreeState(PhysicsBody);
    }

    /// <summary>
    /// Переводит тело детали в состояние твина установки.
    /// </summary>
    internal void DoRigidbodySetInstallTweenState()
    {
        _rigidbodyService.SetInstallTweenState(PhysicsBody);
    }

    /// <summary>
    /// Переводит тело детали в финальное установленное состояние.
    /// </summary>
    internal void DoRigidbodySetInstalledState()
    {
        _rigidbodyService.SetInstalledState(PhysicsBody);
    }

    /// <summary>
    /// Применяет отскок по горизонтали при неверном типе слота.
    /// </summary>
    /// <param name="impulseMagnitude">Величина импульса.</param>
    internal void DoRigidbodyApplyWrongTypeBounce(float impulseMagnitude)
    {
        _rigidbodyService.ApplyHorizontalBounce(PhysicsBody, impulseMagnitude);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Элементы выпадающего списка типов запчастей для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPartTypeItems() =>
        PhonePartsDatabaseDropdowns.PartTypeItems(includeAnyOption: false);

    /// <summary>
    /// Элементы выпадающего списка брендов для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems() =>
        PhonePartsDatabaseDropdowns.BrandItems(includeAnyOption: false);

    /// <summary>
    /// Элементы выпадающего списка моделей для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorModelItems() =>
        PhonePartsDatabaseDropdowns.ModelItemsForBrand(_partBrandId, includeAnyOption: false);

#else
    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPartTypeItems()
    {
        yield break;
    }

    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorBrandItems()
    {
        yield break;
    }

    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorModelItems()
    {
        yield break;
    }
#endif
}
