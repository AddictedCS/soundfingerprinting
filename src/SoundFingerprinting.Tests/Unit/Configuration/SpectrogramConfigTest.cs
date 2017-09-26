namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class SpectrogramConfigTest
    {
        [Test]
        public void InvalidLogBaseTest()
        {
            Assert.Throws<ArgumentException>(() => new DefaultSpectrogramConfig { LogBase = -1 });
        }
    }
}
