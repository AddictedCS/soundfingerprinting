using NUnit.Framework;
using SoundFingerprinting.Query;

namespace SoundFingerprinting.Tests.Unit.Query
{
    [TestFixture]
    public class DiscontinuityTest
    {
        [Test]
        public void LengthMustBePositive()
        {
            Assert.AreEqual(42, new Discontinuity(8, 50).LengthInSeconds);
        }
    }
}
