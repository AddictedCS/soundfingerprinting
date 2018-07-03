namespace SoundFingerprinting.Tests.Unit.FFT
{
    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;

    [TestFixture]
    public class LogUtilityTest : AbstractTest
    {
        private readonly ILogUtility logUtility = new LogUtility();
        private readonly FingerprintConfiguration defaultFingerprintConfiguration = new DefaultFingerprintConfiguration();

        [Test]
        public void FrequencyToIndexTest()
        {
            int result = logUtility.FrequencyToSpectrumIndex(318, 5512, 2048);

            Assert.AreEqual((318 * 1024) / (5512 / 2), result);
        }

        [Test]
        public void GenerateLogFrequenciesRangesTest()
        {
            var defaultConfig = new DefaultSpectrogramConfig { UseDynamicLogBase = false, LogBase = 10 };
            float[] logSpacedFrequencies = {
                    318.00f, 336.81f, 356.73f, 377.83f, 400.18f, 423.85f, 448.92f, 475.47f, 503.59f, 533.38f, 564.92f,
                    598.34f, 633.73f, 671.21f, 710.91f, 752.96f, 797.50f, 844.67f, 894.63f, 947.54f, 1003.58f, 1062.94f,
                    1125.81f, 1192.40f, 1262.93f, 1337.63f, 1416.75f, 1500.54f, 1589.30f, 1683.30f, 1782.86f, 1888.31f,
                    2000f
                };

            ushort[] indexes = logUtility.GenerateLogFrequenciesRanges(defaultFingerprintConfiguration.SampleRate, defaultConfig);

            for (int i = 0; i < logSpacedFrequencies.Length; i++)
            {
                var logSpacedFrequency = logSpacedFrequencies[i];
                int index = logUtility.FrequencyToSpectrumIndex(logSpacedFrequency, defaultFingerprintConfiguration.SampleRate, defaultConfig.WdftSize);
                Assert.AreEqual(index, indexes[i]);
            }
        }
    }
}
