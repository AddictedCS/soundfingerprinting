namespace SoundFingerprinting.Tests
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public abstract class AbstractTest
    {
        private readonly int[] genericHashBucketsArray =
        {
            256, 770, 1284, 1798, 2312, 2826, 3340, 3854, 4368, 4882, 5396, 5910, 6424, 6938, 7452, 7966, 8480,
            9506, 10022, 10536, 11050, 11564, 12078, 12592, 13106
        };

        protected Hashes GetGenericHashes(MediaType mediaType = MediaType.Audio)
        {
            return new Hashes(new[] {new HashedFingerprint(GenericHashBuckets(), 0, 0f, Array.Empty<byte>())}, 1.48f, mediaType, DateTime.Now, Enumerable.Empty<string>());
        }
        
        protected int[] GenericHashBuckets()
        {
            return (int[])genericHashBucketsArray.Clone();
        }

        protected void AssertTracksAreEqual(TrackInfo expectedTrack, TrackInfo actualTrack)
        {
			Assert.Multiple(() =>
			{
				Assert.That(actualTrack.Artist, Is.EqualTo(expectedTrack.Artist));
				Assert.That(actualTrack.Title, Is.EqualTo(expectedTrack.Title));
				Assert.That(actualTrack.Id, Is.EqualTo(expectedTrack.Id));
				Assert.That(actualTrack.MediaType, Is.EqualTo(expectedTrack.MediaType));
			});
			foreach ((string key, string val) in expectedTrack.MetaFields)
            {
				Assert.Multiple(() =>
				{
					Assert.That(actualTrack.MetaFields.TryGetValue(key, out string value), Is.True);
					Assert.That(value, Is.EqualTo(val));
				});
			}
        }
        
        protected void AssertTracksAreEqual(TrackInfo expectedTrack, TrackData actualTrack)
        {
			Assert.Multiple(() =>
			{
				Assert.That(actualTrack.Artist, Is.EqualTo(expectedTrack.Artist));
				Assert.That(actualTrack.Title, Is.EqualTo(expectedTrack.Title));
				Assert.That(actualTrack.Id, Is.EqualTo(expectedTrack.Id));
				Assert.That(actualTrack.MediaType, Is.EqualTo(expectedTrack.MediaType));
			});
			foreach ((string key, string val) in expectedTrack.MetaFields)
            {
				Assert.Multiple(() =>
				{
					Assert.That(actualTrack.MetaFields.TryGetValue(key, out string value), Is.True);
					Assert.That(value, Is.EqualTo(val));
				});
			}
        }
        
        protected void AssertTracksAreEqual(TrackData expectedTrack, TrackData actualTrack)
        {
			Assert.Multiple(() =>
			{
				Assert.That(actualTrack.Artist, Is.EqualTo(expectedTrack.Artist));
				Assert.That(actualTrack.Title, Is.EqualTo(expectedTrack.Title));
				Assert.That(actualTrack.Id, Is.EqualTo(expectedTrack.Id));
				Assert.That(actualTrack.MediaType, Is.EqualTo(expectedTrack.MediaType));
			});
			foreach ((string key, string val) in expectedTrack.MetaFields)
            {
				Assert.Multiple(() =>
				{
					Assert.That(actualTrack.MetaFields.TryGetValue(key, out string value), Is.True);
					Assert.That(value, Is.EqualTo(val));
				});
			}
        }
    }
}
