namespace SoundFingerprinting.Audio.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Infrastructure;

    internal class AudioModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            kernel.Bind<ISamplesAggregator>().To<SamplesAggregator>().InSingletonScope();
            kernel.Bind<IAudioSamplesNormalizer>().To<AudioSamplesNormalizer>().InSingletonScope();
        }
    }
}
