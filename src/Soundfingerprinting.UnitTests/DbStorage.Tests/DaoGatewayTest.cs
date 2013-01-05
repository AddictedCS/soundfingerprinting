namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.Hashing;

    [TestClass]
    public class DaoGatewayTest : BaseTest
    {
        private readonly string connectionstring;

        private readonly DaoGateway dalManager;

        public DaoGatewayTest()
        {
            connectionstring = ConnectionString;
            dalManager = new DaoGateway(connectionstring);
        }

        #region Insert/Read/Delete Album objects tests

        [TestMethod]
        public void InsertReadAlbumTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoGateway manager = new DaoGateway(connectionstring);
            const int FakeId = int.MinValue;
            Album album = new Album { Id = FakeId, Name = name, ReleaseYear = 1986 };
            manager.InsertAlbum(album);
            Assert.AreNotEqual(FakeId, album.Id);
            List<Album> albums = manager.ReadAlbums(); // read all albums
            bool found = false;
            int id = 0;

            if (albums.Any(a => a.Id == album.Id))
            {
                found = true;
                id = album.Id;
            }

            Assert.IsTrue(found); // check if it was inserted
            Album b = manager.ReadAlbumById(id);
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

            manager.InsertAlbum(listAlbums); /*Insert a list of albums*/
            foreach (Album item in listAlbums)
            {
                Assert.AreNotEqual(FakeId, item.Id);
                lId.Add(item.Id);
            }

            List<Album> readAlbums = manager.ReadAlbums(); /*read all albums*/
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
            dalManager.InsertAlbum(new List<Album>());
        }

        [TestMethod]
        public void ReadAlbumByNameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            string albumName = Guid.NewGuid().ToString();
            Album album = new Album(int.MinValue, albumName);
            dalManager.InsertAlbum(album);
            Assert.AreNotEqual(int.MinValue, album.Id);
            Album readAlbum = dalManager.ReadAlbumByName(albumName);
            Assert.IsNotNull(readAlbum);
            Assert.AreEqual(album.Id, readAlbum.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertAlbumNull()
        {
            Album a = null;
            dalManager.InsertAlbum(a);
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
            Album album = dalManager.ReadUnknownAlbum();
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
            DaoGateway manager = dalManager;
            Album album = new Album(int.MinValue, name, 1986);
            manager.InsertAlbum(album);
            Assert.AreNotEqual(int.MinValue, album.Id);
            Track track = new Track(int.MinValue, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Assert.AreNotEqual(int.MinValue, track.Id);
            List<Track> listOfTracks = manager.ReadTracks();
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
            Track t = manager.ReadTrackById(track.Id);
            Assert.AreEqual(track.Id, t.Id);
            Assert.AreEqual(track.AlbumId, t.AlbumId);
            Assert.AreEqual(track.Artist, t.Artist);
            Assert.AreEqual(track.Title, t.Title);
            Assert.AreEqual(track.TrackLength, t.TrackLength);
            List<Album> listAlbums = new List<Album>();
            for (int i = 0; i < 10; i++)
            {
                Album a = new Album(int.MinValue, name + i, i + 1986);
                listAlbums.Add(a);
            }
            manager.InsertAlbum(listAlbums);
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

            manager.InsertTrack(listTracks);
            foreach (Track item in listTracks)
            {
                Assert.AreNotEqual(int.MinValue, item.Id);
            }

            List<Track> readTracks = manager.ReadTracks();
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
            Album album = dalManager.ReadUnknownAlbum();
            const int Count = 10;
            const int FakeId = int.MinValue;
            List<Track> tracks = new List<Track>();
            for (int i = 0; i < Count; i++) tracks.Add(new Track(FakeId, name, name, album.Id));
            dalManager.InsertTrack(tracks);
            foreach (Track item in tracks) Assert.AreNotEqual(FakeId, item.Id);
            Dictionary<Track, int> result = dalManager.ReadDuplicatedTracks();
            Assert.IsNotNull(result);
            if (result.Any(item => item.Key.Artist == name && item.Key.Title == name))
            {
                Assert.AreEqual(Count, result.Count);
            }

            dalManager.DeleteTrack(tracks);
        }

        [TestMethod]
        public void ReadTrackByArtistAndTitle()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album album = dalManager.ReadUnknownAlbum();
            string artistName = name;
            string titleName = name;
            const int FakeId = int.MinValue;
            Track track = new Track(FakeId, artistName, titleName, album.Id);
            dalManager.InsertTrack(track);
            Assert.AreNotEqual(FakeId, track.Id);
            Track readTrack = dalManager.ReadTrackByArtistAndTitleName(artistName, titleName);
            Assert.IsNotNull(readTrack);
            Assert.AreEqual(artistName, readTrack.Artist);
            Assert.AreEqual(titleName, readTrack.Title);
        }

        [TestMethod]
        public void ReadTrackByFingerprintInexistantIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoGateway manager = dalManager;
            const int FakeId = int.MinValue;
            Album album = new Album(FakeId, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(FakeId, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Fingerprint f = new Fingerprint(FakeId, GenericFingerprint, track.Id, 0);
            List<Track> list = manager.ReadTrackByFingerprint(f.Id);
            Assert.AreEqual(null, list);
        }

        [TestMethod]
        public void ReadTrackIdByHashBinAndHashTableTest()
        {
            Album album = dalManager.ReadUnknownAlbum();
            Track track = new Track(
                0, "#ReadTrackIdByHashBinAndHashTableTest", "#ReadTrackIdByHashBinAndHashTableTest", album.Id);
            dalManager.InsertTrack(track);
            List<HashBinNeuralHasher> list = new List<HashBinNeuralHasher>();
            const int Count = 20;
            long[] hashbins = new long[Count];
            int[] hashtables = new int[Count];
            Random rand = new Random();
            const int FakeId = int.MinValue;
            for (int i = 0; i < Count; i++)
            {
                hashbins[i] = rand.Next();
                hashtables[i] = i;
                list.Add(new HashBinNeuralHasher(FakeId, hashbins[i], hashtables[i], track.Id));
            }

            dalManager.InsertHashBin(list);
            foreach (HashBinNeuralHasher item in list)
            {
                Assert.AreNotEqual(FakeId, item.Id);
            }

            Dictionary<int, int> result = dalManager.ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasher(
                hashbins, hashtables);
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.ContainsKey(track.Id));
            Assert.AreEqual(Count, result[track.Id]);
        }

        [TestMethod]
        public void ReadTrackByFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoGateway manager = dalManager;
            Album album = new Album(int.MinValue, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(int.MinValue, name, name, album.Id, 360);
            manager.InsertTrack(track);
            const int FakeId = int.MinValue;
            Fingerprint f = new Fingerprint(FakeId, GenericFingerprint, track.Id, int.MinValue);
            manager.InsertFingerprint(f);
            Assert.AreNotEqual(FakeId, f.Id);
            List<Track> list = manager.ReadTrackByFingerprint(f.Id);
            Track readT = list.FirstOrDefault(temp => temp.Id == track.Id);
            Assert.AreNotEqual(null, readT);
            Assert.AreEqual(track.Id, readT.Id);
            Assert.AreEqual(track.AlbumId, readT.AlbumId);
            Assert.AreEqual(track.Artist, readT.Artist);
            Assert.AreEqual(track.Title, readT.Title);
            Assert.AreEqual(track.TrackLength, readT.TrackLength);
        }

        [TestMethod]
        public void DeleteTrackListTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album album = new Album(int.MinValue, name, 1986);
            dalManager.InsertAlbum(album);
            List<Track> listTracks = new List<Track>();

            List<int> lId = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(int.MinValue, name + i, name + i, album.Id);
                listTracks.Add(a);
            }
            dalManager.InsertTrack(listTracks);
            List<Fingerprint> fingerprintList = new List<Fingerprint>();
            for (int j = 0; j < 100; j++)
            {
                fingerprintList.Add(new Fingerprint(0, GenericFingerprint, listTracks[j / 10].Id, 0));
            }

            dalManager.InsertFingerprint(fingerprintList);
            List<Track> listOfTracks = dalManager.ReadTracks();
            Assert.AreEqual(0, (listOfTracks == null) ? 0 : listOfTracks.Count);
            Album ab = dalManager.ReadAlbumById(album.Id);
            Assert.AreEqual(0, (ab == null) ? 0 : 1);
            List<Fingerprint> list = dalManager.ReadFingerprints();
            Assert.AreEqual(0, (list == null) ? 0 : list.Count);
        }

        [TestMethod]
        public void DeleteTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = new Album(0, name, 1986);
            dalManager.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id);
            dalManager.InsertTrack(track);
            dalManager.DeleteTrack(track);
            Assert.AreEqual(null, dalManager.ReadAlbumById(album.Id));
            Assert.AreEqual(null, dalManager.ReadTrackById(track.Id));
        }

        [TestMethod]
        public void DeleteTrackByIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Album album = new Album(FakeId, name, 1986);
            dalManager.InsertAlbum(album);
            Assert.AreNotEqual(FakeId, album.Id);
            Track track = new Track(FakeId, name, name, album.Id);
            dalManager.InsertTrack(track);
            Assert.AreNotEqual(FakeId, track.Id);
            dalManager.DeleteTrack(track.Id);
            Assert.AreEqual(null, dalManager.ReadAlbumById(album.Id));
            Assert.AreEqual(null, dalManager.ReadTrackById(track.Id));

        }

        [TestMethod]
        public void DeleteTrackListOfIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            const int FakeId = int.MinValue;
            Album album = new Album(FakeId, name, 1986);
            dalManager.InsertAlbum(album);
            Assert.AreNotEqual(FakeId, album.Id);
            List<Track> listTracks = new List<Track>();

            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(FakeId, name + i, name + i, album.Id);
                listTracks.Add(a);
            }

            dalManager.InsertTrack(listTracks);
            foreach (Track track in listTracks) Assert.AreNotEqual(FakeId, track.Id);
            List<Fingerprint> fingerprintList = new List<Fingerprint>();
            for (int j = 0; j < 100; j++) fingerprintList.Add(new Fingerprint(FakeId, GenericFingerprint, listTracks[j / 10].Id, 0));

            dalManager.InsertFingerprint(fingerprintList);
            foreach (Fingerprint finger in fingerprintList) Assert.AreNotEqual(FakeId, finger.Id);
            List<Track> listOfTracks = dalManager.ReadTracks();
            List<int> lId = listOfTracks.Select(t => t.Id).ToList();
            listOfTracks = dalManager.ReadTracks();
            Assert.AreEqual(0, (listOfTracks == null) ? 0 : listOfTracks.Count);
            Album ab = dalManager.ReadAlbumById(album.Id);
            Assert.AreEqual(0, (ab == null) ? 0 : 1);
            List<Fingerprint> list = dalManager.ReadFingerprints();
            Assert.AreEqual(0, (list == null) ? 0 : list.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackNullParamTest()
        {
            Track t = null;
            dalManager.DeleteTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackNullListTest()
        {
            List<Track> t = null;
            dalManager.DeleteTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackEmptyListTest()
        {
            List<Track> t = new List<Track>();
            dalManager.DeleteTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteTrackNullListOfTracksTest()
        {
            List<int> t = null;
            dalManager.DeleteTrack(t);
        }

        #endregion

        #region Insert/Read/Delete Fingerprint objects tests

        [TestMethod]
        public void InsertReadFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoGateway manager = dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Fingerprint f = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            manager.InsertFingerprint(f);
            List<Fingerprint> allFingerprints = manager.ReadFingerprints();
            List<int> lGuid = allFingerprints.Select(temp => temp.Id).ToList();

            Assert.AreEqual(true, lGuid.Contains(f.Id));

            List<Fingerprint> addList = new List<Fingerprint>();
            for (int i = 0; i < 10; i++)
            {
                addList.Add(new Fingerprint(0, GenericFingerprint, track.Id, 0));
            }

            manager.InsertFingerprint(addList);
            allFingerprints = manager.ReadFingerprints();
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

            DaoGateway manager = dalManager;
            const int fakeId = int.MinValue;
            Album album = new Album(fakeId, name, 1986);
            manager.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album);
            Track track = new Track(fakeId, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Assert.AreNotEqual(fakeId, track.Id);
            Fingerprint f = new Fingerprint(fakeId, GenericFingerprint, track.Id, 0);
            manager.InsertFingerprint(f);
            Assert.AreNotEqual(fakeId, f.Id);
            Fingerprint readF = manager.ReadFingerprintById(f.Id);
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
            Album album = dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            dalManager.InsertTrack(track);
            const int Count = 100;
            List<int> listOfGuids = new List<int>();
            const int FakeId = int.MinValue;
            for (int i = 0; i < Count; i++)
            {
                listOfFingers.Add(new Fingerprint(FakeId, GenericFingerprint, track.Id, 0));
            }
            dalManager.InsertFingerprint(listOfFingers);
            listOfGuids.AddRange(listOfFingers.Select((f) => f.Id));
            List<Fingerprint> readFingers = dalManager.ReadFingerprintById(listOfGuids);
            Assert.AreEqual(readFingers.Count, listOfFingers.Count);
        }

        [TestMethod]
        public void ReadFingerprintByTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            DaoGateway manager = dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Fingerprint f = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            manager.InsertFingerprint(f);

            List<Fingerprint> list = manager.ReadFingerprintsByTrackId(track.Id, 0);
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

            DaoGateway manager = dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            Track track0 = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track0);
            Track track1 = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track1);
            Track track2 = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track2);

            Fingerprint f0 = new Fingerprint(0, GenericFingerprint, track0.Id, 0);
            manager.InsertFingerprint(f0);
            Fingerprint f1 = new Fingerprint(0, GenericFingerprint, track0.Id, 1);
            manager.InsertFingerprint(f1);
            Fingerprint f2 = new Fingerprint(0, GenericFingerprint, track1.Id, 2);
            manager.InsertFingerprint(f2);
            Fingerprint f3 = new Fingerprint(0, GenericFingerprint, track1.Id, 3);
            manager.InsertFingerprint(f3);
            Fingerprint f4 = new Fingerprint(0, GenericFingerprint, track2.Id, 4);
            manager.InsertFingerprint(f4);
            Fingerprint f5 = new Fingerprint(0, GenericFingerprint, track2.Id, 5);
            manager.InsertFingerprint(f5);
            Fingerprint f6 = new Fingerprint(0, GenericFingerprint, track0.Id, 6);
            manager.InsertFingerprint(f6);
            Fingerprint f7 = new Fingerprint(0, GenericFingerprint, track1.Id, 7);
            manager.InsertFingerprint(f7);
            Fingerprint f8 = new Fingerprint(0, GenericFingerprint, track2.Id, 8);
            manager.InsertFingerprint(f8);

            Dictionary<int, List<Fingerprint>> dict =
                manager.ReadFingerprintsByMultipleTrackId(new List<Track> { track0, track1, track2 }, 0);

            Assert.AreNotEqual(null, dict);
            Assert.AreEqual(3, dict.Keys.Count);
            foreach (KeyValuePair<int, List<Fingerprint>> item in dict)
            {
                Assert.AreEqual(3, item.Value.Count);
            }

            Assert.AreEqual(true, dict.ContainsKey(track0.Id));
            Assert.AreEqual(true, dict.ContainsKey(track1.Id));
            Assert.AreEqual(true, dict.ContainsKey(track2.Id));

            foreach (KeyValuePair<int, List<Fingerprint>> pair in dict)
            {
                Assert.AreEqual(3, pair.Value.Count);
            }
        }

        [TestMethod]
        public void ReadFingerprintByMultipleTrackIdTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            DaoGateway manager = dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            const int NumberOfTracks = 1153;
            const int NumberOfFingerprintsPerTrack = 10;

            List<Track> listTrack = new List<Track>();
            List<Fingerprint> listOfFingerprints = new List<Fingerprint>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                Track track0 = new Track(0, name, name, album.Id, 360);
                listTrack.Add(track0);
                manager.InsertTrack(track0);
                for (int j = 0; j < NumberOfFingerprintsPerTrack; j++)
                {
                    Fingerprint f0 = new Fingerprint(0, GenericFingerprint, track0.Id, 0);
                    listOfFingerprints.Add(f0);
                }
            }

            manager.InsertFingerprint(listOfFingerprints);

            Dictionary<int, List<Fingerprint>> dict = manager.ReadFingerprintsByMultipleTrackId(listTrack, 0);

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
            dalManager.InsertFingerprint(f);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertFingerprintNullTest()
        {
            dalManager.InsertFingerprint((Fingerprint)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertEmptyCollectionInFingerprints()
        {
            dalManager.InsertFingerprint(new List<Fingerprint>());
        }

        #endregion

        #region Insert/Read/Delete HashBin objects tests

        [TestMethod]
        public void InsertReadHashBinMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            dalManager.InsertFingerprint(finger);
            const long Hashbin = 100000;
            const int Hashtable = 20;
            HashBinMinHash hashbinminhash = new HashBinMinHash(0, Hashbin, Hashtable, track.Id, finger.Id);
            dalManager.InsertHashBin(hashbinminhash);
            Dictionary<int, List<HashBinMinHash>> result =
                dalManager.ReadFingerprintsByHashBucketAndHashTableLSH(new[] { Hashbin }, new[] { Hashtable });
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(true, result.ContainsKey(finger.Id));
            Assert.AreEqual(1, result[finger.Id].Count);
            Assert.AreEqual(hashbinminhash.Id, result[finger.Id][0].Id);
            dalManager.DeleteTrack(track.Id);
        }

        [TestMethod]
        public void InsertReadHashBinNeuralHasherTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album album = dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            dalManager.InsertFingerprint(finger);
            const long Hashbin = 100000;
            const int Hashtable = 20;
            HashBinNeuralHasher hashbinminhash = new HashBinNeuralHasher(0, Hashbin, Hashtable, track.Id);
            dalManager.InsertHashBin(hashbinminhash);
            List<int> result = dalManager.ReadTrackIdByHashBinAndHashTableNeuralHasher(Hashbin, Hashtable);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(track.Id, result[0]);
            dalManager.DeleteTrack(track.Id);
        }

        [TestMethod]
        public void InsertReadHashBinMinHashesTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album album = dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            dalManager.InsertFingerprint(finger);
            const long Hashbin = 100000;
            const int Hashtable = 20;
            List<HashBinMinHash> list = new List<HashBinMinHash>();
            const int Count = 20;
            for (int i = 0; i < Count; i++)
            {
                HashBinMinHash hashbinminhash = new HashBinMinHash(0, Hashbin, Hashtable, track.Id, finger.Id);
                list.Add(hashbinminhash);
            }

            dalManager.InsertHashBin(list);
            Dictionary<int, List<HashBinMinHash>> result =
                dalManager.ReadFingerprintsByHashBucketAndHashTableLSH(new[] { Hashbin }, new[] { Hashtable });
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(true, result.ContainsKey(finger.Id));
            Assert.AreEqual(Count, result[finger.Id].Count);
            dalManager.DeleteTrack(track.Id);
        }

        [TestMethod]
        public void InsertReadHashBinNeuralHashesTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            Album album = dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GenericFingerprint, track.Id, 0);
            dalManager.InsertFingerprint(finger);
            const long Hashbin = 100000;
            const int Hashtable = 20;
            List<HashBinNeuralHasher> list = new List<HashBinNeuralHasher>();
            const int Count = 20;
            for (int i = 0; i < Count; i++)
            {
                HashBinNeuralHasher hashbinminhash = new HashBinNeuralHasher(0, Hashbin, Hashtable, track.Id);
                list.Add(hashbinminhash);
            }

            dalManager.InsertHashBin(list);
            List<int> result = dalManager.ReadTrackIdByHashBinAndHashTableNeuralHasher(Hashbin, Hashtable);
            Assert.IsNotNull(result);
            Assert.AreEqual(Count, result.Count);
            dalManager.DeleteTrack(track.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertNullHashBinTest()
        {
            dalManager.InsertHashBin(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertEmptyCollectionInHashBinMinHash()
        {
            List<HashBinMinHash> list = new List<HashBinMinHash>();
            dalManager.InsertHashBin(list);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertTrackNullTest()
        {
            Track t = null;
            dalManager.InsertTrack(t);
        }

        [TestMethod]
        [ExpectedException(typeof(FingerprintEntityException))]
        public void CheckTrackLengthConstraints()
        {
            Track track = new Track { TrackLength = int.MinValue };
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void InsertTrackWithBadAlbumIdForeignKeyReference()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            Album a = new Album(int.MinValue, name, 1990);
            Track track = new Track(int.MinValue, name, name, a.Id);
            dalManager.InsertTrack(track);
        }

        #endregion

        #region False positive analisys

        [TestMethod]
        public void DaoGatewayConstructorTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            Assert.IsNotNull(target);
        }

        ///<summary>
        ///  A test for DeleteTrack. Delete a list of not-existent track, and check if 0 elements have been changed in the DB
        ///</summary>
        [TestMethod]
        public void DeleteTrackFalseTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            IEnumerable<int> collection = new List<int> { 0 };
            const int Expected = 0;
            int actual = target.DeleteTrack(collection);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void DeleteTrackFalseTest1()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            Track track = new Track();
            const int Expected = 0;
            int actual = target.DeleteTrack(track);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void DeleteTrackFalseTest2()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            IEnumerable<Track> collection = new List<Track> { new Track() };
            const int Expected = 0;
            int actual = target.DeleteTrack(collection);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void DeleteTrackFalseTest3()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            const int TrackId = 0;
            const int Expected = 0;
            int actual = target.DeleteTrack(TrackId);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReadAlbumByIdFalseTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            const int Id = 0;
            Album actual = target.ReadAlbumById(Id);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadAlbumByNameFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoGateway target = new DaoGateway(connectionstring);
            Album actual = target.ReadAlbumByName(name);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintByIdFalseTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            const int Id = 0;
            Fingerprint actual = target.ReadFingerprintById(Id);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintByIdFalseTest1()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            IEnumerable<int> ids = new List<int> { 0 };
            List<Fingerprint> actual = target.ReadFingerprintById(ids);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintByIdConcurrentFalseTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            IEnumerable<int> ids = new List<int> { 0 };
            List<Fingerprint> actual = target.ReadFingerprintByIdConcurrent(ids);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintIdByHashBinAndHashTableMinHashFalseTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            long[] hashBins = new long[] { 1, 2, 3, 4, 5, 6 };
            int[] hashTables = new[] { 100, 101, 102, 103, 104, 105 };
            Dictionary<int, List<HashBinMinHash>> actual = target.ReadFingerprintsByHashBucketAndHashTableLSH(
                hashBins, hashTables);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoGateway target = new DaoGateway(connectionstring);
            Album album = target.ReadUnknownAlbum();
            Track t = new Track(0, name, name, album.Id);
            target.InsertTrack(t);
            Fingerprint f = new Fingerprint(0, GenericFingerprint, t.Id, 10);
            target.InsertFingerprint(f);
            List<Fingerprint> actual = target.ReadFingerprints();
            Assert.IsTrue(actual.Count >= 1);
        }

        [TestMethod]
        public void ReadFingerprintsByMultipleTrackIdTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            List<Track> tracks = new List<Track> { new Track(), new Track(), new Track(), new Track() };
            const int NumberOfFingerprintsToRead = 10;
            Dictionary<int, List<Fingerprint>> actual = target.ReadFingerprintsByMultipleTrackId(
                tracks, NumberOfFingerprintsToRead);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadFingerprintsByTrackIdTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            const int TrackId = 0;
            const int NumberOfFingerprintsToRead = 10;
            List<Fingerprint> actual = target.ReadFingerprintsByTrackId(TrackId, NumberOfFingerprintsToRead);
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
            DaoGateway target = new DaoGateway(connectionstring);
            string artist = name;
            string title = name;
            Track actual = target.ReadTrackByArtistAndTitleName(artist, title);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadTrackByFingerprintFalseTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            const int Id = 0;
            List<Track> actual = target.ReadTrackByFingerprint(Id);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadTrackByIdTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            const int Id = 0;
            Track actual = target.ReadTrackById(Id);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadTrackIdByHashBinAndHashTableNeuralHasherTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            const long HashBin = 0;
            const int HashTable = 100;
            List<int> actual = target.ReadTrackIdByHashBinAndHashTableNeuralHasher(HashBin, HashTable);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasherTest()
        {
            DaoGateway target = new DaoGateway(connectionstring);
            long[] hashBins = new long[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int[] hashTables = new[] { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110 };
            Dictionary<int, int> actual = target.ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasher(
                hashBins, hashTables);
            Assert.IsNull(actual);
        }

        #endregion
    }
}