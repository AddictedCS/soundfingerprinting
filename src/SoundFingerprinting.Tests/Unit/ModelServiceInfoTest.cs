namespace SoundFingerprinting.Tests.Unit
{
    using NUnit.Framework;

    [TestFixture]
    public class ModelServiceInfoTest
    {
        [Test]
        public void ShouldHaveAHumanReadableStringRepresentation()
        {
            var info = new ModelServiceInfo(1, 2, new[] { 3, 4, 5 });

            Assert.AreEqual("ModelServiceInfo{TracksCount: 1, SubFingerprintsCount: 2, HashCountsInTables: [3, 4, 5]}", info.ToString());
        }
    }
}
