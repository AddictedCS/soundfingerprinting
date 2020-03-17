namespace SoundFingerprinting.Audio
{
    using System;
    using SoundFingerprinting.Strides;

    public class RealtimeAudioSamplesAggregator : IRealtimeAudioSamplesAggregator
    {
        private readonly float[] buffer;
        private int left;
        private DateTime relativeTo;
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
                return cached;
            }
        }

        private AudioSamples Copy(AudioSamples chunk)
        {
            float[] withCached = new float[left + chunk.Samples.Length];
            Buffer.BlockCopy(buffer, 0, withCached, 0, sizeof(float) * left);
            Buffer.BlockCopy(chunk.Samples, 0, withCached, sizeof(float) * left, sizeof(float) * chunk.Samples.Length);
            return new AudioSamples(withCached, chunk.Origin, chunk.SampleRate, relativeTo);
        }

        private void Cache(AudioSamples chunk)
        {
            int nextStride = Stride.NextStride;
            left = chunk.Samples.Length - nextStride;
            relativeTo = chunk.RelativeTo.AddSeconds((double)(nextStride) / chunk.SampleRate);
            Buffer.BlockCopy(chunk.Samples, sizeof(float) * (nextStride), buffer, 0, sizeof(float) * left);
        }

        public IStride Stride { get; }
        
        public int MinSize { get; }
    }
}