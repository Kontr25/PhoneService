using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

/// <summary>
/// Запчасть: идентичность и визуал загружаются из записи базы; при перетаскивании оркестрирует пробу, превью слота и установку через сервисы.
/// </summary>
public sealed class PhoneRepairPart : Grabbable, IGrabLifecycle, IGrabPhysicsTick
{
    /// <summary>
    /// MeshFilter с мешем детали для превью, если <see cref="_installPreviewMesh"/> не задан.
    /// </summary>
    [SerializeField]
    private MeshFilter _installPreviewMeshFilter;

    /// <summary>
    /// Рендерер визуала запчасти.
    /// </summary>
    [SerializeField]
    private MeshRenderer _partMeshRenderer;

    /// <summary>
    /// Id записи запчасти в базе.
    /// </summary>
    [Tooltip("Выберите запись из Phone Parts Database.")]
    [ValueDropdown(nameof(EditorPartRecordItems), IsUniqueList = false, DropdownTitle = "Запись запчасти")]
    [SerializeField]
    private string _partRecordId;

    /// <summary>
    /// Id категории запчасти.
    /// </summary>
    [FormerlySerializedAs("_partTypeId")]
    [HideInInspector]
    [SerializeField]
    private string _partCategoryId;

    /// <summary>
    /// Название телефона, для которого предназначена запчасть.
    /// </summary>
    [FormerlySerializedAs("_partBrandId")]
    [HideInInspector]
    [SerializeField]
    private string _partPhoneName;

    /// <summary>
    /// Модель телефона.
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private string _partModelName;

    /// <summary>
    /// Качество запчасти.
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private PartQualityType _partQualityType;

    /// <summary>
    /// Стоимость запчасти.
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private int _partCost;

    /// <summary>
    /// Описание запчасти.
    /// </summary>
    [HideInInspector]
    [SerializeField]
    [TextArea]
    private string _partDescription;

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
    /// Id категории детали.
    /// </summary>
    public string PartCategoryId =>
        string.IsNullOrWhiteSpace(_partCategoryId) ? string.Empty : _partCategoryId.Trim();

    /// <summary>
    /// Название телефона детали.
    /// </summary>
    public string PartPhoneName =>
        string.IsNullOrWhiteSpace(_partPhoneName) ? string.Empty : _partPhoneName.Trim();

    /// <summary>
    /// Модель детали.
    /// </summary>
    public string PartModelName =>
        string.IsNullOrWhiteSpace(_partModelName) ? string.Empty : _partModelName.Trim();

    /// <summary>
    /// Качество запчасти.
    /// </summary>
    public PartQualityType PartQualityType => _partQualityType;

    /// <summary>
    /// Стоимость запчасти.
    /// </summary>
    public int PartCost => _partCost;

    /// <summary>
    /// Описание запчасти.
    /// </summary>
    public string PartDescription => _partDescription ?? string.Empty;

    /// <summary>
    /// Заданы ли название телефона и модель.
    /// </summary>
    public bool HasModelSpecified =>
        !string.IsNullOrWhiteSpace(_partPhoneName) && !string.IsNullOrWhiteSpace(_partModelName);

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
        ApplyDatabaseRecord();
        _installPreviewMesh = _installPreviewMeshFilter.mesh;
        _slotProbeCollider = _slotProbeSphere.ProbeCollider;
        _previewCursor.SlotIndex = -1;
        _previewCursor.Phone = null;
    }

    /// <summary>
    /// Инициализирует запчасть данными из записи базы.
    /// </summary>
    /// <param name="record">Запись запчасти.</param>
    public void InitializeFromDatabaseRecord(PartRecordEntry record)
    {
        if (record == null)
            throw new System.InvalidOperationException();

        _partCategoryId = record.PartCategoryId;
        _partPhoneName = record.PhoneName;
        _partModelName = record.PhoneModelName;
        _partQualityType = record.PartQualityType;
        _partCost = record.Cost;
        _partDescription = record.Description;

        _installPreviewMeshFilter.sharedMesh = record.PartMesh;
        _partMeshRenderer.sharedMaterial = record.PartMaterial;
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
                case SlotInstallFit.WrongCategory:
                    _installMotion.ApplyWrongCategoryBounce(this, _wrongTypeBounceImpulse);
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
    /// Применяет отскок по горизонтали при неверной категории слота.
    /// </summary>
    /// <param name="impulseMagnitude">Величина импульса.</param>
    internal void DoRigidbodyApplyWrongCategoryBounce(float impulseMagnitude)
    {
        _rigidbodyService.ApplyHorizontalBounce(PhysicsBody, impulseMagnitude);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Элементы выпадающего списка записей запчастей для Odin.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPartRecordItems() =>
        PhonePartsDatabaseDropdowns.PartRecordItems(includeAnyOption: true);

#else
    /// <summary>
    /// Заглушка сборки без редактора.
    /// </summary>
    private IEnumerable<ValueDropdownItem<string>> EditorPartRecordItems()
    {
        yield break;
    }
#endif

    /// <summary>
    /// Применяет запись из базы по идентификатору.
    /// </summary>
    private void ApplyDatabaseRecord()
    {
        if (string.IsNullOrWhiteSpace(_partRecordId))
            throw new System.InvalidOperationException();

        var database = PhonePartsDatabaseAccess.TryGetRuntime();
        if (database == null)
            throw new System.InvalidOperationException();

        if (!database.TryGetPartRecord(_partRecordId, out var record))
            throw new System.InvalidOperationException();

        if (!database.IsValidPartRecord(record))
            throw new System.InvalidOperationException();

        InitializeFromDatabaseRecord(record);
    }
}
