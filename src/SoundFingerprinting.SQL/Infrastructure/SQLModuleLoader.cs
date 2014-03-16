namespace SoundFingerprinting.SQL.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Infrastructure;

    public class SQLModuleLoader : IModuleLoader
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
