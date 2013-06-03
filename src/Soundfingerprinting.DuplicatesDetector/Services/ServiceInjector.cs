namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.DuplicatesDetector.DataAccess;
    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Infrastructure;

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
            ServiceContainer.Kernel.Bind<IExtendedAudioService, ITagService>().To<BassAudioService>().InSingletonScope();
            ServiceContainer.Kernel.Bind<ICombinedHashingAlgoritm>().To<CombinedHashingAlgorithm>();

            DependencyResolver.Current.Bind<IPermutations, LocalPermutations>(new LocalPermutations(PathToPermutations, Separator));
        }
    }
}