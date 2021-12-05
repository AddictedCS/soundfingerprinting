namespace SoundFingerprinting.Media
{
    /// <summary>
    ///  Class that contains all the required information to read audio track with <see cref="IMediaService"/>.
    /// </summary>
    public class AudioTrackReadConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioTrackReadConfiguration"/> class.
        /// </summary>
        /// <param name="sampleRate">Sample rate.</param>
        public AudioTrackReadConfiguration(int sampleRate)
        {
            SampleRate = sampleRate;
        }

        /// <summary>
        ///  Gets sample rate at which to read the audio from the underlying data source.
        /// </summary>
        public int SampleRate { get; }
    }
}