namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;

    [TestClass]
    public class SpectrogramConfigTest
    {
        public void CustomSpectrumValuesInheritFromDefault()
        {
            SpectrogramConfig defaultConfiguration = SpectrogramConfig.Default;
            SpectrogramConfig customConfiguration = new CustomSpectrogramConfig();

            Assert.AreEqual(defaultConfiguration.ImageLength, customConfiguration.ImageLength);
            Assert.AreEqual(defaultConfiguration.LogBase, customConfiguration.LogBase);
            Assert.AreEqual(defaultConfiguration.LogBins, customConfiguration.LogBins);
            Assert.AreEqual(defaultConfiguration.FrequencyRange.Max, customConfiguration.FrequencyRange.Max);
            Assert.AreEqual(defaultConfiguration.FrequencyRange.Min, customConfiguration.FrequencyRange.Min);
            Assert.AreEqual(defaultConfiguration.UseDynamicLogBase, customConfiguration.UseDynamicLogBase);
            Assert.AreEqual(defaultConfiguration.WdftSize, customConfiguration.WdftSize);
            Assert.AreEqual(defaultConfiguration.Overlap, customConfiguration.Overlap);
        }

        public void CustomSpectrumValuesOverrideDefaults()
        {
            var customConfiguration = new CustomSpectrogramConfig
            {
                ImageLength = 256,
                LogBase = 4,
                LogBins = 46,
                FrequencyRange = new FrequencyRange { Min = 5512, Max = 22050 },
                Overlap = 32,
                UseDynamicLogBase = true,
                WdftSize = 4048
            };

            Assert.AreEqual(256, customConfiguration.ImageLength);
            Assert.AreEqual(4, customConfiguration.LogBase);
            Assert.AreEqual(46, customConfiguration.LogBins);
            Assert.AreEqual(22050, customConfiguration.FrequencyRange.Max);
            Assert.AreEqual(5512, customConfiguration.FrequencyRange.Min);
            Assert.AreEqual(32, customConfiguration.Overlap);
            Assert.IsTrue(customConfiguration.UseDynamicLogBase);
            Assert.AreEqual(4048, customConfiguration.WdftSize);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NegativeOverlapTest()
        {
            var configuration = new CustomSpectrogramConfig { Overlap = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidWdftSizeTest()
        {
            var configuration = new CustomSpectrogramConfig { WdftSize = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMaxFrequencyTest()
        {
            var configuration = new CustomSpectrogramConfig
                { FrequencyRange = new FrequencyRange { Max = -1, Min = 5512 } };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMinFrequencyTest()
        {
            var configuration = new CustomSpectrogramConfig
                { FrequencyRange = new FrequencyRange { Max = 5512, Min = -1 } };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidLogBaseTest()
        {
            var configuration = new CustomSpectrogramConfig { LogBase = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidLogBinsTest()
        {
            var configuration = new CustomSpectrogramConfig { LogBins = 0 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidFingerprintLengthTest()
        {
            var configuration = new CustomSpectrogramConfig { ImageLength = 0 };

            Assert.Fail();
        }
    }
}
