namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;

    [TestClass]
    public class FingerprintConfigurationTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidTopWaveletsTest()
        {
            var configuration = new DefaultFingerprintConfiguration { TopWavelets = 0 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidSampleRateTest()
        {
            var configuration = new DefaultFingerprintConfiguration { SampleRate = 0 };

            Assert.Fail();
        }
    }
}
