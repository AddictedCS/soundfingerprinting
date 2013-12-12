namespace SoundFingerprinting.Tests.Integration.DbStorage
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query.Configuration;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Tests.Integration;

    [TestClass]
    public class ModelServiceTest : AbstractIntegrationTest
    {
        private readonly ModelService modelService = new ModelService();

        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly ITagService tagService;
        private readonly ILSHService lshService;

        public ModelServiceTest()
        {
            fingerprintUnitBuilder = DependencyResolver.Current.Get<IFingerprintUnitBuilder>();
            tagService = DependencyResolver.Current.Get<ITagService>();
            lshService = DependencyResolver.Current.Get<ILSHService>();
        }

        #region Insert/Read/Delete Track objects tests

        [TestMethod]
        public void InsertReadTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            var expectedTrack = GetTrack(name);
            modelService.InsertTrack(expectedTrack);

            Assert.AreNotEqual(int.MinValue, expectedTrack.Id);
            var listOfTracks = modelService.ReadTracks();
            Assert.AreEqual(1, listOfTracks.Count);
            
            Track actualTrack = modelService.ReadTrackById(expectedTrack.Id);
            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void InsertMultipleTrackAtOnceTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            List<Track> expectedTracks = new List<Track>();
            const int TrackCount = 10;
            for (int i = 0; i < TrackCount; i++)
            {
                Track expectedTrack = GetTrack(name);
                expectedTracks.Add(expectedTrack);
            }

            modelService.InsertTrack(expectedTracks);
            foreach (Track track in expectedTracks)
            {
                Assert.AreNotEqual(int.MinValue, track.Id);
            }

            var actualTracks = modelService.ReadTracks();
            Assert.AreEqual(expectedTracks.Count, actualTracks.Count);
            for (int i = 0; i < actualTracks.Count; i++)
            {
                AssertTracksAreEqual(
                    expectedTracks[i], actualTracks.First(track => track.Id == expectedTracks[i].Id));
            }
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitle()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Track expectedTrack = GetTrack(name);
            modelService.InsertTrack(expectedTrack);
            Assert.AreNotEqual(FakeId, expectedTrack.Id);
            Track actualTrack = modelService.ReadTrackByArtistAndTitleName(expectedTrack.Artist, expectedTrack.Title);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void ReadTrackByISRC()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track expectedTrack = GetTrack(name);
            modelService.InsertTrack(expectedTrack);

            Track actualTrack = modelService.ReadTrackByISRC(expectedTrack.ISRC);

            AssertTracksAreEqual(expectedTrack, actualTrack);
        }

        [TestMethod]
        public void ReadTrackByFingerprintInexistantIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track = GetTrack(name);
            modelService.InsertTrack(track);
            Assert.AreEqual(0, modelService.ReadTrackByFingerprint(int.MinValue).Count);
        }

        [TestMethod]
        public void ReadTrackByFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track expectedTrack = GetTrack(name);
            modelService.InsertTrack(expectedTrack);
            const int FakeId = int.MinValue;
            Fingerprint fingerprint = new Fingerprint(GenericFingerprint, expectedTrack.Id, int.MinValue);
            modelService.InsertFingerprint(fingerprint);
            Assert.AreNotEqual(FakeId, fingerprint.Id);
            var list = modelService.ReadTrackByFingerprint(fingerprint.Id);
            Track actualTrack = list.FirstOrDefault(temp => temp.Id == expectedTrack.Id);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(expectedTrack, actualTrack);
         }

        [TestMethod]
        public void DeleteCollectionOfTracksTest()
        {
            List<Track> tracks = GetRandomListOfTracks(10);
            modelService.InsertTrack(tracks);
            var allTracks = modelService.ReadTracks();
            Assert.IsTrue(allTracks.Count > 0);
            modelService.DeleteTrack(allTracks);
            Assert.IsTrue(modelService.ReadTracks().Count == 0);
        }

        [TestMethod]
        public void DeleteOneTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track = GetTrack(name);
            modelService.InsertTrack(track);
            modelService.DeleteTrack(track);
            Assert.IsNull(modelService.ReadTrackById(track.Id));
        }

        [TestMethod]
        public void DeleteCollectionOfTracksIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track = GetTrack(name);
            modelService.InsertTrack(track);
            Assert.AreNotEqual(int.MinValue, track.Id);
            modelService.DeleteTrack(track.Id);
            Assert.AreEqual(null, modelService.ReadTrackById(track.Id));
        }

        [TestMethod]
        public void DeleteTrackListOfIdTest()
        {
            var expectedTracks = GetRandomListOfTracks(10);
            modelService.InsertTrack(expectedTracks);
            foreach (Track track in expectedTracks)
            {
                Assert.AreNotEqual(int.MinValue, track.Id);
            }

            var allTracks = modelService.ReadTracks();
            Assert.IsTrue(allTracks.Count > 0);
            modelService.DeleteTrack(allTracks.Select(t => t.Id));
            Assert.IsTrue(modelService.ReadTracks().Count == 0);
        }

        [TestMethod]
        public void DeleteHashBinsAndSubfingerprintsOnTrackDelete()
        {
            const int StaticStride = 5115;
            const int SecondsToProcess = 20;
            const int StartAtSecond = 30;
            DefaultQueryConfiguration defaultQueryConfiguration = new DefaultQueryConfiguration();
            TagInfo tagInfo = tagService.GetTagInfo(PathToMp3);
            int releaseYear = tagInfo.Year;
            Track track = new Track(tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, releaseYear, (int)tagInfo.Duration);
            modelService.InsertTrack(track);

            var subFingerprints = fingerprintUnitBuilder.BuildAudioFingerprintingUnit()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .WithCustomAlgorithmConfiguration(config =>
                                            {
                                                config.Stride = new IncrementalStaticStride(StaticStride, config.SamplesPerFingerprint);
                                            })
                                            .FingerprintIt()
                                            .HashIt()
                                            .ForTrack(track.Id)
                                            .Result;

            modelService.InsertSubFingerprint(subFingerprints);
            var hashBins = new List<HashBinMinHash>();
            foreach (var subFingerprint in subFingerprints)
            {
                long[] groupedSubFingerprint = lshService.Hash(subFingerprint.Signature, defaultQueryConfiguration.NumberOfLSHTables, defaultQueryConfiguration.NumberOfMinHashesPerTable);
                for (int i = 0; i < groupedSubFingerprint.Length; i++)
                {
                    int tableNumber = i + 1;
                    hashBins.Add(new HashBinMinHash(groupedSubFingerprint[i], tableNumber, subFingerprint.Id));
                }
            }

            modelService.InsertHashBin(hashBins);
            var actualTrack = modelService.ReadTrackByISRC(tagInfo.ISRC);
            Assert.IsNotNull(actualTrack);
            AssertTracksAreEqual(track, actualTrack);
            foreach (var subFingerprint in subFingerprints)
            {
                long[] groupedSubFingerprint = lshService.Hash(subFingerprint.Signature, defaultQueryConfiguration.NumberOfLSHTables, defaultQueryConfiguration.NumberOfMinHashesPerTable);
                var result = modelService.ReadSubFingerprintsByHashBucketsHavingThreshold(groupedSubFingerprint, defaultQueryConfiguration.NumberOfLSHTables);
                var tupple = result.First();
                Assert.AreEqual(defaultQueryConfiguration.NumberOfLSHTables, tupple.Item2);
                var actualSufingerprint = tupple.Item1;
                AssertSubFingerprintsAreEqual(subFingerprint, actualSufingerprint);
            }

            // Act
            modelService.DeleteTrack(track);

            Assert.IsNull(modelService.ReadTrackByISRC(tagInfo.ISRC));
            foreach (var subFingerprint in subFingerprints)
            {
                long[] groupedSubFingerprint = lshService.Hash(subFingerprint.Signature, defaultQueryConfiguration.NumberOfLSHTables, defaultQueryConfiguration.NumberOfMinHashesPerTable);
                var result = modelService.ReadSubFingerprintsByHashBucketsHavingThreshold(groupedSubFingerprint, defaultQueryConfiguration.NumberOfLSHTables);
                Assert.IsTrue(!result.Any());
            }
        }

        #endregion

        #region Insert/Read/Delete Signature objects tests

        [TestMethod]
        public void InsertReadFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track = GetTrack(name);
            modelService.InsertTrack(track);
            Fingerprint fingerprint = new Fingerprint(GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(fingerprint);
            var allFingerprints = modelService.ReadFingerprints();
            List<int> fingerprintIds = allFingerprints.Select(temp => temp.Id).ToList();

            Assert.AreEqual(true, fingerprintIds.Contains(fingerprint.Id));

            List<Fingerprint> addList = new List<Fingerprint>();
            for (int i = 0; i < 10; i++)
            {
                addList.Add(new Fingerprint(GenericFingerprint, track.Id, 0));
            }

            modelService.InsertFingerprint(addList);
            allFingerprints = modelService.ReadFingerprints();
            fingerprintIds.Clear();
            fingerprintIds.AddRange(allFingerprints.Select(temp => temp.Id));
            
            foreach (Fingerprint finger in addList)
            {
                Assert.AreEqual(true, fingerprintIds.Contains(finger.Id));
            }
        }

        [TestMethod]
        public void ReadFingerprintByIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Track track = GetTrack(name);
            modelService.InsertTrack(track);
            Assert.AreNotEqual(FakeId, track.Id);
            Fingerprint fingerprint = new Fingerprint(GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(fingerprint);
            Assert.AreNotEqual(FakeId, fingerprint.Id);
            Fingerprint readFingerprintById = modelService.ReadFingerprintById(fingerprint.Id);
            Assert.AreEqual(fingerprint.Id, readFingerprintById.Id);
            Assert.AreEqual(fingerprint.Signature.Length, readFingerprintById.Signature.Length);
            for (int i = 0; i < fingerprint.Signature.Length; i++)
            {
                Assert.AreEqual(fingerprint.Signature[i], readFingerprintById.Signature[i]);
            }

            Assert.AreEqual(fingerprint.TrackId, readFingerprintById.TrackId);
        }

        [TestMethod]
        public void ReadFingerprintsByMultipleIdsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            List<Fingerprint> listOfFingers = new List<Fingerprint>();
            Track track = GetTrack(name);
            modelService.InsertTrack(track);
            const int Count = 100;
            List<int> listOfGuids = new List<int>();
            for (int i = 0; i < Count; i++)
            {
                listOfFingers.Add(new Fingerprint(GenericFingerprint, track.Id, 0));
            }

            modelService.InsertFingerprint(listOfFingers);
            listOfGuids.AddRange(listOfFingers.Select(f => f.Id));
            var readFingers = modelService.ReadFingerprintById(listOfGuids);
            Assert.AreEqual(readFingers.Count, listOfFingers.Count);
        }

        [TestMethod]
        public void ReadFingerprintByTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track = GetTrack(name);
            modelService.InsertTrack(track);
            Fingerprint fingerprint = new Fingerprint(GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(fingerprint);
            var list = modelService.ReadFingerprintsByTrackId(track.Id, 0);
            Fingerprint expectedFingerprint = list.FirstOrDefault(temp => temp.Id == fingerprint.Id);
            Assert.IsNotNull(expectedFingerprint);
            Assert.AreEqual(fingerprint.Id, expectedFingerprint.Id);
            Assert.AreEqual(fingerprint.Signature.Length, expectedFingerprint.Signature.Length);
            for (int i = 0; i < fingerprint.Signature.Length; i++)
            {
                Assert.AreEqual(fingerprint.Signature[i], expectedFingerprint.Signature[i]);
            }

            Assert.AreEqual(fingerprint.TrackId, expectedFingerprint.TrackId);
        }

        [TestMethod]
        public void ReadFingerprintByMultipleTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track0 = GetTrack(name);
            modelService.InsertTrack(track0);
            Track track1 = GetTrack(name);
            modelService.InsertTrack(track1);
            Track track2 = GetTrack(name);
            modelService.InsertTrack(track2);

            Fingerprint f0 = new Fingerprint(GenericFingerprint, track0.Id, 0);
            modelService.InsertFingerprint(f0);
            Fingerprint f1 = new Fingerprint(GenericFingerprint, track0.Id, 1);
            modelService.InsertFingerprint(f1);
            Fingerprint f2 = new Fingerprint(GenericFingerprint, track1.Id, 2);
            modelService.InsertFingerprint(f2);
            Fingerprint f3 = new Fingerprint(GenericFingerprint, track1.Id, 3);
            modelService.InsertFingerprint(f3);
            Fingerprint f4 = new Fingerprint(GenericFingerprint, track2.Id, 4);
            modelService.InsertFingerprint(f4);
            Fingerprint f5 = new Fingerprint(GenericFingerprint, track2.Id, 5);
            modelService.InsertFingerprint(f5);
            Fingerprint f6 = new Fingerprint(GenericFingerprint, track0.Id, 6);
            modelService.InsertFingerprint(f6);
            Fingerprint f7 = new Fingerprint(GenericFingerprint, track1.Id, 7);
            modelService.InsertFingerprint(f7);
            Fingerprint f8 = new Fingerprint(GenericFingerprint, track2.Id, 8);
            modelService.InsertFingerprint(f8);

            var dict =
                modelService.ReadFingerprintsByMultipleTrackId(new List<Track> { track0, track1, track2 }, 0);

            Assert.AreNotEqual(null, dict);
            Assert.AreEqual(3, dict.Keys.Count);
            foreach (var item in dict)
            {
                Assert.AreEqual(3, item.Value.Count);
            }

            Assert.AreEqual(true, dict.ContainsKey(track0.Id));
            Assert.AreEqual(true, dict.ContainsKey(track1.Id));
            Assert.AreEqual(true, dict.ContainsKey(track2.Id));

            foreach (var pair in dict)
            {
                Assert.AreEqual(3, pair.Value.Count);
            }
        }

        [TestMethod]
        public void ReadFingerprintByMultipleTrackIdTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int NumberOfTracks = 1153;
            const int NumberOfFingerprintsPerTrack = 10;

            List<Track> listTrack = new List<Track>();
            List<Fingerprint> listOfFingerprints = new List<Fingerprint>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                Track track0 = GetTrack(name);
                listTrack.Add(track0);
                modelService.InsertTrack(track0);
                for (int j = 0; j < NumberOfFingerprintsPerTrack; j++)
                {
                    Fingerprint f0 = new Fingerprint(GenericFingerprint, track0.Id, 0);
                    listOfFingerprints.Add(f0);
                }
            }

            modelService.InsertFingerprint(listOfFingerprints);

            var dict = modelService.ReadFingerprintsByMultipleTrackId(listTrack, 0);

            Assert.AreNotEqual(null, dict);
            Assert.AreEqual(NumberOfTracks, dict.Keys.Count);
            foreach (Track track in listTrack)
            {
                Assert.AreEqual(true, dict.ContainsKey(track.Id));
                Assert.AreEqual(NumberOfFingerprintsPerTrack, dict[track.Id].Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void InsertFingerprintWithBadTrackIdForeignKeyreference()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track track = GetTrack(name);
            Fingerprint fingerprint = new Fingerprint(GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(fingerprint);
        }

        #endregion

        #region False positive analisys

        [TestMethod]
        public void DeleteTrackFalseTest()
        {
            IEnumerable<int> collection = new List<int> { 0 };
            const int Expected = 0;
            int actual = modelService.DeleteTrack(collection);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void DeleteTrackFalseTest1()
        {
            Track track = new Track();
            const int Expected = 0;
            int actual = modelService.DeleteTrack(track);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void DeleteTrackFalseTest2()
        {
            IEnumerable<Track> collection = new List<Track> { new Track() };
            const int Expected = 0;
            int actual = modelService.DeleteTrack(collection);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void DeleteTrackFalseTest3()
        {
            const int TrackId = 0;
            const int Expected = 0;
            int actual = modelService.DeleteTrack(TrackId);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReadFingerprintByIdFalseTest()
        {
            const int Id = 0;
            Fingerprint actual = modelService.ReadFingerprintById(Id);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintByIdFalseTest1()
        {
            IEnumerable<int> ids = new List<int> { 0 };
            var actual = modelService.ReadFingerprintById(ids);
            Assert.IsTrue(actual.Count == 0);
        }

        [TestMethod]
        public void ReadFingerprintsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Track t = GetTrack(name);
            modelService.InsertTrack(t);
            Fingerprint f = new Fingerprint(GenericFingerprint, t.Id, 10);
            modelService.InsertFingerprint(f);
            var actual = modelService.ReadFingerprints();
            Assert.IsTrue(actual.Count >= 1);
        }

        [TestMethod]
        public void ReadFingerprintsByMultipleTrackIdTest()
        {
            List<Track> tracks = new List<Track> { new Track(), new Track(), new Track(), new Track() };
            const int NumberOfFingerprintsToRead = 10;
            var actual = modelService.ReadFingerprintsByMultipleTrackId(tracks, NumberOfFingerprintsToRead);
            Assert.IsTrue(actual.Count == 0);
        }

        [TestMethod]
        public void ReadFingerprintsByTrackIdTest()
        {
            const int TrackId = 0;
            const int NumberOfFingerprintsToRead = 10;
            var actual = modelService.ReadFingerprintsByTrackId(TrackId, NumberOfFingerprintsToRead);
            Assert.IsTrue(actual.Count == 0);
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitleNameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            string artist = name;
            string title = name;
            Track actual = modelService.ReadTrackByArtistAndTitleName(artist, title);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadTrackByNonExistentFingerprintId()
        {
            const int Id = 0;
            Assert.IsTrue(modelService.ReadTrackByFingerprint(Id).Count == 0);
        }

        [TestMethod]
        public void ReadTrackByIdTest()
        {
            const int Id = 0;
            Track actual = modelService.ReadTrackById(Id);
            Assert.IsNull(actual);
        }

        #endregion

        private List<Track> GetRandomListOfTracks(int count)
        {
            var tracks = new List<Track>();
            for (int i = 0; i < count; i++)
            {
                Track track = GetTrack(MethodBase.GetCurrentMethod().Name);
                track.ISRC += i;
                tracks.Add(track);
            }

            return tracks;
        }

        private Track GetTrack(string methodName)
        {
            string isrc = Guid.NewGuid().ToString();
            string albumName = methodName + "album name";
            string artist = methodName + "artist";
            string title = methodName + "title";
            return new Track(isrc, artist, title, albumName, 1986, 360);
        }

        private void AssertTracksAreEqual(Track expectedTrack, Track actualTrack)
        {
            Assert.AreEqual(expectedTrack.Id, actualTrack.Id);
            Assert.AreEqual(expectedTrack.Album, actualTrack.Album);
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.TrackLengthSec, actualTrack.TrackLengthSec);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
        }

        private void AssertSubFingerprintsAreEqual(SubFingerprint subFingerprint, SubFingerprint actualSufingerprint)
        {
            Assert.AreEqual(subFingerprint.Id, actualSufingerprint.Id);
            Assert.AreEqual(subFingerprint.TrackId, actualSufingerprint.TrackId);
            for (var i = 0; i < subFingerprint.Signature.Length; i++)
            {
                Assert.AreEqual(subFingerprint.Signature[i], actualSufingerprint.Signature[i]);
            }
        }
    }
}
