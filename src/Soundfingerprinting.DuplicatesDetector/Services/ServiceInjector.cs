namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.DuplicatesDetector.DataAccess;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.FFT.FFTW;
    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.LSH;
    using Soundfingerprinting.Hashing.MinHash;

    /// <summary>
    ///   Service injector loads all the services into Service Container on Application startup
    /// </summary>
    /// <remarks>
    ///   Dependency injection with Service Locator
    /// </remarks>
    public static class ServiceInjector
    {
        /// <summary>
        ///   Add bindings that will be applied within the application runtime
        /// </summary>
        public static void InjectServices()
        {
             const string PathToPermutations = "perms.csv";
             const string Separator = ",";

            ServiceContainer.Kernel.Bind<IFolderBrowserDialogService>().To<FolderBrowserDialogService>();
            ServiceContainer.Kernel.Bind<IMessageBoxService>().To<MessageBoxService>();
            ServiceContainer.Kernel.Bind<IOpenFileDialogService>().To<OpenFileDialogService>();
            ServiceContainer.Kernel.Bind<ISaveFileDialogService>().To<SaveFileDialogService>();
            ServiceContainer.Kernel.Bind<IWindowService>().To<WindowService>();
            ServiceContainer.Kernel.Bind<IGenericViewWindow>().To<GenericViewWindowService>();
            ServiceContainer.Kernel.Bind<IStorage>().To<RamStorage>();
            ServiceContainer.Kernel.Bind<IFingerprintingUnitsBuilder>().To<FingerprintingUnitsBuilder>();
            ServiceContainer.Kernel.Bind<IPermutations>()
                            .To<LocalPermutations>()
                            .WithConstructorArgument("pathToPermutations", PathToPermutations)
                            .WithConstructorArgument("separator", Separator);

            ServiceContainer.Kernel.Bind<IAudioService, IExtendedAudioService, ITagService>().To<BassAudioService>().InSingletonScope();
            ServiceContainer.Kernel.Bind<IFingerprintService>().To<FingerprintService>();
            ServiceContainer.Kernel.Bind<IFFTService>().To<CachedFFTWService>();
            ServiceContainer.Kernel.Bind<IFingerprintDescriptor>().To<FingerprintDescriptor>();
            ServiceContainer.Kernel.Bind<IWaveletDecomposition>().To<StandardHaarWaveletDecomposition>();
            ServiceContainer.Kernel.Bind<IFingerprintingConfiguration>().To<DefaultFingerprintingConfiguration>();
            ServiceContainer.Kernel.Bind<IWaveletService>().To<WaveletService>();
            ServiceContainer.Kernel.Bind<ISpectrumService>().To<SpectrumService>();
            ServiceContainer.Kernel.Bind<IMinHashService>().To<MinHashService>();
            ServiceContainer.Kernel.Bind<ILSHService>().To<LSHService>();
            ServiceContainer.Kernel.Bind<ICombinedHashingAlgoritm>().To<CombinedHashingAlgorithm>();
        }
    }
}