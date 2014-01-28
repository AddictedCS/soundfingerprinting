namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Strides;

#pragma warning disable 168
    [TestClass]
    public class CustomFingerprintConfigurationTest
    {
        [TestMethod]
        public void FingerprintConfigurationInheritsDefaultValues()
        {
            var customConfiguration = new CustomFingerprintConfiguration();
            var defaultConfiguration = new DefaultFingerprintConfiguration();

            Assert.AreEqual(defaultConfiguration.FingerprintLength, customConfiguration.FingerprintLength);
            Assert.AreEqual(defaultConfiguration.LogBase, customConfiguration.LogBase);
            Assert.AreEqual(defaultConfiguration.LogBins, customConfiguration.LogBins);
            Assert.AreEqual(defaultConfiguration.MaxFrequency, customConfiguration.MaxFrequency);
            Assert.AreEqual(defaultConfiguration.MinFrequency, customConfiguration.MinFrequency);
            Assert.AreEqual(defaultConfiguration.NormalizeSignal, customConfiguration.NormalizeSignal);
            Assert.AreEqual(defaultConfiguration.NumberOfLSHTables, customConfiguration.NumberOfLSHTables);
            Assert.AreEqual(defaultConfiguration.NumberOfMinHashesPerTable, customConfiguration.NumberOfMinHashesPerTable);
            Assert.AreEqual(defaultConfiguration.Overlap, customConfiguration.Overlap);
            Assert.AreEqual(defaultConfiguration.SampleRate, customConfiguration.SampleRate);
            Assert.AreEqual(defaultConfiguration.SamplesPerFingerprint, customConfiguration.SamplesPerFingerprint);
            Assert.IsTrue(defaultConfiguration.Stride.GetType() == customConfiguration.Stride.GetType());
            Assert.AreEqual(defaultConfiguration.TopWavelets, customConfiguration.TopWavelets);
            Assert.AreEqual(defaultConfiguration.UseDynamicLogBase, customConfiguration.UseDynamicLogBase);
            Assert.AreEqual(defaultConfiguration.WdftSize, customConfiguration.WdftSize);
        }

        [TestMethod]
        public void CustomValuesAreSetOnFingerprintConfiguration()
        {
            var staticStride = new StaticStride(2048);
            var customConfiguration = new CustomFingerprintConfiguration
                                          {
                                              FingerprintLength = 256,
                                              LogBase = 4,
                                              LogBins = 46,
                                              MaxFrequency = 22050,
                                              MinFrequency = 5512,
                                              NormalizeSignal = true,
                                              NumberOfLSHTables = 30,
                                              NumberOfMinHashesPerTable = 4,
                                              Overlap = 32,
                                              SampleRate = 10024,
                                              Stride = staticStride,
                                              TopWavelets = 250,
                                              UseDynamicLogBase = true,
                                              WdftSize = 4048
                                          };

            Assert.AreEqual(256, customConfiguration.FingerprintLength);
            Assert.AreEqual(4, customConfiguration.LogBase);
            Assert.AreEqual(46, customConfiguration.LogBins);
            Assert.AreEqual(22050, customConfiguration.MaxFrequency);
            Assert.AreEqual(5512, customConfiguration.MinFrequency);
            Assert.IsTrue(customConfiguration.NormalizeSignal);
            Assert.AreEqual(30, customConfiguration.NumberOfLSHTables);
            Assert.AreEqual(4, customConfiguration.NumberOfMinHashesPerTable);
            Assert.AreEqual(32, customConfiguration.Overlap);
            Assert.AreEqual(10024, customConfiguration.SampleRate);
            Assert.AreEqual(staticStride, customConfiguration.Stride);
            Assert.AreEqual(250, customConfiguration.TopWavelets);
            Assert.IsTrue(customConfiguration.UseDynamicLogBase);
            Assert.AreEqual(4048, customConfiguration.WdftSize);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NegativeOverlapTest()
        {
            var configuration = new CustomFingerprintConfiguration { Overlap = -1 };
            
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidWdftSizeTest()
        {
            var configuration = new CustomFingerprintConfiguration { WdftSize = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMinFrequencyTest()
        {
            var configuration = new CustomFingerprintConfiguration { MinFrequency = 44100 * 2 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMaxFrequencyTest()
        {
            var configuration = new CustomFingerprintConfiguration { MaxFrequency = 44100 * 2 };

            Assert.Fail();
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidLogBaseTest()
        {
            var configuration = new CustomFingerprintConfiguration { LogBase = -1 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidLogBinsTest()
        {
            var configuration = new CustomFingerprintConfiguration { LogBins = 0 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidFingerprintLengthTest()
        {
            var configuration = new CustomFingerprintConfiguration { FingerprintLength = 0 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidNumberOfLSHTablesTest()
        {
            var configuration = new CustomFingerprintConfiguration { NumberOfLSHTables = 0 };

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidNumberOfMinHashesPerTableTest()
        {
            var configuration = new CustomFingerprintConfiguration { NumberOfMinHashesPerTable = 0 };

            Assert.Fail();
        }
    }
#pragma warning restore 168
}
