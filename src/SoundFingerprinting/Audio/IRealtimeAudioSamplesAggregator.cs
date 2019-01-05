namespace SoundFingerprinting.Audio
{
    using System;
    using SoundFingerprinting.Strides;

    public interface IRealtimeAudioSamplesAggregator
    {
        AudioSamples Aggregate(AudioSamples chunk);
        
        IStride Stride { get; }
        
        int MinSize { get; }
    }

    public class RealtimeAudioSamplesAggregator : IRealtimeAudioSamplesAggregator
    {
        private readonly float[] buffer;
        private int left;
        private readonly object lockObject = new object();
        private long inc = -1;
        
        public RealtimeAudioSamplesAggregator(IStride stride, int minSize)
        {
            Stride = stride;
            MinSize = minSize;
            buffer = new float[minSize];
        }
        
        public AudioSamples Aggregate(AudioSamples chunk)
        {
            if (chunk.Samples.Length < MinSize)
            {
                throw new ArgumentException($"{nameof(chunk)} cannot be less than {MinSize}");
            }

            lock (lockObject)
            {
                inc++;
                if (inc == 0)
                {
                    Cache(chunk);
                    return chunk;
                }

                var cached = Copy(chunk);
                Cache(chunk);
                return new AudioSamples(cached, chunk.Origin, chunk.SampleRate);
            }
        }

        private float[] Copy(AudioSamples chunk)
        {
            float[] withCached = new float[left + chunk.Samples.Length];
            Buffer.BlockCopy(buffer, 0, withCached, 0, sizeof(float) * left);
            Buffer.BlockCopy(chunk.Samples, 0, withCached, sizeof(float) * left, sizeof(float) * chunk.Samples.Length);
            return withCached;
        }

        private void Cache(AudioSamples chunk)
        {
            left = MinSize - Stride.NextStride;
            Buffer.BlockCopy(chunk.Samples, sizeof(float) * (chunk.Samples.Length - left), buffer, 0, sizeof(float) * left);
        }

        public IStride Stride { get; }
        
        public int MinSize { get; }
    }
}