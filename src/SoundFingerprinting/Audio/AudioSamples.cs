namespace SoundFingerprinting.Audio
{
    using System;

    /// <summary>
    ///  Audio samples that can be used for fingerprinting
    /// </summary>
    [Serializable]
    public class AudioSamples
    {
        public AudioSamples(float[] samples, string origin, int sampleRate)
        {
            Samples = samples;
            Origin = origin;
            SampleRate = sampleRate;
        }

        internal AudioSamples()
        {
        }

        /// <summary>
        ///  Gets audio samples in Ieee32 format
        /// </summary>
        public float[] Samples { get; internal set; }

        /// <summary>
        ///  Gets the origin of the audio samples
        /// </summary>
        public string Origin { get; internal set; }

        /// <summary>
        ///  Gets sample rate at which the audio has been sampled
        /// </summary>
        public int SampleRate { get;  internal set; }

        /// <summary>
        ///  Gets the duration of the audio samples
        /// </summary>
        public double Duration
        {
            get
            {
                return (double)Samples.Length / SampleRate;
            }
        }
    }
}
