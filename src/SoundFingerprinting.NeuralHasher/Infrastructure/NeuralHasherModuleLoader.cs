namespace SoundFingerprinting.NeuralHasher.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.NeuralHasher.Utils;

    internal class NeuralHasherModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            kernel.Bind<IBinaryOutputHelper>().To<BinaryOutputHelper>().InSingletonScope();
            kernel.Bind<INetworkFactory>().To<NetworkFactory>().InSingletonScope();
            kernel.Bind<IDynamicReorderingAlgorithm>().To<DynamicReorderingAlgorithm>().InSingletonScope();
        }
    }
}
