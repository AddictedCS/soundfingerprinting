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
            Assert.That(readTrack, Is.Not.Null);
            var hashes = storage.ReadAvHashesByTrackId(track.Id);
            Assert.That(hashes.IsEmpty, Is.True);
        }

        [Test]
        public void ShouldNotAllowInsertingBothAudioAndVideoHashes()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = GetTrack();

            var audio = TestUtilities.GetRandomHashes(120, MediaType.Audio);
            var video = TestUtilities.GetRandomHashes(120, MediaType.Video);

            Assert.That(() => storage.InsertTrack(track, new AVHashes(audio, video)), Throws.TypeOf<ArgumentException>());
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
                Assert.That(subFingerprints.Count, Is.EqualTo(tracksCount * subFingerprintsPerTrack));
            }
        }
        
        [Test]
        public void GetTrackIdsTest()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            const int trackCount = 5;
            var expectedTracks = InsertTracks(storage, trackCount);

            var tracks = storage.GetTrackIds().ToList();

            Assert.That(tracks, Is.EqualTo(expectedTracks.Select(_ => _.Id)));
        }

        [Test]
        public void ReadTrackByIdTest()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var expectedTrack = GetTrack();
            storage.InsertTrack(expectedTrack, AVHashes.Empty);

            var actualTrack = storage.ReadByTrackId(expectedTrack.Id);
            Assert.That(actualTrack, Is.Not.Null);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [Test]
        public void DeleteCollectionOfTracksTest()
        {
            const int numberOfTracks = 10;
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            InsertTracks(storage, numberOfTracks);

            var allTracks = storage.GetTrackIds().ToList();

            Assert.That(allTracks.Count, Is.EqualTo(numberOfTracks));
            foreach (var track in allTracks.Select(trackId => storage.ReadByTrackId(trackId)))
            {
                Assert.That(track, Is.Not.Null);
                storage.DeleteTrack(track.Id);
            }

            Assert.That(storage.GetTrackIds(), Is.Empty);
        }

        [Test]
        public void DeleteOneTrackTest()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = GetTrack();
            storage.InsertTrack(track, AVHashes.Empty);
            var tracks = storage.ReadByTrackId(track.Id);

            Assert.That(tracks, Is.Not.Null);
            storage.DeleteTrack(tracks.Id);
            Assert.That(storage.ReadByTrackId(track.Id), Is.Null);
            Assert.That(storage.ReadAvHashesByTrackId(track.Id).IsEmpty, Is.True);
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
            Assert.That(actualTrack, Is.Not.Null);

            // Act
            int modifiedRows = storage.DeleteTrack(actualTrack.Id);

            Assert.That(storage.ReadByTrackId(tagInfo.ISRC), Is.Null);
            Assert.That(storage.ReadAvHashesByTrackId(actualTrack.Id).IsEmpty, Is.True);
            Assert.That(modifiedRows, Is.EqualTo(1 + avHashes.Count + 25 * avHashes.Count));
        }

        [Test]
        public void InsertTrackShouldAcceptEmptyEntriesCodes()
        {
            var storage = new RAMStorage("audio", new UIntModelReferenceTracker(), new NullLoggerFactory());
            var track = new TrackInfo(string.Empty, string.Empty, string.Empty);
            storage.InsertTrack(track, new AVHashes(TestUtilities.GetRandomHashes(10), null));
            var trackData = storage.ReadByTrackId(string.Empty);
            Assert.That(trackData, Is.Not.Null);
            AssertTracksAreEqual(track, trackData);
            Assert.That(storage.ReadAvHashesByTrackId(track.Id).IsEmpty, Is.False);
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
            
            Assert.That(reload.GetTrackIds(), Is.EqualTo(storage.GetTrackIds()));
            Assert.That(reload.TracksCount, Is.EqualTo(storage.TracksCount));
            Assert.That(reload.SubFingerprintsCount, Is.EqualTo(storage.SubFingerprintsCount));
            foreach (string id in reload.GetTrackIds())
            {
                var (audioReloaded, _) = reload.ReadAvHashesByTrackId(id);
                var (audioHashes, _) = storage.ReadAvHashesByTrackId(id);
                TestUtilities.AssertHashesAreTheSame(audioHashes, audioReloaded);
            }
            
            reload.InsertTrack(GetTrack(), AVHashes.Empty);
            
            Assert.That(reload.TracksCount, Is.EqualTo(storage.TracksCount + 1));
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
            
            Assert.That(readHashes.Count, Is.EqualTo(numberOfHashBins));
            foreach (var hashedFingerprint in readHashes)
            {
                Assert.That(hashedFingerprint.HashBins, Is.EqualTo(genericHashBuckets));
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

            Assert.That(readHashes.Count, Is.EqualTo(avHashes.Count));
            foreach (var data in readHashes)
            {
                Assert.That(data.HashBins.Length, Is.EqualTo(25));
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
