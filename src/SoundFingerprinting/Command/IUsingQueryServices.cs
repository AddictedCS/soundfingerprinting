namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;

    /// <summary>
    ///  Query services selector.
    /// </summary>
    public interface IUsingQueryServices : IQueryCommand
    {
        /// <summary>
        ///  Sets model service that will be used for querying the data source.
        /// </summary>
        /// <param name="modelService">Model Service used as access interfaces to underlying fingerprints storage.</param>
        /// <returns>Query command.</returns>
        IQueryCommand UsingServices(IModelService modelService);
            
        /// <summary>
        /// Sets model service as well as audio service using in querying the source.
        /// </summary>
        /// <param name="modelService">Model Service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="audioService">Audio Service used in building the fingerprints from the source.</param>
        /// <returns>Query command.</returns>
        IQueryCommand UsingServices(IModelService modelService, IAudioService audioService);

        /// <summary>
        /// Sets model service as well as audio service using in querying the source.
        /// </summary>
        /// <param name="modelService">Model Service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="audioService">Audio Service used in building the fingerprints from the source.</param>
        /// <param name="queryMatchRegistry">Match Registry used to store the results in a separate storage.</param>
        /// <returns>Query command.</returns>
        IQueryCommand UsingServices(IModelService modelService, IAudioService audioService, IQueryMatchRegistry queryMatchRegistry);
    }
}