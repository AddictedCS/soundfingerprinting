namespace SoundFingerprinting.MongoDb.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;

    public class MongoDbModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            kernel.Bind<IMongoDbConnectionStringFactory>().To<MongoDbConnectionStringFactory>().InSingletonScope();
            kernel.Bind<IMongoDatabaseProviderFactory>().To<MongoDatabaseProviderFactory>().InSingletonScope();
        }
    }
}
