namespace SoundFingerprinting.Audio.NAudio
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;

    public class BlockingQueueSamplesProvider : ISamplesProvider
    {
        private readonly BlockingCollection<float[]> producer;

        public BlockingQueueSamplesProvider(BlockingCollection<float[]> producer)
        {
            this.producer = producer;
        }

        public int GetNextSamples(float[] buffer)
        {
            try
            {
                float[] samples = producer.Take();
                Array.Copy(samples, buffer, samples.Length);
                return samples.Length * 4;
            }
            catch (InvalidOperationException e)
            {
                // thrown when collection is marked as not allowing more additions
                Trace.WriteLine(e.Message);
            }

            return 0;
        }
    }
}
