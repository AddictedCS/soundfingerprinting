namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;

    [TestClass]
    public class ContinuousStreamSamplesProviderTest
    {
        private ContinuousStreamSamplesProvider samplesProvider;

        [TestMethod]
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

            int[] expectedResults = new[] { 1024, 512, 256 };
            for (int i = 0; i < 3; i++)
            {
                var result = samplesProvider.GetNextSamples(buffer);
                Assert.AreEqual(expectedResults[i], result / 4);
            }
        }
    }
}
