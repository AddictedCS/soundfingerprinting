namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class QueryConfigurationTest : AbstractTest
    {
        [Test]
        public void InvalidThresholdVotesIsSetOnQueryConfigurationInstanceTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultQueryConfiguration { ThresholdVotes = -1 });
        }

        [Test]
        public void InvalidMaximumNumberOfTracksToReturnIsSetOnQueryConfigurationInstanceTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultQueryConfiguration { MaxTracksToReturn = 0 });
        }
    }
}
