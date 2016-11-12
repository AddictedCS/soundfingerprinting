namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;

    [TestClass]
    public class QueryConfigurationTest : AbstractTest
    {
        [TestMethod]
        public void CustomQueryConfigurationParametersCanBeSuccessfullySetTest()
        {
            var queryConfiguration = new DefaultQueryConfiguration { ThresholdVotes = 7, MaxTracksToReturn = 10 };

            Assert.AreEqual(7, queryConfiguration.ThresholdVotes);
            Assert.AreEqual(10, queryConfiguration.MaxTracksToReturn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidThresholdVotesIsSetOnQueryConfigurationInstanceTest()
        {
            var queryConfig = new DefaultQueryConfiguration { ThresholdVotes = -1 };
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidMaximumNumberOfTracksToReturnIsSetOnQueryConfigurationInstanceTest()
        {
            var queryConfig = new DefaultQueryConfiguration { MaxTracksToReturn = 0 };
            Assert.Fail();
        }
    }
}
