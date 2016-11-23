namespace SoundFingerprinting.Tests
{
    using NUnit.Framework;

    using SoundFingerprinting.DAO.Data;

    public abstract class AbstractTest
    {
        protected const double Epsilon = 0.0001;

        protected const int BitsPerSample = 32;
        
        protected const int SampleRate = 5512;

        protected const int SamplesPerFingerprint = 128 * 64;

        protected const int WaveHeader = 58;

        protected const string PathToMp3 = @"Kryptonite.mp3";

        protected const string PathToSamples = @"floatsamples.bin";

        protected const int SamplesToRead = 128 * 64;

        protected const int MinYear = 1501;

        protected readonly bool[] GenericFingerprint = new[]
            {
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true, true, false, true, false, true, false, true, false,
                true, false, true, false, false, true, false, true, false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true, true, false, true, false, true, false, true, false,
                true, false, true, false, false, true, false, true, false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false, true, false, true, false, false, true, false, true,
                false, true, false, true, false, true, false, true
            };

        protected readonly byte[] GenericSignature = new[]
            {
                (byte)0, (byte)1, (byte)2, (byte)3, (byte)4, (byte)5, (byte)6, (byte)7, (byte)8, (byte)9, (byte)10, (byte)11,
                (byte)12, (byte)13, (byte)14, (byte)15, (byte)16, (byte)17, (byte)18, (byte)19, (byte)20, (byte)21,
                (byte)22, (byte)23, (byte)24, (byte)25, (byte)26, (byte)27, (byte)28, (byte)29, (byte)30, (byte)31,
                (byte)32, (byte)33, (byte)34, (byte)37, (byte)38, (byte)39, (byte)40, (byte)41, (byte)42, (byte)43,
                (byte)44, (byte)45, (byte)46, (byte)47, (byte)48, (byte)49, (byte)50, (byte)51
            };

        protected readonly long[] GenericHashBuckets = new[]
            {
                256L, 770, 1284, 1798, 2312, 2826, 3340, 3854, 4368, 4882, 5396, 5910, 6424, 6938, 7452, 7966, 8480, 9506,
                10022, 10536, 11050, 11564, 12078, 12592, 13106
            };

        protected void AssertTracksAreEqual(TrackData expectedTrack, TrackData actualTrack)
        {
            Assert.AreEqual(expectedTrack.TrackReference, actualTrack.TrackReference);
            Assert.AreEqual(expectedTrack.Album, actualTrack.Album);
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.TrackLengthSec, actualTrack.TrackLengthSec);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
            Assert.AreEqual(expectedTrack.GroupId, actualTrack.GroupId);
        }
    }
}
