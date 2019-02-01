using NUnit.Framework;
using SoundFingerprinting.DAO;

namespace SoundFingerprinting.Tests.Unit.DAO
{
    [TestFixture]
    public class ModelReferenceTest
    {
        [Test]
        public void ShouldHaveAHumanReadableStringRepresentation()
        {
            Assert.AreEqual("ModelReference<UInt32>{Id: 42}", new ModelReference<uint>(42).ToString());
        }
    }
}
