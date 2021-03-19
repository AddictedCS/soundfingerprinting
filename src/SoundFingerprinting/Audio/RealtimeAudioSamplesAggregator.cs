namespace SoundFingerprinting.Audio
{
    using System;
    using SoundFingerprinting.Strides;

    public class RealtimeAudioSamplesAggregator : IRealtimeAudioSamplesAggregator
    {
        private readonly float[] tailBuffer;
        private int tailLength;
        private DateTime relativeTo;
        private long inc = -1;
        
        public RealtimeAudioSamplesAggregator(IStride stride, int minSize)
        {
            Stride = stride;
            MinSize = minSize;
            
            tailBuffer = new float[minSize];
        }

        public AudioSamples Aggregate(AudioSamples chunk)
        {
            if (chunk.Samples.Length < MinSize)
            {
                throw new ArgumentException($"{nameof(chunk)} cannot be less than {MinSize}");
            }

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

        private AudioSamples Copy(AudioSamples chunk)
        {
            float[] withCached = new float[tailLength + chunk.Samples.Length];
            Buffer.BlockCopy(tailBuffer, 0, withCached, 0, sizeof(float) * tailLength);
            Buffer.BlockCopy(chunk.Samples, 0, withCached, sizeof(float) * tailLength, sizeof(float) * chunk.Samples.Length);
            return new AudioSamples(withCached, chunk.Origin, chunk.SampleRate, relativeTo);
        }

        private void Cache(AudioSamples chunk)
        {
            int nextStride = Stride.NextStride;
            tailLength = tailBuffer.Length - nextStride;
            relativeTo = chunk.RelativeTo.AddSeconds((double)(chunk.Samples.Length - tailLength) / chunk.SampleRate);
            Buffer.BlockCopy(chunk.Samples,  sizeof(float) * (chunk.Samples.Length - tailLength), tailBuffer, 0, sizeof(float) * tailLength);
        }

        public IStride Stride { get; }
        
        public int MinSize { get; }
    }
}