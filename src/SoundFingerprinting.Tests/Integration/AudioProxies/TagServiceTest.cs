namespace SoundFingerprinting.Tests.Integration.AudioProxies
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class TagServiceTest : AbstractIntegrationTest
    {
        private readonly ITagService tagService = new BassAudioService();

        [TestMethod]
        public void TagAreSuccessfullyReadFromFile()
        {
            TagInfo tags = tagService.GetTagInfo(PathToMp3);
            Assert.IsNotNull(tags);
            Assert.IsNotNull(tags.Artist);
            Assert.IsNotNull(tags.Title);
            Assert.IsNotNull(tags.Duration);
        }
    }
}
