namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class SpectrogramConfigTest
    {
        [Test]
        public void NegativeOverlapTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultSpectrogramConfig { Overlap = -1 });
        }

        [Test]
        public void InvalidWdftSizeTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultSpectrogramConfig { WdftSize = -1 });
        }

        [Test]
        public void InvalidMaxFrequencyTest()
        {
            Assert.Throws<ArgumentException>(
                () => new DefaultSpectrogramConfig { FrequencyRange = new FrequencyRange { Max = -1, Min = 5512 } });
        }

        [Test]
        public void InvalidMinFrequencyTest()
        {
            Assert.Throws<ArgumentException>(
                () => new DefaultSpectrogramConfig { FrequencyRange = new FrequencyRange { Max = 5512, Min = -1 } });
        }

        [Test]
        public void InvalidLogBaseTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultSpectrogramConfig { LogBase = -1 });
        }

        [Test]
        public void InvalidLogBinsTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultSpectrogramConfig { LogBins = 0 });
        }

        [Test]
        public void InvalidFingerprintLengthTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultSpectrogramConfig { ImageLength = 0 });
        }
    }
}
