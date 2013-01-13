namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Dao.Entities;
    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.Hashing;

    [TestClass]
    public class ModelServiceTest : BaseTest
    {
        private readonly string connectionstring;

        private readonly ModelService modelService;

        public ModelServiceTest()
        {
            connectionstring = ConnectionString;
            modelService = new ModelService(
                new MsSqlDatabaseProviderFactory(new DefaultConnectionStringFactory()), new ModelBinderFactory());
        }

        #region Insert/Read/Delete Album objects tests

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here."),TestMethod]
        public void InsertReadAlbumTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Album album = new Album { Id = FakeId, Name = name, ReleaseYear = 1986 };
            modelService.InsertAlbum(album);
            Assert.AreNotEqual(FakeId, album.Id);
            var albums = modelService.ReadAlbums(); // read all albums
            bool found = false;
            int id = 0;

            if (albums.Any(a => a.Id == album.Id))
            {
                found = true;
                id = album.Id;
            }

            Assert.IsTrue(found); // check if it was inserted
            Album b = modelService.ReadAlbumById(id);
            Assert.AreEqual(id, b.Id);
            Assert.AreEqual(album.Name, b.Name);
            Assert.AreEqual(album.ReleaseYear, b.ReleaseYear);
            List<Album> listAlbums = new List<Album>();
            List<int> lId = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                Album a = new Album(FakeId, name + ":" + i, i + 1986);
                listAlbums.Add(a);
            }

            modelService.InsertAlbum(listAlbums); /*Insert a list of albums*/
            foreach (Album item in listAlbums)
            {
                Assert.AreNotEqual(FakeId, item.Id);
                lId.Add(item.Id);
            }

            var readAlbums = modelService.ReadAlbums(); /*read all albums*/
            List<int> lReadIds = readAlbums.Select(a => a.Id).ToList();
            foreach (int i in lId)
            {
                Assert.AreEqual(true, lReadIds.Contains(i));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertEmptyCollectionInAlbumsTest()
        {
            modelService.InsertAlbum(new List<Album>());
        }

        [TestMethod]
        public void ReadAlbumByNameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            string albumName = Guid.NewGuid().ToString();
            Album album = new Album(int.MinValue, albumName);
            modelService.InsertAlbum(album);
            Assert.AreNotEqual(int.MinValue, album.Id);
            Album readAlbum = modelService.ReadAlbumByName(albumName);
            Assert.IsNotNull(readAlbum);
            Assert.AreEqual(album.Id, readAlbum.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertAlbumNull()
        {
            Album a = null;
            modelService.InsertAlbum(a);
        }

        [TestMethod]
        [ExpectedException(typeof(FingerprintEntityException))]
        public void CheckReleaseYearConstraintsAlbum()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album a = new Album();
            Assert.AreEqual(true, !string.IsNullOrEmpty(a.Name));
            Assert.AreEqual(MinYear /*Default value*/, a.ReleaseYear);
            a.ReleaseYear = 0; /*Release Year (1500..2100]*/
        }

        [TestMethod]
        public void ReadUnknownAlbumTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = modelService.ReadUnknownAlbum();
            Assert.AreNotEqual(null, album);
            Assert.IsTrue(album.Id > 0);
            Assert.AreEqual("UNKNOWN", album.Name);
        }

        #endregion

        #region Insert/Read/Delete Track objects tests

        [TestMethod]
        public void InsertReadTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = new Album(int.MinValue, name, 1986);
            modelService.InsertAlbum(album);
            Assert.AreNotEqual(int.MinValue, album.Id);
            Track track = new Track(int.MinValue, name, name, album.Id, 360);
            modelService.InsertTrack(track);
            Assert.AreNotEqual(int.MinValue, track.Id);
            var listOfTracks = modelService.ReadTracks();
            bool found = false;
            foreach (Track temp in listOfTracks)
            {
                Assert.IsTrue(temp.Id > 0);
                if (temp.Id == track.Id)
                {
                    found = true;
                    break;
                }
            }
            Assert.AreEqual(true, found);
            Track t = modelService.ReadTrackById(track.Id);
            Assert.AreEqual(track.Id, t.Id);
            Assert.AreEqual(track.AlbumId, t.AlbumId);
            Assert.AreEqual(track.Artist, t.Artist);
            Assert.AreEqual(track.Title, t.Title);
            Assert.AreEqual(track.TrackLengthSec, t.TrackLengthSec);
            List<Album> listAlbums = new List<Album>();
            for (int i = 0; i < 10; i++)
            {
                Album a = new Album(int.MinValue, name + i, i + 1986);
                listAlbums.Add(a);
            }
            modelService.InsertAlbum(listAlbums);
            foreach (Album a in listAlbums)
            {
                Assert.AreNotEqual(int.MinValue, a.Id);
            }

            List<Track> listTracks = new List<Track>();
            List<int> lId = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(int.MinValue, name + i, name + i, listAlbums[i].Id);
                listTracks.Add(a);
            }

            modelService.InsertTrack(listTracks);
            foreach (Track item in listTracks)
            {
                Assert.AreNotEqual(int.MinValue, item.Id);
            }

            var readTracks = modelService.ReadTracks();
            List<int> lReadIds = readTracks.Select(a => a.Id).ToList();
            foreach (int i in lId)
            {
                Assert.AreEqual(true, lReadIds.Contains(i));
            }
        }

        [TestMethod]
        public void ReadDuplicatedTracksTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = modelService.ReadUnknownAlbum();
            const int Count = 10;
            const int FakeId = int.MinValue;
            List<Track> tracks = new List<Track>();
            for (int i = 0; i < Count; i++) tracks.Add(new Track(FakeId, name, name, album.Id));
            modelService.InsertTrack(tracks);
            foreach (Track item in tracks) Assert.AreNotEqual(FakeId, item.Id);
            var result = modelService.ReadDuplicatedTracks();
            Assert.IsNotNull(result);
            if (result.Any(item => item.Key.Artist == name && item.Key.Title == name))
            {
                Assert.AreEqual(Count, result.Count);
            }

            modelService.DeleteTrack(tracks);
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitle()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = modelService.ReadUnknownAlbum();
            string artistName = name;
            string titleName = name;
            const int FakeId = int.MinValue;
            Track track = new Track(FakeId, artistName, titleName, album.Id);
            modelService.InsertTrack(track);
            Assert.AreNotEqual(FakeId, track.Id);
            Track readTrack = modelService.ReadTrackByArtistAndTitleName(artistName, titleName);
            Assert.IsNotNull(readTrack);
            Assert.AreEqual(artistName, readTrack.Artist);
            Assert.AreEqual(titleName, readTrack.Title);
        }

        [TestMethod]
        public void ReadTrackByFingerprintInexistantIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Album album = new Album(FakeId, name, 1986);
            modelService.InsertAlbum(album);
            Track track = new Track(FakeId, name, name, album.Id, 360);
            modelService.InsertTrack(track);
            Fingerprint f = new Fingerprint(FakeId, GenericFingerprint, track.Id, 0);
            var list = modelService.ReadTrackByFingerprint(f.Id);
            Assert.AreEqual(null, list);
        }

        [TestMethod]
        public void ReadTrackByFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = new Album(int.MinValue, name, 1986);
            modelService.InsertAlbum(album);
            Track track = new Track(int.MinValue, name, name, album.Id, 360);
            modelService.InsertTrack(track);
            const int FakeId = int.MinValue;
            Fingerprint f = new Fingerprint(FakeId, GenericFingerprint, track.Id, int.MinValue);
            modelService.InsertFingerprint(f);
            Assert.AreNotEqual(FakeId, f.Id);
            var list = modelService.ReadTrackByFingerprint(f.Id);
            Track readT = list.FirstOrDefault(temp => temp.Id == track.Id);
            Assert.AreNotEqual(null, readT);
            Assert.AreEqual(track.Id, readT.Id);
            Assert.AreEqual(track.AlbumId, readT.AlbumId);
            Assert.AreEqual(track.Artist, readT.Artist);
            Assert.AreEqual(track.Title, readT.Title);
            Assert.AreEqual(track.TrackLengthSec, readT.TrackLengthSec);
        }

        [TestMethod]
        public void DeleteTrackListTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album album = new Album(int.MinValue, name, 1986);
            modelService.InsertAlbum(album);
            List<Track> listTracks = new List<Track>();

            List<int> lId = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(int.MinValue, name + i, name + i, album.Id);
                listTracks.Add(a);
            }
            modelService.InsertTrack(listTracks);
            List<Fingerprint> fingerprintList = new List<Fingerprint>();
            for (int j = 0; j < 100; j++)
            {
                fingerprintList.Add(new Fingerprint(0, GenericFingerprint, listTracks[j / 10].Id, 0));
            }

            modelService.InsertFingerprint(fingerprintList);
            var listOfTracks = modelService.ReadTracks();
            Assert.AreEqual(0, (listOfTracks == null) ? 0 : listOfTracks.Count);
            Album ab = modelService.ReadAlbumById(album.Id);
            Assert.AreEqual(0, (ab == null) ? 0 : 1);
            var list = modelService.ReadFingerprints();
            Assert.AreEqual(0, (list == null) ? 0 : list.Count);
        }

        [TestMethod]
        public void DeleteTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = new Album(0, name, 1986);
            modelService.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id);
            modelService.InsertTrack(track);
            modelService.DeleteTrack(track);
            Assert.AreEqual(null, modelService.ReadAlbumById(album.Id));
            Assert.AreEqual(null, modelService.ReadTrackById(track.Id));
        }

        [TestMethod]
        public void DeleteTrackByIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Album album = new Album(FakeId, name, 1986);
            modelService.InsertAlbum(album);
            Assert.AreNotEqual(FakeId, album.Id);
            Track track = new Track(FakeId, name, name, album.Id);
            modelService.InsertTrack(track);
            Assert.AreNotEqual(FakeId, track.Id);
            modelService.DeleteTrack(track.Id);
            Assert.AreEqual(null, modelService.ReadAlbumById(album.Id));
            Assert.AreEqual(null, modelService.ReadTrackById(track.Id));

        }

        [TestMethod]
        public void DeleteTrackListOfIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Album album = new Album(FakeId, name, 1986);
            modelService.InsertAlbum(album);
            Assert.AreNotEqual(FakeId, album.Id);
            List<Track> listTracks = new List<Track>();

            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(FakeId, name + i, name + i, album.Id);
                listTracks.Add(a);
            }

            modelService.InsertTrack(listTracks);
            foreach (Track track in listTracks) Assert.AreNotEqual(FakeId, track.Id);
            List<Fingerprint> fingerprintList = new List<Fingerprint>();
            for (int j = 0; j < 100; j++) fingerprintList.Add(new Fingerprint(FakeId, GenericFingerprint, listTracks[j / 10].Id, 0));

            modelService.InsertFingerprint(fingerprintList);
            foreach (Fingerprint finger in fingerprintList) Assert.AreNotEqual(FakeId, finger.Id);
            var listOfTracks = modelService.ReadTracks();
            List<int> lId = listOfTracks.Select(t => t.Id).ToList();
            listOfTracks = modelService.ReadTracks();
            Assert.AreEqual(0, (listOfTracks == null) ? 0 : listOfTracks.Count);
            Album ab = modelService.ReadAlbumById(album.Id);
            Assert.AreEqual(0, (ab == null) ? 0 : 1);
            var list = modelService.ReadFingerprints();
            Assert.AreEqual(0, (list == null) ? 0 : list.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackNullParamTest()
        {
            Track t = null;
            modelService.DeleteTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackNullListTest()
        {
            List<Track> t = null;
            modelService.DeleteTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackEmptyListTest()
        {
            List<Track> t = new List<Track>();
            modelService.DeleteTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackNullListOfTracksTest()
        {
            List<int> t = null;
            modelService.DeleteTrack(t);
        }

        #endregion

        #region Insert/Read/Delete Signature objects tests

        [TestMethod]
        public void InsertReadFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = new Album(0, name, 1986);
            modelService.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id, 360);
            modelService.InsertTrack(track);
            Fingerprint f = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(f);
            var allFingerprints = modelService.ReadFingerprints();
            List<int> lGuid = allFingerprints.Select(temp => temp.Id).ToList();

            Assert.AreEqual(true, lGuid.Contains(f.Id));

            List<Fingerprint> addList = new List<Fingerprint>();
            for (int i = 0; i < 10; i++)
            {
                addList.Add(new Fingerprint(0, GenericFingerprint, track.Id, 0));
            }

            modelService.InsertFingerprint(addList);
            allFingerprints = modelService.ReadFingerprints();
            lGuid.Clear();
            lGuid.AddRange(allFingerprints.Select(temp => temp.Id));
            
            foreach (Fingerprint finger in addList)
            {
                Assert.AreEqual(true, lGuid.Contains(finger.Id));
            }
        }

        [TestMethod]
        public void ReadFingerprintByIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            const int fakeId = int.MinValue;
            Album album = new Album(fakeId, name, 1986);
            modelService.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album);
            Track track = new Track(fakeId, name, name, album.Id, 360);
            modelService.InsertTrack(track);
            Assert.AreNotEqual(fakeId, track.Id);
            Fingerprint f = new Fingerprint(fakeId, GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(f);
            Assert.AreNotEqual(fakeId, f.Id);
            Fingerprint readF = modelService.ReadFingerprintById(f.Id);
            Assert.AreEqual(f.Id, readF.Id);
            Assert.AreEqual(f.Signature.Length, readF.Signature.Length);
            for (int i = 0; i < f.Signature.Length; i++)
            {
                Assert.AreEqual(f.Signature[i], readF.Signature[i]);
            }

            Assert.AreEqual(f.TrackId, readF.TrackId);
        }

        [TestMethod]
        public void ReadFingerprintsByMultipleIdsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            List<Fingerprint> listOfFingers = new List<Fingerprint>();
            Album album = modelService.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            modelService.InsertTrack(track);
            const int Count = 100;
            List<int> listOfGuids = new List<int>();
            const int FakeId = int.MinValue;
            for (int i = 0; i < Count; i++)
            {
                listOfFingers.Add(new Fingerprint(FakeId, GenericFingerprint, track.Id, 0));
            }
            modelService.InsertFingerprint(listOfFingers);
            listOfGuids.AddRange(listOfFingers.Select((f) => f.Id));
            var readFingers = modelService.ReadFingerprintById(listOfGuids);
            Assert.AreEqual(readFingers.Count, listOfFingers.Count);
        }

        [TestMethod]
        public void ReadFingerprintByTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album album = new Album(0, name, 1986);
            modelService.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id, 360);
            modelService.InsertTrack(track);
            Fingerprint f = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(f);

            var list = modelService.ReadFingerprintsByTrackId(track.Id, 0);
            Fingerprint readF = list.FirstOrDefault(temp => temp.Id == f.Id);
            Assert.AreNotEqual(null, readF);
            Assert.AreEqual(f.Id, readF.Id);
            Assert.AreEqual(f.Signature.Length, readF.Signature.Length);
            for (int i = 0; i < f.Signature.Length; i++)
            {
                Assert.AreEqual(f.Signature[i], readF.Signature[i]);
            }

            Assert.AreEqual(f.TrackId, readF.TrackId);
        }

        [TestMethod]
        public void ReadFingerprintByMultipleTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = new Album(0, name, 1986);
            modelService.InsertAlbum(album);
            Track track0 = new Track(0, name, name, album.Id, 360);
            modelService.InsertTrack(track0);
            Track track1 = new Track(0, name, name, album.Id, 360);
            modelService.InsertTrack(track1);
            Track track2 = new Track(0, name, name, album.Id, 360);
            modelService.InsertTrack(track2);

            Fingerprint f0 = new Fingerprint(0, GenericFingerprint, track0.Id, 0);
            modelService.InsertFingerprint(f0);
            Fingerprint f1 = new Fingerprint(0, GenericFingerprint, track0.Id, 1);
            modelService.InsertFingerprint(f1);
            Fingerprint f2 = new Fingerprint(0, GenericFingerprint, track1.Id, 2);
            modelService.InsertFingerprint(f2);
            Fingerprint f3 = new Fingerprint(0, GenericFingerprint, track1.Id, 3);
            modelService.InsertFingerprint(f3);
            Fingerprint f4 = new Fingerprint(0, GenericFingerprint, track2.Id, 4);
            modelService.InsertFingerprint(f4);
            Fingerprint f5 = new Fingerprint(0, GenericFingerprint, track2.Id, 5);
            modelService.InsertFingerprint(f5);
            Fingerprint f6 = new Fingerprint(0, GenericFingerprint, track0.Id, 6);
            modelService.InsertFingerprint(f6);
            Fingerprint f7 = new Fingerprint(0, GenericFingerprint, track1.Id, 7);
            modelService.InsertFingerprint(f7);
            Fingerprint f8 = new Fingerprint(0, GenericFingerprint, track2.Id, 8);
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

            Album album = new Album(0, name, 1986);
            modelService.InsertAlbum(album);
            const int NumberOfTracks = 1153;
            const int NumberOfFingerprintsPerTrack = 10;

            List<Track> listTrack = new List<Track>();
            List<Fingerprint> listOfFingerprints = new List<Fingerprint>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                Track track0 = new Track(0, name, name, album.Id, 360);
                listTrack.Add(track0);
                modelService.InsertTrack(track0);
                for (int j = 0; j < NumberOfFingerprintsPerTrack; j++)
                {
                    Fingerprint f0 = new Fingerprint(0, GenericFingerprint, track0.Id, 0);
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
            Album a = new Album(0, name, 1990);
            Track track = new Track(0, name, name, a.Id);
            Fingerprint f = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            modelService.InsertFingerprint(f);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertFingerprintNullTest()
        {
            modelService.InsertFingerprint((Fingerprint)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertEmptyCollectionInFingerprints()
        {
            modelService.InsertFingerprint(new List<Fingerprint>());
        }

        #endregion

        #region Insert/Read/Delete Bin objects tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertEmptyCollectionInHashBinMinHash()
        {
            List<HashBinMinHash> list = new List<HashBinMinHash>();
            modelService.InsertHashBin(list);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertTrackNullTest()
        {
            Track t = null;
            modelService.InsertTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(FingerprintEntityException))]
        public void CheckTrackLengthConstraints()
        {
            Track track = new Track { TrackLengthSec = int.MinValue };
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void InsertTrackWithBadAlbumIdForeignKeyReference()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album a = new Album(int.MinValue, name, 1990);
            Track track = new Track(int.MinValue, name, name, a.Id);
            modelService.InsertTrack(track);
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
        public void ReadAlbumByIdFalseTest()
        {
            const int Id = 0;
            Album actual = modelService.ReadAlbumById(Id);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadAlbumByNameFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album actual = modelService.ReadAlbumByName(name);
            Assert.IsNull(actual);
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
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = modelService.ReadUnknownAlbum();
            Track t = new Track(0, name, name, album.Id);
            modelService.InsertTrack(t);
            Fingerprint f = new Fingerprint(0, GenericFingerprint, t.Id, 10);
            modelService.InsertFingerprint(f);
            var actual = modelService.ReadFingerprints();
            Assert.IsTrue(actual.Count >= 1);
        }

        [TestMethod]
        public void ReadFingerprintsByMultipleTrackIdTest()
        {
            List<Track> tracks = new List<Track> { new Track(), new Track(), new Track(), new Track() };
            const int NumberOfFingerprintsToRead = 10;
            var actual = modelService.ReadFingerprintsByMultipleTrackId(
                tracks, NumberOfFingerprintsToRead);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintsByTrackIdTest()
        {
            const int TrackId = 0;
            const int NumberOfFingerprintsToRead = 10;
            var actual = modelService.ReadFingerprintsByTrackId(TrackId, NumberOfFingerprintsToRead);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadPermutationsTest()
        {
            IPermutations perms = new DbPermutations(connectionstring);
            int[][] actual = perms.GetPermutations();
            Assert.IsNull(actual);
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
        public void ReadTrackByFingerprintFalseTest()
        {
            const int Id = 0;
            var actual = modelService.ReadTrackByFingerprint(Id);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadTrackByIdTest()
        {
            const int Id = 0;
            Track actual = modelService.ReadTrackById(Id);
            Assert.IsNull(actual);
        }

        #endregion
    }
}