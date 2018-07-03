namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;
    using ProtoBuf;

    [TestFixture]
    public class QueryMathTest
    {
        private readonly QueryMath queryMath = new QueryMath(
            new QueryResultCoverageCalculator(new LongestIncreasingTrackSequence()),
            new ConfidenceCalculator());

        [Test]
        public void ShouldFilterExactMatches0()
        {
            bool result = queryMath.IsCandidatePassingThresholdVotes(
                new HashedFingerprint(new[] { 1, 2, 3, 4, 5 }, 0, 0, Enumerable.Empty<string>()),
                new SubFingerprintData(new[] { 1, 2, 3, 7, 8 }, 0, 0, null, null),
                3);

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldFilterExactMatches1()
        {
            bool result = queryMath.IsCandidatePassingThresholdVotes(
                new HashedFingerprint(new[] { 1, 2, 3, 4, 5 }, 0, 0, Enumerable.Empty<string>()),
                new SubFingerprintData(new[] { 1, 2, 4, 7, 8 }, 0, 0, null, null),
                3);

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldFailDeserialization()
        {
            using (var memory = new MemoryStream())
            {
                var track = new TrackData("asdf", "asdf", "asdf", "asdf", 1986, 1d, new ModelReference<int>(0));
                Serializer.SerializeWithLengthPrefix(memory, new List<TrackData> { track, track }, PrefixStyle.Fixed32);

                byte[] buffer = memory.GetBuffer();

                for (int i = 1; i < buffer.Length; ++i)
                {
                    byte[] corrupted = new byte[i];
                    Array.Copy(buffer, 0, corrupted, 0, i);
                    using (var toRead = new MemoryStream(corrupted))
                    {
                        try
                        {
                            var result = Serializer.DeserializeWithLengthPrefix<List<TrackData>>(toRead, PrefixStyle.Fixed32);
                            CollectionAssert.IsNotEmpty(result);
                            Assert.IsNotNull(result[0].TrackReference);
                            if (result.Count > 1)
                            {
                                Assert.IsNotNull(result[1].TrackReference);
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }
        }
    }
}
