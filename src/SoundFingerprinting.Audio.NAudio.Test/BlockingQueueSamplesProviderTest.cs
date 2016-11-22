namespace SoundFingerprinting.Audio.NAudio.Test
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
        public void TestGetNextSamples()
        {
            var producer = new BlockingCollection<float[]>();
            const int NumberOfItemsToAdd = 5;
            PutSamplesIntoQueueOnUnregularIntervals(producer, NumberOfItemsToAdd);

            var samplesProvider = new BlockingQueueSamplesProvider(producer);

            int count = 0;
            while (!producer.IsAddingCompleted)
            {
                var added = samplesProvider.GetNextSamples(new float[1024]);
                if (count < NumberOfItemsToAdd)
                {
                    Assert.AreEqual(1024 * 4, added);
                }

                count++;
            }

            Assert.AreEqual(NumberOfItemsToAdd + 1, count);
        }

        private void PutSamplesIntoQueueOnUnregularIntervals(BlockingCollection<float[]> producer, int count)
        {
            Task.Factory.StartNew(
                () =>
                    {
                        Parallel.For(
                            0,
                            count,
                            index =>
                                {
                                    Thread.Sleep(random.Next(1000, 3000));
                                    producer.Add(new float[1024]);
                                });
                    }).ContinueWith(t => producer.CompleteAdding());
        }
    }
}
