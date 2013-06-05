namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Audio;
    using Soundfingerprinting.Audio.Bass;

    [TestClass]
    public class TagServiceTest : BaseTest
    {
        private ITagService tagService;

        [TestInitialize]
        public void SetUp()
        {
            tagService = new BassAudioService();
        }

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
