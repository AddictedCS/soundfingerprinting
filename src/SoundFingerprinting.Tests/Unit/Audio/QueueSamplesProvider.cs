namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;

    public class QueueSamplesProvider : ISamplesProvider
    {
        private readonly Queue<int> queue;

        public QueueSamplesProvider(Queue<int> queue)
        {
            this.queue = queue;
        }

        public int GetNextSamples(float[] buffer)
        {
            return queue.Dequeue();
        }
    }
}