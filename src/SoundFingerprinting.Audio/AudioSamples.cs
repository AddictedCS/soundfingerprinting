namespace SoundFingerprinting.Audio
{
    /// <summary>
    ///  Audio samples that can be used for fingerprinting
    /// </summary>
    public class AudioSamples
    {
        /// <summary>
        ///  Gets or set audio samples
        /// </summary>
        public float[] Samples { get; set; }

        /// <summary>
        /// Gets or sets the origin of the audio samples
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        ///  Gets or sets sample rate at which the audio has been sampled
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        ///  Gets or sets duration of the audio samples
        /// </summary>
        public double Duration { get; set; }
    }
}
