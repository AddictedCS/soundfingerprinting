namespace SoundFingerprinting.Tests.Unit.Audio
{
    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class TagInfoTest
    {
        [Test]
        public void ShouldNotIdentify()
        {
            var tagInfoWithoutTitle = new TagInfo { Artist = "artist" };
            var tagInfoWithoutArtist = new TagInfo { Title = "title" };
            var tagInfoWithoutISRC = new TagInfo();

            Assert.IsFalse(tagInfoWithoutArtist.IsTrackUniquelyIdentifiable());
            Assert.IsFalse(tagInfoWithoutTitle.IsTrackUniquelyIdentifiable());
            Assert.IsFalse(tagInfoWithoutISRC.IsTrackUniquelyIdentifiable());
        }

        [Test]
        public void ShouldIdentify()
        {
            var tagInfoWithTitleAndArtist = new TagInfo { Artist = "artist", Title = "title" };
            var tagInfoWithISRC = new TagInfo { ISRC = "ISRC" };

            Assert.IsTrue(tagInfoWithISRC.IsTrackUniquelyIdentifiable());
            Assert.IsTrue(tagInfoWithTitleAndArtist.IsTrackUniquelyIdentifiable());
        }
    }
}
