namespace SoundFingerprinting.Audio.Bass.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class BassTagServiceTest : AbstractIntegrationTest
    {
        private readonly ITagService tagService = new BassTagService();

        [TestMethod]
        public void TagAreSuccessfullyReadFromFile()
        {
            TagInfo tags = tagService.GetTagInfo(PathToMp3);
            Assert.IsNotNull(tags);
            Assert.AreEqual("3 Doors Down", tags.Artist);
            Assert.AreEqual("Kryptonite", tags.Title);
            Assert.AreEqual("USUR19980187", tags.ISRC);
            Assert.AreEqual(1997, tags.Year);
            Assert.IsTrue(Math.Abs(232 - tags.Duration) < 1);
        }
    }
}
