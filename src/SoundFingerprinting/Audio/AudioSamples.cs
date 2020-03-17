namespace SoundFingerprinting.Audio
{
    using System;

    /// <summary>
    ///  Audio samples that can be used for fingerprinting
    /// </summary>
    [Serializable]
    public class AudioSamples
    {
        public AudioSamples(float[] samples, string origin, int sampleRate) : this(samples, origin, sampleRate, DateTime.Now.AddSeconds(-(double)samples.Length / sampleRate))
        {
            // no op
        }

        public AudioSamples(float[] samples, string origin, int sampleRate, DateTime relativeTo)
        {
            Samples = samples;
            Origin = origin;
            SampleRate = sampleRate;
            RelativeTo = relativeTo;
        }

        private AudioSamples()
        {
            // left for serializers
        }

        /// <summary>
        ///  Gets audio samples in Ieee32 format
        /// </summary>
        public float[] Samples { get; }

        /// <summary>
        ///  Gets the origin of the audio samples
        /// </summary>
        public string Origin { get; }

        /// <summary>
        ///  Gets sample rate at which the audio has been sampled
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        ///  Gets relative to time location when the audio samples have been generated
        /// </summary>
        public DateTime RelativeTo { get; }

        /// <summary>
        ///  Gets the duration of the audio samples
        /// </summary>
        public double Duration => (double)Samples.Length / SampleRate;
    }
}
