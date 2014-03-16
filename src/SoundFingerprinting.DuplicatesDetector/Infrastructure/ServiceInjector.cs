namespace SoundFingerprinting.DuplicatesDetector.Infrastructure
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DuplicatesDetector.Services;
    using SoundFingerprinting.InMemory;

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

            ServiceContainer.Kernel.Bind<IFingerprintCommandBuilder>().To<FingerprintCommandBuilder>().InSingletonScope();
            ServiceContainer.Kernel.Bind<IQueryFingerprintService>().To<QueryFingerprintService>().InSingletonScope();
            ServiceContainer.Kernel.Bind<DuplicatesDetectorFacade>().ToSelf().InSingletonScope();
            ServiceContainer.Kernel.Bind<TrackHelper>().ToSelf().InSingletonScope();
            ServiceContainer.Kernel.Bind<IAudioService>().To<BassAudioService>().InSingletonScope();
            ServiceContainer.Kernel.Bind<IPlayAudioFileService>().To<BassPlayAudioFileService>().InSingletonScope();
            ServiceContainer.Kernel.Bind<ITagService>().To<BassTagService>().InSingletonScope();
            ServiceContainer.Kernel.Bind<IModelService>().To<InMemoryModelService>().InSingletonScope();
        }
    }
}