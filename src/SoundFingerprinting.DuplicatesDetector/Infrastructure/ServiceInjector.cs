namespace SoundFingerprinting.DuplicatesDetector.Infrastructure
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.RAM;
    using SoundFingerprinting.DuplicatesDetector.Services;
    using SoundFingerprinting.Infrastructure;

    public static class ServiceInjector
    {
        public static void InjectServices()
        {
            ServiceContainer.Kernel.Bind<IFolderBrowserDialogService>().To<FolderBrowserDialogService>();
            ServiceContainer.Kernel.Bind<IMessageBoxService>().To<MessageBoxService>();
            ServiceContainer.Kernel.Bind<IOpenFileDialogService>().To<OpenFileDialogService>();
            ServiceContainer.Kernel.Bind<ISaveFileDialogService>().To<SaveFileDialogService>();
            ServiceContainer.Kernel.Bind<IWindowService>().To<WindowService>();
            ServiceContainer.Kernel.Bind<IGenericViewWindow>().To<GenericViewWindowService>();

            DependencyResolver.Current.BindAsSingleton<IModelService, InMemoryModelService>();

            ServiceContainer.Kernel.Bind<DuplicatesDetectorService>().ToMethod(
                context =>
                new DuplicatesDetectorService(
                    DependencyResolver.Current.Get<IModelService>(),
                    DependencyResolver.Current.Get<IFingerprintCommandBuilder>(),
                    DependencyResolver.Current.Get<IQueryFingerprintService>())).InSingletonScope();

            ServiceContainer.Kernel.Bind<DuplicatesDetectorFacade>().ToSelf().InSingletonScope();
            ServiceContainer.Kernel.Bind<TrackHelper>().ToSelf().InSingletonScope();
            ServiceContainer.Kernel.Bind<ITagService, IAudioService, IExtendedAudioService>().To<BassAudioService>();
        }
    }
}