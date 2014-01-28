namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;

    [TestClass]
    public class CustomQueryConfigurationTest : AbstractTest
    {
        [TestMethod]
        public void CustomQueryConfigurationInheritsDefaultValuesTest()
        {
            var queryConfiguration = new CustomQueryConfiguration();
            var defaultConfiguration = new DefaultQueryConfiguration();

            Assert.AreEqual(defaultConfiguration.ThresholdVotes, queryConfiguration.ThresholdVotes);
            Assert.AreEqual(defaultConfiguration.MaximumNumberOfTracksToReturnAsResult, queryConfiguration.MaximumNumberOfTracksToReturnAsResult);
        }

        [TestMethod]
        public void CustomQueryConfigurationParametersCanBeSuccessfullySetTest()
        {
            var queryConfiguration = new CustomQueryConfiguration { ThresholdVotes = 7, MaximumNumberOfTracksToReturnAsResult = 10 };

            Assert.AreEqual(7, queryConfiguration.ThresholdVotes);
            Assert.AreEqual(10, queryConfiguration.MaximumNumberOfTracksToReturnAsResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidThresholdVotesIsSetOnQueryConfigurationInstanceTest()
        {
            var queryConfig = new CustomQueryConfiguration { ThresholdVotes = -1 };
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMaximumNumberOfTracksToReturnIsSetOnQueryConfigurationInstanceTest()
        {
            var queryConfig = new CustomQueryConfiguration { MaximumNumberOfTracksToReturnAsResult = 0 };
            Assert.Fail();
        }
    }
}
