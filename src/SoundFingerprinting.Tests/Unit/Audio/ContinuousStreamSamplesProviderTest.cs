namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class ContinuousStreamSamplesProviderTest
    {
        private ContinuousStreamSamplesProvider samplesProvider;

        [Test]
        public void TestGetNextSamples()
        {
            float[] buffer = new float[1024];
            var queue =
                new Queue<float[]>(
                    new[]
                        {
                            new float[1024], new float[0], new float[0], new float[512], new float[0], new float[0],
                            new float[0], new float[256]
                        });

            samplesProvider = new ContinuousStreamSamplesProvider(new QueueSamplesProvider(queue));

            int[] expectedResults = { 1024, 512, 256 };
            for (int i = 0; i < 3; i++)
            {
                var result = samplesProvider.GetNextSamples(buffer);
                Assert.AreEqual(expectedResults[i], result / 4);
            }
        }
    }
}
