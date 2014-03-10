namespace SoundFingerprinting.Audio.NAudio
{
    using System;
    using System.Collections.Concurrent;

    public class BlockingQueueSamplesProvider : ISamplesProvider
    {
        private readonly BlockingCollection<float[]> producer;

        public BlockingQueueSamplesProvider(BlockingCollection<float[]> producer)
        {
            this.producer = producer;
        }

        public int GetNextSamples(float[] buffer)
        {
            float[] samples = producer.Take();
            Array.Copy(samples, buffer, samples.Length);
            return samples.Length * 4;
        }
    }
}
