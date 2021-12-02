namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Query services selector.
    /// </summary>
    public interface IUsingQueryServices : IQueryCommand
    {
        /// <summary>
        ///  Sets model service that will be used for querying the data source.
        /// </summary>
        /// <param name="modelService">Model service used as access interfaces to underlying fingerprints storage.</param>
        /// <returns>Query command.</returns>
        IQueryCommand UsingServices(IModelService modelService);
            
        /// <summary>
        /// Sets model service as well as audio service using in querying the source.
        /// </summary>
        /// <param name="modelService">Model service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="audioService">Audio service used in building the fingerprints from the source.</param>
        /// <returns>Query command.</returns>
        IQueryCommand UsingServices(IModelService modelService, IAudioService audioService);

        /// <summary>
        /// Sets model service as well as audio service using in querying the source.
        /// </summary>
        /// <param name="modelService">Model service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="audioService">Audio service used in building the fingerprints from the source.</param>
        /// <param name="queryMatchRegistry">Match registry used to store the results in a separate storage.</param>
        /// <returns>Query command.</returns>
        IQueryCommand UsingServices(IModelService modelService, IAudioService audioService, IQueryMatchRegistry queryMatchRegistry);

        /// <summary>
        ///  Sets model service as well as video service.
        /// </summary>
        /// <param name="modelService">Model service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="videoService">Video service that will be used for reading <see cref="Frames"/> from the underlying source.</param>
        /// <returns>Query command.</returns>
        /// <remarks>
        ///  Set video service in case you want to generate video fingerprints only by setting MediaType.Video on the <see cref="IQuerySource"/> overloads.
        /// </remarks>
        IQueryCommand UsingServices(IModelService modelService, IVideoService videoService);

        /// <summary>
        ///  Sets model service as well as video service.
        /// </summary>
        /// <param name="modelService">Model service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="videoService">Video service that will be used for reading <see cref="Frames"/> from the underlying source.</param>
        /// <param name="queryMatchRegistry">Match registry used to store the results in a separate storage.</param>
        /// <returns>Query command.</returns>
        /// <remarks>
        ///  Set video service in case you want to generate video fingerprints only by setting MediaType.Video on the <see cref="IQuerySource"/> overloads.
        /// </remarks>
        IQueryCommand UsingServices(IModelService modelService, IVideoService videoService, IQueryMatchRegistry queryMatchRegistry);
        
        /// <summary>
        /// Sets model service as well as media service.
        /// </summary>
        /// <param name="modelService">Model service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="mediaService">Media service that will be used to read <see cref="AudioSamples"/> or <see cref="Frames"/> or both, from the underlying source.</param>
        /// <returns>Query command.</returns>
        /// <remarks>
        ///  Media service can be used to read both <see cref="AudioSamples"/> and <see cref="Frames"/> from a media file, and generate <see cref="AVHashes"/> that will be used to query the underlying source.
        /// </remarks>
        IQueryCommand UsingServices(IModelService modelService, IMediaService mediaService);

        /// <summary>
        /// Sets model service as well as media service.
        /// </summary>
        /// <param name="modelService">Model service used as access interfaces to underlying fingerprints storage.</param>
        /// <param name="mediaService">Media service that will be used to read <see cref="AudioSamples"/> or <see cref="Frames"/> or both, from the underlying source.</param>
        /// <param name="queryMatchRegistry">Match registry used to store the results in a separate storage.</param>
        /// <returns>Query command.</returns>
        /// <remarks>
        ///  Media service can be used to read both <see cref="AudioSamples"/> and <see cref="Frames"/> from a media file, and generate <see cref="AVHashes"/> that will be used to query the underlying source.
        /// </remarks>
        IQueryCommand UsingServices(IModelService modelService, IMediaService mediaService, IQueryMatchRegistry queryMatchRegistry);
    }
}