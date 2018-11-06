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
        ///  Gets the duration of the audio samples
        /// </summary>
        public double Duration => (double)Samples.Length / SampleRate;
    }
}
