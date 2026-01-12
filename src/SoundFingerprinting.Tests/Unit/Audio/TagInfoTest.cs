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
            var tagInfoWithoutIsrc = new TagInfo();

			Assert.Multiple(() =>
			{
				Assert.That(tagInfoWithoutArtist.IsTrackUniquelyIdentifiable(), Is.False);
				Assert.That(tagInfoWithoutTitle.IsTrackUniquelyIdentifiable(), Is.False);
				Assert.That(tagInfoWithoutIsrc.IsTrackUniquelyIdentifiable(), Is.False);
			});
		}

        [Test]
        public void ShouldIdentify()
        {
            var tagInfoWithTitleAndArtist = new TagInfo { Artist = "artist", Title = "title" };
            var tagInfoWithIsrc = new TagInfo { ISRC = "12345" };

			Assert.Multiple(() =>
			{
				Assert.That(tagInfoWithIsrc.IsTrackUniquelyIdentifiable(), Is.True);
				Assert.That(tagInfoWithTitleAndArtist.IsTrackUniquelyIdentifiable(), Is.True);
			});
		}
    }
}
