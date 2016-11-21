namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;

    /// <summary>
    ///   Query services selector
    /// </summary>
    public interface IUsingQueryServices
    {
        /// <summary>
        /// Sets model service as well as audio service using in querying the source
        /// </summary>
        /// <param name="modelService">Model Service used as access interfaces to underlying fingerprints storage</param>
        /// <param name="audioService">Audio Service used in building the fingerprints from the source</param>
        /// <returns>Query command</returns>
        IQueryCommand UsingServices(IModelService modelService, IAudioService audioService);
    }
}