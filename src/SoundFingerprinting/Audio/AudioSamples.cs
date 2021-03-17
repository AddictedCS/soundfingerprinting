namespace SoundFingerprinting.Audio
{
    using System;
    using SoundFingerprinting.Command;

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

       
        public AudioSamples(float[] samples, string origin, int sampleRate, DateTime relativeTo) : this(samples, origin, sampleRate, relativeTo, 0d)
        {
            // no op
        }
        
        /// <summary>
        ///  Creates new instance of AudioSamples class.
        /// </summary>
        /// <param name="samples">Audio samples.</param>
        /// <param name="origin">Source origin (i.e., filename, URI).</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="relativeTo">Relative to a particular date time offset.</param>
        /// <param name="offset">Offset of the captured audio samples.</param>
        public AudioSamples(float[] samples, string origin, int sampleRate, DateTime relativeTo, double offset)
        {
            Samples = samples;
            Origin = origin;
            SampleRate = sampleRate;
            RelativeTo = relativeTo;
            Offset = offset;
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
        ///  Gets the duration in seconds of the audio samples
        /// </summary>
        public double Duration => (double)Samples.Length / SampleRate;

        /// <summary>
        ///  Gets of offset of the captured audio samples relative to the previous capture.
        /// </summary>
        /// <remarks>
        ///  Used by <see cref="RealtimeQueryCommand"/>, 0 in normal request/reply scenarios.
        /// </remarks>
        public double Offset { get; }
    }
}
