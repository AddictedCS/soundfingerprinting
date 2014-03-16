namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;

    public interface IUsingQueryServices
    {
        IQueryCommand UsingServices(IModelService modelService, IAudioService audioService);
    }
}