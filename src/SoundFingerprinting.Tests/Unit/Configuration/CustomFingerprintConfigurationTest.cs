namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Strides;

    [TestClass]
    public class CustomFingerprintConfigurationTest
    {
        [TestMethod]
        public void FingerprintConfigurationInheritsDefaultValues()
        {
            var customConfiguration = new CustomFingerprintConfiguration();
            var defaultConfiguration = FingerprintConfiguration.Default;

            Assert.AreEqual(defaultConfiguration.NormalizeSignal, customConfiguration.NormalizeSignal);
            Assert.AreEqual(defaultConfiguration.SampleRate, customConfiguration.SampleRate);
            Assert.AreEqual(defaultConfiguration.SamplesPerFingerprint, customConfiguration.SamplesPerFingerprint);
            Assert.AreEqual(defaultConfiguration.TopWavelets, customConfiguration.TopWavelets);
        }

        [TestMethod]
        public void CustomValuesAreSetOnFingerprintConfiguration()
        {
            var customConfiguration = new CustomFingerprintConfiguration
                { NormalizeSignal = true, SampleRate = 10024, TopWavelets = 250, };

            Assert.IsTrue(customConfiguration.NormalizeSignal);
            Assert.AreEqual(10024, customConfiguration.SampleRate);
            Assert.AreEqual(250, customConfiguration.TopWavelets);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidTopWaveletsTest()
        {
            var configuration = new CustomFingerprintConfiguration { TopWavelets = 0 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidSampleRateTest()
        {
            var configuration = new CustomFingerprintConfiguration { SampleRate = 0 };

            Assert.Fail();
        }
    }
}
