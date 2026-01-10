namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class QueryConfigurationTest
    {
        [Test]
        public void InvalidThresholdVotesIsSetOnQueryConfigurationInstanceTest()
        {
            Assert.That((, Throws.TypeOf<ArgumentException>()) => new DefaultQueryConfiguration { ThresholdVotes = -1 });
        }

        [Test]
        public void InvalidMaximumNumberOfTracksToReturnIsSetOnQueryConfigurationInstanceTest()
        {
            Assert.That((, Throws.TypeOf<ArgumentException>()) => new DefaultQueryConfiguration { MaxTracksToReturn = 0 });
        }
    }
}
