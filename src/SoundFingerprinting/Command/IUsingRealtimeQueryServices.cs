namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Contract for model service selection during realtime query building.
    /// </summary>
    public interface IUsingRealtimeQueryServices : IRealtimeQueryCommand
    {
        /// <summary>
        ///  Sets the model service that will be used as the data source during query.
        /// </summary>
        /// <param name="queryService">ModelService to query.</param>
        /// <returns>Realtime command.</returns>
        IRealtimeQueryCommand UsingServices(IQueryService queryService);

        /// <summary>
        ///  Sets the model service that will query the datasource, as well as the audio service that will read content to fingerprint.
        /// </summary>
        /// <param name="queryService">ModelService to query.</param>
        /// <param name="audioService">AudioService to use for file processing.</param>
        /// <returns>Realtime command.</returns>
        IRealtimeQueryCommand UsingServices(IQueryService queryService, IAudioService audioService);
        
        /// <summary>
        ///  Sets the model service that will query the datasource, as well as the media service that will read content to fingerprint.
        /// </summary>
        /// <param name="queryService">ModelService to query.</param>
        /// <param name="mediaService">MediaService to use for file processing.</param>
        /// <returns>Realtime command.</returns>
        IRealtimeQueryCommand UsingServices(IQueryService queryService, IMediaService mediaService);
        
        /// <summary>
        ///  Sets the model service that will query the datasource, as well as the video service that will read content to fingerprint.
        /// </summary>
        /// <param name="queryService">ModelService to query.</param>
        /// <param name="videoService">VideoService to use for file processing.</param>
        /// <returns>Realtime command.</returns>
        IRealtimeQueryCommand UsingServices(IQueryService queryService, IVideoService videoService);
        
        /// <summary>
        ///  Sets the model service that will query the datasource, as well as the realtime media service used to create fingerprints directly from a given URL.
        /// </summary>
        /// <param name="queryService">ModelService to query.</param>
        /// <param name="realtimeMediaService">Realtime media service to use.</param>
        /// <returns>Realtime command.</returns>
        IRealtimeQueryCommand UsingServices(IQueryService queryService, IRealtimeMediaService realtimeMediaService);
    }
}