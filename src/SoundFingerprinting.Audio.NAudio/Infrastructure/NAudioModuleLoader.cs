namespace SoundFingerprinting.Audio.NAudio.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Audio.NAudio.Play;
    using SoundFingerprinting.Infrastructure;

    internal class NAudioModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            kernel.Bind<INAudioPlayAudioFactory>().To<NAudioPlayAudioFactory>().InSingletonScope();
            kernel.Bind<INAudioFactory>().To<NAudioFactory>().InSingletonScope();
            kernel.Bind<INAudioSourceReader>().To<NAudioSourceReader>().InSingletonScope();
        }
    }
}
