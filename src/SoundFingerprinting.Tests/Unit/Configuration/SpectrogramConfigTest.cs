namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using Assert = NUnit.Framework.Legacy.ClassicAssert;
    using static NUnit.Framework.Legacy.ClassicAssert;

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
