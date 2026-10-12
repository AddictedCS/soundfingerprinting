namespace SoundFingerprinting.Video
{
    using SoundFingerprinting.Configuration.Frames;
    using SoundFingerprinting.Media;

    /// <summary>
    ///  Class that holds all the required information for <see cref="IMediaService"/> or <see cref="IVideoService"/> to read frames from the underlying source.
    /// </summary>
    public class VideoTrackReadConfiguration
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="VideoTrackReadConfiguration"/> class.
        /// </summary>
        /// <param name="height">Required height of the image frame.</param>
        /// <param name="width">Required width of the image frame.</param>
        /// <param name="frameRate">Frame rate at which to read the frames from the source.</param>
        /// <param name="blackFramesFilterConfiguration">Black frames filter configuration.</param>
        /// <param name="croppingConfiguration">Cropping configuration.</param>
        /// <param name="additionalFilters">Additional filters to apply when reading from the source.</param>
        /// <param name="frameCropSink">Sink that takes crops of a frame region while the source is read, or null when nothing is cropped.</param>
        public VideoTrackReadConfiguration(int height, int width, int frameRate, BlackFramesFilterConfiguration blackFramesFilterConfiguration, CroppingConfiguration croppingConfiguration, string additionalFilters, IFrameCropSink? frameCropSink = null)
        {
            Height = height;
            Width = width;
            FrameRate = frameRate;
            BlackFramesFilterConfiguration = blackFramesFilterConfiguration;
            CroppingConfiguration = croppingConfiguration;
            AdditionalFilters = additionalFilters;
            FrameCropSink = frameCropSink;
        }

        /// <summary>
        ///  Gets the required height of the image frame.
        /// </summary>
        public int Height { get; }

        /// <summary>
        ///  Gets the required width of the image frame.
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///  Gets the framerate at which to read the frames from the source.
        /// </summary>
        public int FrameRate { get; }

        /// <summary>
        ///  Gets black frames filter configuration.
        /// </summary>
        /// <remarks>
        ///  Frames that are identified as black will be removed from the resulting set.
        /// </remarks>
        public BlackFramesFilterConfiguration BlackFramesFilterConfiguration { get; }

        /// <summary>
        ///  Gets cropping configuration.
        /// </summary>
        /// <remarks>
        ///  Cropping configuration will be used to identify outer regions of the image to crop during processing.
        /// </remarks>
        public CroppingConfiguration CroppingConfiguration { get; }

        /// <summary>
        ///  Gets additional filters to apply when reading from the source.
        /// </summary>
        public string AdditionalFilters { get; }

        /// <summary>
        ///  Gets the sink that takes crops of a frame region while the source is read, or null when nothing is cropped.
        /// </summary>
        /// <remarks>
        ///  A media service that supports it reads <see cref="IFrameCropSink.Region"/> once at the start of every source and crops the decoded frames at the region's rate.
        /// </remarks>
        public IFrameCropSink? FrameCropSink { get; }
    }
}