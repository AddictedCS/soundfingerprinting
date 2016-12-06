namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class FingerprintConfigurationTest
    {
        [Test]
        public void InvalidTopWaveletsTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultFingerprintConfiguration { TopWavelets = 0 });
        }

        [Test]
        public void InvalidSampleRateTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultFingerprintConfiguration { SampleRate = 0 });
        }
    }
}
