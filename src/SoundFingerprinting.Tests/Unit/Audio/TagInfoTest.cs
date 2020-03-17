﻿namespace SoundFingerprinting.Tests.Unit.Audio
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

            Assert.IsFalse(tagInfoWithoutArtist.IsTrackUniquelyIdentifiable());
            Assert.IsFalse(tagInfoWithoutTitle.IsTrackUniquelyIdentifiable());
            Assert.IsFalse(tagInfoWithoutIsrc.IsTrackUniquelyIdentifiable());
        }

        [Test]
        public void ShouldIdentify()
        {
            var tagInfoWithTitleAndArtist = new TagInfo { Artist = "artist", Title = "title" };
            var tagInfoWithIsrc = new TagInfo { ISRC = "12345" };

            Assert.IsTrue(tagInfoWithIsrc.IsTrackUniquelyIdentifiable());
            Assert.IsTrue(tagInfoWithTitleAndArtist.IsTrackUniquelyIdentifiable());
        }
    }
}
