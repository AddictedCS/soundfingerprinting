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
            Assert.That((, Throws.TypeOf<ArgumentException>()) => new DefaultSpectrogramConfig { LogBase = -1 });
        }
    }
}
