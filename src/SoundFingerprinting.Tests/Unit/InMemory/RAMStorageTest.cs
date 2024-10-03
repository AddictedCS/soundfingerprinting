namespace SoundFingerprinting.Tests.Unit.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Tests.Integration;

    [TestFixture]
    public class RAMStorageTest : IntegrationWithSampleFilesTest
    {
        [Test]
        public void ShouldAllowInsertingEmptyHashes()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = GetTrack();
            
            storage.InsertTrack(track, AVHashes.Empty);

            var readTrack = storage.ReadByTrackId(track.Id);
            Assert.IsNotNull(readTrack);
            var hashes = storage.ReadAvHashesByTrackId(track.Id);
            Assert.IsTrue(hashes.IsEmpty);
        }

        [Test]
        public void ShouldNotAllowInsertingBothAudioAndVideoHashes()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = GetTrack();

            var audio = TestUtilities.GetRandomHashes(120, MediaType.Audio);
            var video = TestUtilities.GetRandomHashes(120, MediaType.Video);

            Assert.Throws<ArgumentException>(() => storage.InsertTrack(track, new AVHashes(audio, video)));
        }
        
        [Test]
        public void ShouldInsertEntriesInThreadSafeManner()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory(), numberOfHashTables: 50);
            var hashConverter = new HashConverter();

            var hashes = Enumerable.Range(0, 100).Select(b => (byte)b).ToArray();
            var longs = hashConverter.ToInts(hashes, 25);

            int tracksCount = 520;
            int subFingerprintsPerTrack = 33;
            float one = 8192f / 5512;
            Parallel.For(0, tracksCount, i =>
            {
                var trackReference = new ModelReference<uint>((uint)i);
                for (int j = 0; j < subFingerprintsPerTrack; ++j)
                {
                    var subFingerprintData = new SubFingerprintData(longs, (uint)j, j * one, new ModelReference<uint>((uint)j), trackReference, Array.Empty<byte>());
                    storage.AddSubFingerprint(subFingerprintData);
                }
            });

            for (int i = 0; i < 25; ++i)
            {
                var subFingerprints = storage.GetSubFingerprintsByHashTableAndHash(i, longs[i], MediaType.Audio);
                Assert.AreEqual(tracksCount * subFingerprintsPerTrack, subFingerprints.Count);
            }
        }
        
        [Test]
        public void GetTrackIdsTest()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            const int trackCount = 5;
            var expectedTracks = InsertTracks(storage, trackCount);

            var tracks = storage.GetTrackIds().ToList();

            CollectionAssert.AreEqual(expectedTracks.Select(_ => _.Id), tracks);
        }

        [Test]
        public void ReadTrackByIdTest()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var expectedTrack = GetTrack();
            storage.InsertTrack(expectedTrack, AVHashes.Empty);

            var actualTrack = storage.ReadByTrackId(expectedTrack.Id);
            Assert.IsNotNull(actualTrack);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [Test]
        public void DeleteCollectionOfTracksTest()
        {
            const int numberOfTracks = 10;
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            InsertTracks(storage, numberOfTracks);

            var allTracks = storage.GetTrackIds().ToList();

            Assert.AreEqual(numberOfTracks, allTracks.Count);
            foreach (var track in allTracks.Select(trackId => storage.ReadByTrackId(trackId)))
            {
                Assert.IsNotNull(track);
                storage.DeleteTrack(track.Id);
            }

            Assert.IsEmpty(storage.GetTrackIds());
        }

        [Test]
        public void DeleteOneTrackTest()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = GetTrack();
            storage.InsertTrack(track, AVHashes.Empty);
            var tracks = storage.ReadByTrackId(track.Id);

            Assert.IsNotNull(tracks);
            storage.DeleteTrack(tracks.Id);
            Assert.IsNull(storage.ReadByTrackId(track.Id));
            Assert.IsTrue(storage.ReadAvHashesByTrackId(track.Id).IsEmpty);
        }

        [Test]
        public async Task DeleteHashBinsAndSubFingerprintsOnTrackDelete()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var tagInfo = GetTagInfo();
            var track = new TrackInfo(tagInfo.ISRC, tagInfo.Title, tagInfo.Artist);
            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .WithFingerprintConfig(config =>
                {
                    config.Audio.Stride = new IncrementalStaticStride(8192);
                    return config;
                })
                .Hash();

            storage.InsertTrack(track, avHashes);
            
            var actualTrack = storage.ReadByTrackId(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);

            // Act
            int modifiedRows = storage.DeleteTrack(actualTrack.Id);

            Assert.IsNull(storage.ReadByTrackId(tagInfo.ISRC));
            Assert.IsTrue(storage.ReadAvHashesByTrackId(actualTrack.Id).IsEmpty);
            Assert.AreEqual(1 + avHashes.Count + 25 * avHashes.Count, modifiedRows);
        }

        [Test]
        public void InsertTrackShouldAcceptEmptyEntriesCodes()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = new TrackInfo(string.Empty, string.Empty, string.Empty);
            storage.InsertTrack(track, new AVHashes(TestUtilities.GetRandomHashes(10), null));
            var trackData = storage.ReadByTrackId(string.Empty);
            Assert.IsNotNull(trackData);
            AssertTracksAreEqual(track, trackData);
            Assert.IsFalse(storage.ReadAvHashesByTrackId(track.Id).IsEmpty);
        }

        [Test]
        public void ShouldSnapshotAndReload()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            foreach (var index in Enumerable.Range(0, 10))
            {
                var track = new TrackInfo($"id-{index}", string.Empty, string.Empty);
                var hashes = TestUtilities.GetRandomHashes(30, MediaType.Audio);
                storage.InsertTrack(track, new AVHashes(hashes, null));
            }
            
            storage.Snapshot("audio-storage.bin");

            var reload = new RAMStorage("audio-reloaded", new UIntModelReferenceTracker(), new NullLoggerFactory(), "audio-storage.bin");
            
            CollectionAssert.AreEqual(storage.GetTrackIds(), reload.GetTrackIds());
            Assert.AreEqual(storage.TracksCount, reload.TracksCount);
            Assert.AreEqual(storage.SubFingerprintsCount, reload.SubFingerprintsCount);
            foreach (string id in reload.GetTrackIds())
            {
                var (audioReloaded, _) = reload.ReadAvHashesByTrackId(id);
                var (audioHashes, _) = storage.ReadAvHashesByTrackId(id);
                TestUtilities.AssertHashesAreTheSame(audioHashes, audioReloaded);
            }
            
            reload.InsertTrack(GetTrack(), AVHashes.Empty);
            
            Assert.AreEqual(storage.TracksCount + 1, reload.TracksCount);
        }
        
         [Test]
        public void ShouldInsertAndReadSubFingerprints()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = new TrackInfo("id", string.Empty, string.Empty);
            const int numberOfHashBins = 100;
            var genericHashBuckets = GenericHashBuckets();
            var hashedFingerprints = Enumerable
                .Range(0, numberOfHashBins)
                .Select(sequenceNumber => new HashedFingerprint(genericHashBuckets, (uint)sequenceNumber, sequenceNumber * 0.928f, Array.Empty<byte>()));
            

            var hashes = new Hashes(hashedFingerprints, (numberOfHashBins + 1) * 0.928f, MediaType.Audio);
            
            storage.InsertTrack(track, new AVHashes(hashes, null));
            var (readHashes, _) = storage.ReadAvHashesByTrackId(track.Id);
            
            Assert.AreEqual(numberOfHashBins, readHashes.Count);
            foreach (var hashedFingerprint in readHashes)
            {
                CollectionAssert.AreEqual(genericHashBuckets, hashedFingerprint.HashBins);
            }
        }

        [Test]
        public async Task SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = new TrackInfo("id", string.Empty, string.Empty);
            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(GetAudioSamples())
                .Hash();

            storage.InsertTrack(track, avHashes);
            var (readHashes, _) = storage.ReadAvHashesByTrackId(track.Id);

            Assert.AreEqual(avHashes.Count, readHashes.Count);
            foreach (var data in readHashes)
            {
                Assert.AreEqual(25, data.HashBins.Length);
            }
        }

        private static List<TrackInfo> InsertTracks(IRAMStorage storage, int trackCount)
        {
            var tracks = new List<TrackInfo>();
            for (int i = 0; i < trackCount; i++)
            {
                var track = GetTrack();
                tracks.Add(track);
                storage.InsertTrack(track, AVHashes.Empty);
            }

            return tracks;
        }

        private static TrackInfo GetTrack()
        {
            return new TrackInfo(Guid.NewGuid().ToString(), "title", "artist");
        }
    }
}
