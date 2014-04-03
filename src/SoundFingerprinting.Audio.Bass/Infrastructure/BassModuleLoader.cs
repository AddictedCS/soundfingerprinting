namespace SoundFingerprinting.Audio.Bass.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Infrastructure;

    internal class BassModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            kernel.Bind<IBassServiceProxy>().To<BassServiceProxy>().InSingletonScope();
            kernel.Bind<IBassStreamFactory>().To<BassStreamFactory>().InSingletonScope();
            kernel.Bind<IBassResampler>().To<BassResampler>().InSingletonScope();
        }
    }
}
