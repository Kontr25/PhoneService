using Zenject;

namespace Pools
{
    /// <summary>
    /// Installer регистрации зависимостей пула в контейнере Zenject.
    /// </summary>
    public sealed class PoolInstaller : MonoInstaller
    {
        /// <inheritdoc />
        public override void InstallBindings()
        {
            Container.Bind<IPoolElementFactory>().To<ZenjectPoolElementFactory>().AsSingle();
        }
    }
}
