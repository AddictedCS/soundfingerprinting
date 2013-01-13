namespace Soundfingerprinting.UnitTests.AudioProxies.Tests
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Audio.Models;
    using Soundfingerprinting.Audio.Services;

    [TestClass]
    public class TagServiceTest : BaseTest
    {
        private ITagService tagService;

        [TestInitialize]
        public void SetUp()
        {
            tagService = new TagService();
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
