﻿namespace SoundFingerprinting.Audio.NAudio.Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    [TestFixture]
    public class BlockingQueueSamplesProviderTest
    {
        private readonly Random random = new Random();

        [Test]
        public void ShouldGetNextSamples()
        {
            var producer = new BlockingCollection<float[]>();
            const int numberOfItemsToAdd = 5;
            PutSamplesIntoQueueOnIrregularIntervals(producer, numberOfItemsToAdd);

            var samplesProvider = new BlockingQueueSamplesProvider(producer);

            int count = 0;
            while (!producer.IsAddingCompleted)
            {
                var added = samplesProvider.GetNextSamples(new float[1024]);
                if (count < numberOfItemsToAdd)
                {
                    Assert.AreEqual(1024 * 4, added);
                }

                count++;
            }

            Assert.AreEqual(numberOfItemsToAdd + 1, count);
        }

        private void PutSamplesIntoQueueOnIrregularIntervals(BlockingCollection<float[]> producer, int count)
        {
            Task.Factory.StartNew(() => AddValues(producer, count)).ContinueWith(t => producer.CompleteAdding());
        }

        private void AddValues(BlockingCollection<float[]> producer, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                Thread.Sleep(random.Next(1000, 3000));
                producer.Add(new float[1024]);
            }
        }
    }
}
