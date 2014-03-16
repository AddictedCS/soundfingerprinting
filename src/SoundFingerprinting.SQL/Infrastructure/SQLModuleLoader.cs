namespace SoundFingerprinting.SQL.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.SQL.Connection;
    using SoundFingerprinting.SQL.ORM;

    internal class SQLModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            kernel.Bind<IModelBinderFactory>().To<CachedModelBinderFactory>().InSingletonScope();
            kernel.Bind<IModelBinderFactory>().To<ModelBinderFactory>()
                                              .WhenInjectedInto<CachedModelBinderFactory>();

            kernel.Bind<IDatabaseProviderFactory>().To<MsSqlDatabaseProviderFactory>();
            kernel.Bind<IConnectionStringFactory>().To<DefaultConnectionStringFactory>().InSingletonScope();
        }
    }
}
