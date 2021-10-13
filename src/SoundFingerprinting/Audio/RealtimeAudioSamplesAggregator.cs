namespace SoundFingerprinting.Audio
{
    using System;
    using SoundFingerprinting.Strides;

    /// <summary>
    ///  Realtime samples aggregator.
    /// </summary>
    public class RealtimeAudioSamplesAggregator : IRealtimeAudioSamplesAggregator
    {
        private const int MinSizeOfOneFingerprint = 128 * 64 + 2048;
        private float[] tail;
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="RealtimeAudioSamplesAggregator"/> class.
        /// </summary>
        /// <param name="stride">Stride to use between consecutive fingerprints.</param>
        public RealtimeAudioSamplesAggregator(IStride stride)
        {
            Stride = stride;
            tail = Array.Empty<float>();
        }

        private IStride Stride { get; }
        
        /// <inheritdoc cref="IRealtimeAudioSamplesAggregator.Aggregate"/>
        public AudioSamples? Aggregate(AudioSamples chunk)
        {
            var withPreviousTail = AttachNewChunk(chunk);
            CacheTail(withPreviousTail);
            return withPreviousTail.Samples.Length >= MinSizeOfOneFingerprint ? withPreviousTail : null;
        }

        private AudioSamples AttachNewChunk(AudioSamples chunk)
        {
            float[] prefixed = new float[tail.Length + chunk.Samples.Length];
            Buffer.BlockCopy(tail, 0, prefixed, 0, sizeof(float) * tail.Length);
            Buffer.BlockCopy(chunk.Samples, 0, prefixed, sizeof(float) *  tail.Length, sizeof(float) * chunk.Samples.Length);
            var samples = new AudioSamples(prefixed, chunk.Origin, chunk.SampleRate, chunk.RelativeTo.AddSeconds(-(float)tail.Length / chunk.SampleRate));
            tail = prefixed;
            return samples;
        }

        private void CacheTail(AudioSamples samples)
        {
            if (samples.Samples.Length >= MinSizeOfOneFingerprint)
            {
                int nextStride = Stride.NextStride;
                if (nextStride < MinSizeOfOneFingerprint)
                {
                    int tailSize = MinSizeOfOneFingerprint - nextStride;
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