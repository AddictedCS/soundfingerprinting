using NUnit.Framework;
using SoundFingerprinting.Query;
using System;

namespace SoundFingerprinting.Tests.Unit.Query
{
    [TestFixture]
    public class DiscontinuityTest
    {
        [Test]
        public void ConstructorThrowsOnInvalidArgs()
        {
            Assert.Throws<ArgumentException>(() => new Gap(4, 3, false));
        }

        [Test]
        public void LengthMustBePositive()
        {
            Assert.AreEqual(42, new Gap(8, 50, false).LengthInSeconds);
        }
    }
}
