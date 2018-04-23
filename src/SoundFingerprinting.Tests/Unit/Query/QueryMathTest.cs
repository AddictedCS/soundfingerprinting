namespace SoundFingerprinting.Tests.Unit.Query
{
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.IO;
    using System;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;
    using ProtoBuf;

    [TestFixture]
    public class QueryMathTest
    {/*
        private Mock<IModelService> modelService = new Mock<IModelService>(MockBehavior.Strict);

        private readonly QueryMath queryMath = new QueryMath(
            new QueryResultCoverageCalculator(),
            new ConfidenceCalculator());

        [Test]
        public void ShoulCalculateSnippetLengthCorrectly()
        {
            var hashedFingerprints = new List<HashedFingerprint>
                {
                    new HashedFingerprint(null, 1, 3, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 0, 1, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 3, 9.142235f, Enumerable.Empty<string>())
                };

            double snippetLength = queryMath.CalculateExactQueryLength(hashedFingerprints, new DefaultFingerprintConfiguration());

            Assert.AreEqual(9.6284d, snippetLength, 0.0001);
        }

        [Test]
        public void ShouldGetBestCandidatesByHammingDistance()
        {
            var modelService = new Mock<IModelService>(MockBehavior.Strict);
            var trackReference = new ModelReference<int>(3);
            modelService.Setup(service => service.ReadTracksByReferences(new[] { trackReference })).Returns(new List<TrackData> { new TrackData { ISRC = "isrc-1234-1234", TrackReference = trackReference } });

            var queryConfiguration = new DefaultQueryConfiguration { MaxTracksToReturn = 1 };

            var query = new List<HashedFingerprint>
                {
                    new HashedFingerprint(null, 1, 0, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 1, 4, Enumerable.Empty<string>()),
                    new HashedFingerprint(null, 1, 8, Enumerable.Empty<string>())
                };

            var first = new ResultEntryAccumulator(query[0], new SubFingerprintData(null, 1, 0, null, null), 100);
            var second = new ResultEntryAccumulator(query[1], new SubFingerprintData(null, 1, 4, null, null), 99);
            var third = new ResultEntryAccumulator(query[2], new SubFingerprintData(null, 1, 8, null, null), 101);
            var hammingSimilarties = new Dictionary<IModelReference, ResultEntryAccumulator>
                {
                    { new ModelReference<int>(1), first },
                    { new ModelReference<int>(2), second },
                    { new ModelReference<int>(3), third },
                };

            var best = queryMath.GetBestCandidates(
                query,
                hammingSimilarties,
                queryConfiguration.MaxTracksToReturn,
                modelService.Object,
                queryConfiguration.FingerprintConfiguration);

            Assert.AreEqual(1, best.Count);
            Assert.AreEqual("isrc-1234-1234", best[0].Track.ISRC);
            Assert.AreEqual(9.48d, best[0].QueryLength, 0.01);
            Assert.AreEqual(0d, best[0].TrackStartsAt);
            modelService.VerifyAll();
        }

        [Test]
        public void ShouldFilterExactMatches0()
        {
            bool result = queryMath.IsCandidatePassingThresholdVotes(
                new HashedFingerprint(new int[] { 1, 2, 3, 4, 5 }, 0, 0, Enumerable.Empty<string>()),
                new SubFingerprintData(new int[] { 1, 2, 3, 7, 8 }, 0, 0, null, null),
                3);

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldFilterExactMatches1()
        {
            bool result = queryMath.IsCandidatePassingThresholdVotes(
                new HashedFingerprint(new int[] { 1, 2, 3, 4, 5 }, 0, 0, Enumerable.Empty<string>()),
                new SubFingerprintData(new int[] { 1, 2, 4, 7, 8 }, 0, 0, null, null),
                3);

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldNotFailIfModelServiceReturnsAnEmptyList()
        {
            var fingerprint = new HashedFingerprint(new[] { 1, 2, 3, 4, 5 }, 1, 1f, Enumerable.Empty<string>());
            var hashedFingerprints = new List<HashedFingerprint> { fingerprint };

            var hammingSimilarities = new ConcurrentDictionary<IModelReference, ResultEntryAccumulator>();
            hammingSimilarities.AddOrUpdate(
                new ModelReference<int>(0),
                new ResultEntryAccumulator(fingerprint, new SubFingerprintData(), 100),
                (a, b) => b);

            modelService.Setup(s => s.ReadTracksByReferences(It.IsAny<IEnumerable<IModelReference>>()))
                .Returns(new List<TrackData> { new TrackData() });

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    queryMath.GetBestCandidates(
                        hashedFingerprints,
                        hammingSimilarities,
                        5,
                        modelService.Object,
                        new DefaultFingerprintConfiguration());
                });
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
        } */
    }
}
