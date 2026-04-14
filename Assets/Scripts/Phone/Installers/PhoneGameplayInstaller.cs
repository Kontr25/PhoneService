using UnityEngine;
using Zenject;

/// <summary>
/// Регистрация сервисов телефона и превью (композиционный корень сцены).
/// </summary>
public sealed class PhoneGameplayInstaller : MonoInstaller
{
    /// <summary>
    /// Сфера-проб для выбора ближайшего слота по триггерам.
    /// </summary>
    [SerializeField]
    private PhoneSlotProbeSphere _slotProbeSphere;
    
    /// <inheritdoc />
    public override void InstallBindings()
    {
        Container.Bind<IPhoneInstallFitEvaluator>().To<PhoneInstallFitEvaluator>().AsSingle();
        Container.Bind<PhonePartsDatabase>().FromMethod(_ => ResolveDatabase()).AsSingle();
        Container.Bind<IInstallPreviewMaterialSource>().To<InstallPreviewMaterialSourceFromDatabase>().AsSingle();
        Container.Bind<IPhonePartDownRayHitSelector>().To<PhonePartDownRayHitSelector>().AsSingle();
        Container.Bind<IPhonePartSlotInstallPreviewSync>().To<PhonePartSlotInstallPreviewSync>().AsSingle();
        Container.Bind<IPhonePartInstallMotion>().To<PhonePartInstallMotion>().AsSingle();
        Container.Bind<IPhonePartRigidbodyService>().To<PhonePartRigidbodyService>().AsSingle();
        Container.Bind<PhoneSlotProbeSphere>().FromInstance(_slotProbeSphere).AsSingle();
    }

    /// <summary>
    /// Загружает базу из Resources (fail-fast при отсутствии).
    /// </summary>
    private static PhonePartsDatabase ResolveDatabase()
    {
        var db = PhonePartsDatabaseAccess.TryGetRuntime();
        if (db == null)
            throw new ZenjectException(string.Empty);

        return db;
    }
}
