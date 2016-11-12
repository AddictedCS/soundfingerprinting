namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;

    [TestClass]
    public class SpectrogramConfigTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NegativeOverlapTest()
        {
            var configuration = new DefaultSpectrogramConfig { Overlap = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidWdftSizeTest()
        {
            var configuration = new DefaultSpectrogramConfig { WdftSize = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMaxFrequencyTest()
        {
            var configuration = new DefaultSpectrogramConfig { FrequencyRange = new FrequencyRange { Max = -1, Min = 5512 } };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMinFrequencyTest()
        {
            var configuration = new DefaultSpectrogramConfig { FrequencyRange = new FrequencyRange { Max = 5512, Min = -1 } };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidLogBaseTest()
        {
            var configuration = new DefaultSpectrogramConfig { LogBase = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidLogBinsTest()
        {
            var configuration = new DefaultSpectrogramConfig { LogBins = 0 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidFingerprintLengthTest()
        {
            var configuration = new DefaultSpectrogramConfig { ImageLength = 0 };

            Assert.Fail();
        }
    }
}
