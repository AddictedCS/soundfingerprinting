namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;

    internal class QueueSamplesProvider : ISamplesProvider
    {
        private readonly Queue<float[]> samples; 

        public QueueSamplesProvider(Queue<float[]> samples)
        {
            this.samples = samples;
        }

        public int GetNextSamples(float[] buffer)
        {
            float[] toCopy = samples.Dequeue();
            Array.Copy(toCopy, buffer, toCopy.Length);
            return toCopy.Length * 4;
        }
    }
}