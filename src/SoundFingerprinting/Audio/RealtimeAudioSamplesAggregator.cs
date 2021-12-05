namespace SoundFingerprinting.Audio
{
    using System;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Strides;

    /// <summary>
    ///  Realtime samples aggregator.
    /// </summary>
    public class RealtimeAudioSamplesAggregator : IRealtimeAudioSamplesAggregator
    {
        private readonly int minSamplesPerFingerprint;
        private float[] tail;

        /// <summary>
        ///  Initializes a new instance of the <see cref="RealtimeAudioSamplesAggregator"/> class.
        /// </summary>
        /// <param name="minSamplesPerFingerprint">Minimum number of sampler per one fingerprint (see <see cref="SpectrogramConfig"/>).</param>
        /// <param name="stride">Stride to use between consecutive fingerprints.</param>
        public RealtimeAudioSamplesAggregator(int minSamplesPerFingerprint, IStride stride)
        {
            this.minSamplesPerFingerprint = minSamplesPerFingerprint;
            Stride = stride;
            tail = Array.Empty<float>();
        }

        private IStride Stride { get; }
        
        /// <inheritdoc cref="IRealtimeAudioSamplesAggregator.Aggregate"/>
        public AudioSamples? Aggregate(AudioSamples chunk)
        {
            var withPreviousTail = AttachNewChunk(chunk);
            CacheTail(withPreviousTail);
            return withPreviousTail.Samples.Length >= minSamplesPerFingerprint ? withPreviousTail : null;
        }

        private AudioSamples AttachNewChunk(AudioSamples chunk)
        {
            float[] prefixed = new float[tail.Length + chunk.Samples.Length];
            Buffer.BlockCopy(tail, 0, prefixed, 0, sizeof(float) * tail.Length);
            Buffer.BlockCopy(chunk.Samples, 0, prefixed, sizeof(float) *  tail.Length, sizeof(float) * chunk.Samples.Length);
            var samples = new AudioSamples(prefixed, chunk.Origin, chunk.SampleRate, chunk.RelativeTo.AddSeconds(-(float)tail.Length / chunk.SampleRate), -(double)tail.Length / chunk.SampleRate);
            tail = prefixed;
            return samples;
        }

        private void CacheTail(AudioSamples samples)
        {
            if (samples.Samples.Length >= minSamplesPerFingerprint)
            {
                int nextStride = Stride.NextStride;
                if (nextStride < minSamplesPerFingerprint)
                {
                    // this value is exact when we use IncrementalStaticStride which can tell exactly how much tail length we ignore because it is not evenly divisible by length
                    // Example:
                    // hash   |       |   |   |    = 3 hashes with
                    // q      ------------------
                    // stride |   |   |   |   |
                    // cache               -----
                    int estimatedIgnoredWindow = (samples.Samples.Length - minSamplesPerFingerprint) % nextStride;
                    
                    // tail size is always shorter than minSamplesPerFingerprint
                    int tailSize = minSamplesPerFingerprint - nextStride + estimatedIgnoredWindow;
                    tail = new float[tailSize];
                    Buffer.BlockCopy(samples.Samples, sizeof(float) * (samples.Samples.Length - tailSize), tail, 0, sizeof(float) * tailSize);
                }
                else
                {
                    tail = Array.Empty<float>();
                }
            }
        }
    }
}