namespace Soundfingerprinting.SoundTools
{
    using System;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.FFT.FFTW;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.Windows;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Image;
    using Soundfingerprinting.SoundTools.DI;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            IDependencyResolver dependencyResolver = new NinjectDependencyResolver();
            dependencyResolver.Bind<IDependencyResolver>().ToConstant(dependencyResolver);
            dependencyResolver.Bind<IFingerprintService>().To<FingerprintService>();
            dependencyResolver.Bind<IWindowFunction>().To<HanningWindow>();
            dependencyResolver.Bind<IWaveletDecomposition>().To<DiscreteHaarWaveletDecomposition>();
            dependencyResolver.Bind<IFingerprintDescriptor>().To<FingerprintDescriptor>();
            dependencyResolver.Bind<IFingerprintingConfiguration>().To<DefaultFingerprintingConfiguration>();
            dependencyResolver.Bind<IAudioService>().To<BassAudioService>();
            dependencyResolver.Bind<IExtendedAudioService>().To<BassAudioService>();
            dependencyResolver.Bind<IFFTService>().To<CachedFFTWService>();
            dependencyResolver.Bind<ITagService>().To<TagService>();
            dependencyResolver.Bind<IWorkUnitBuilder>().To<WorkUnitBuilder>();
            dependencyResolver.Bind<IDatabaseProviderFactory>().To<MsSqlDatabaseProviderFactory>();
            dependencyResolver.Bind<IConnectionStringFactory>().To<DefaultConnectionStringFactory>();
            dependencyResolver.Bind<IModelBinderFactory>().To<CachedModelBinderFactory>();
            dependencyResolver.Bind<IModelBinderFactory>().To<ModelBinderFactory>().WhenInjectedInto<CachedModelBinderFactory>();
            dependencyResolver.Bind<IModelService>().To<ModelService>();
            dependencyResolver.Bind<IPermutationGeneratorService>().To<PermutationGeneratorService>();
            dependencyResolver.Bind<IImageService>().To<ImageService>();
            dependencyResolver.Bind<ISpectrumService>().To<SpectrumService>();
            dependencyResolver.Bind<IWaveletService>().To<WaveletService>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(dependencyResolver.Get<WinMain>());
        }
    }
}