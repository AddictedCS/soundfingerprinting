// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
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

namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    /// <summary>
    ///   Dal Manager class test
    /// </summary>
    [TestClass]
    public class DaoGatewayTest : BaseTest
    {
        private readonly string _connectionstring;

        /// <summary>
        ///   Dal Fingerprint manager object
        /// </summary>
        private readonly DaoGateway _dalManager;

        public DaoGatewayTest()
        {
            _connectionstring = ConnectionString;
            _dalManager = new DaoGateway(_connectionstring);
        }

        #region Insert/Read/Delete Album objects tests

        /// <summary>
        ///   Perform Insert/Read album test
        /// </summary>
        [TestMethod]
        public void InsertReadAlbumTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = new DaoGateway(_connectionstring);
            const int fakeId = Int32.MinValue;
            Album album = new Album {Id = fakeId, Name = name, ReleaseYear = 1986};
            manager.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album.Id);
            List<Album> albums = manager.ReadAlbums(); //read all albums
            bool found = false;
            Int32 id = 0;

            if (albums.Any(a => a.Id == album.Id))
            {
                found = true;
                id = album.Id;
            }
            Assert.IsTrue(found); //check if it was inserted
            Album b = manager.ReadAlbumById(id);
            Assert.AreEqual(id, b.Id);
            Assert.AreEqual(album.Name, b.Name);
            Assert.AreEqual(album.ReleaseYear, b.ReleaseYear);
            List<Album> listAlbums = new List<Album>();
            List<Int32> lId = new List<Int32>();
            for (int i = 0; i < 10; i++)
            {
                Album a = new Album(fakeId, name + ":" + i, i + 1986);
                listAlbums.Add(a);
            }

            manager.InsertAlbum(listAlbums); /*Insert a list of albums*/
            foreach (Album item in listAlbums)
            {
                Assert.AreNotEqual(fakeId, item.Id);
                lId.Add(item.Id);
            }
            List<Album> readAlbums = manager.ReadAlbums(); /*read all albums*/
            List<Int32> lReadIds = readAlbums.Select(a => a.Id).ToList();
            foreach (Int32 i in lId) /*for each Id from inserted album check if it exists in the all albums*/
            {
                if (Bswitch.Enabled)
                    Trace.WriteLine("#InsertReadAlbumTest::" + i);
                Assert.AreEqual(true, lReadIds.Contains(i));
            }
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert an empty collection in the database Collection.Count = 0
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void InsertEmptyCollectionInAlbumsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            _dalManager.InsertAlbum(new List<Album>());

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read album by name test
        /// </summary>
        [TestMethod]
        public void ReadAlbumByNameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            string albumName = Guid.NewGuid().ToString();
            const int fakeId = Int32.MinValue;
            Album album = new Album(fakeId, albumName);
            _dalManager.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album.Id);
            Album readAlbum = _dalManager.ReadAlbumByName(albumName);
            Assert.IsNotNull(readAlbum);
            Assert.AreEqual(album.Id, readAlbum.Id);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }


        /// <summary>
        ///   Insert null album in the database
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void InsertAlbumNull()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album a = null;
            _dalManager.InsertAlbum(a);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Release Year of an album should be between the range [1900:2100]
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (FingerprintEntityException))]
        public void CheckReleaseYearConstraintsAlbum()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album a = new Album();
            Assert.AreEqual(true, !String.IsNullOrEmpty(a.Name));
            Assert.AreEqual(MIN_YEAR /*Default value*/, a.ReleaseYear);

            a.ReleaseYear = 0; /*Release Year (1500..2100]*/

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }


        /// <summary>
        ///   Read UNKNOWN album from the database
        /// </summary>
        [TestMethod]
        public void ReadUnknownAlbumTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            Assert.AreNotEqual(null, album);
            Assert.IsTrue(album.Id > 0);
            Assert.AreEqual("UNKNOWN", album.Name);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        #endregion

        #region Insert/Read/Delete Track objects tests

        /// <summary>
        ///   Insert/Read track test
        /// </summary>
        [TestMethod]
        public void InsertReadTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            const int fakeId = Int32.MinValue;
            Album album = new Album(fakeId, name, 1986);
            manager.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album.Id);
            Track track = new Track(fakeId, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Assert.AreNotEqual(fakeId, track.Id);
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
                Album a = new Album(fakeId, name + i, i + 1986);
                listAlbums.Add(a);
            }
            manager.InsertAlbum(listAlbums);
            foreach (Album a in listAlbums)
                Assert.AreNotEqual(fakeId, a.Id);
            List<Track> listTracks = new List<Track>();
            List<Int32> lId = new List<Int32>();
            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(fakeId, name + i, name + i, listAlbums[i].Id);
                listTracks.Add(a);
            }
            manager.InsertTrack(listTracks);
            foreach (Track item in listTracks)
                Assert.AreNotEqual(fakeId, item.Id);
            List<Track> readTracks = manager.ReadTracks();
            List<Int32> lReadIds = readTracks.Select(a => a.Id).ToList();
            foreach (Int32 i in lId)
                Assert.AreEqual(true, lReadIds.Contains(i));

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read duplicated tracks test
        /// </summary>
        [TestMethod]
        public void ReadDuplicatedTracksTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            const int count = 10;
            const int fakeId = Int32.MinValue;
            List<Track> tracks = new List<Track>();
            for (int i = 0; i < count; i++)
                tracks.Add(new Track(fakeId, name, name, album.Id));
            _dalManager.InsertTrack(tracks);
            foreach (Track item in tracks)
                Assert.AreNotEqual(fakeId, item.Id);
            Dictionary<Track, int> result = _dalManager.ReadDuplicatedTracks();
            Assert.IsNotNull(result);
            if (result.Any(item => item.Key.Artist == name && item.Key.Title == name))
            {
                Assert.AreEqual(count, result.Count);
            }
            _dalManager.DeleteTrack(tracks);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read track by artist and title
        /// </summary>
        [TestMethod]
        public void ReadTrackByArtistAndTitle()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            string artistName = name;
            string titleName = name;
            const int fakeId = Int32.MinValue;
            Track track = new Track(fakeId, artistName, titleName, album.Id);
            _dalManager.InsertTrack(track);
            Assert.AreNotEqual(fakeId, track.Id);
            Track readTrack = _dalManager.ReadTrackByArtistAndTitleName(artistName, titleName);
            Assert.IsNotNull(readTrack);
            Assert.AreEqual(artistName, readTrack.Artist);
            Assert.AreEqual(titleName, readTrack.Title);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read Track by inexistent Fingerprint identifier
        /// </summary>
        [TestMethod]
        public void ReadTrackByFingerprintInexistantIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            const int fakeId = Int32.MinValue;
            Album album = new Album(fakeId, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(fakeId, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Fingerprint f = new Fingerprint(fakeId, GENERIC_FINGERPRINT, track.Id, 0);
            List<Track> list = manager.ReadTrackByFingerprint(f.Id);
            Assert.AreEqual(null, list);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read Track id by hash-bin and hash-table test
        /// </summary>
        [TestMethod]
        public void ReadTrackIdByHashBinAndHashTableTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            Track track = new Track(0, "#ReadTrackIdByHashBinAndHashTableTest", "#ReadTrackIdByHashBinAndHashTableTest", album.Id);
            _dalManager.InsertTrack(track);
            List<HashBinNeuralHasher> list = new List<HashBinNeuralHasher>();
            const int count = 20;
            long[] hashbins = new long[count];
            int[] hashtables = new int[count];
            Random rand = new Random();
            const int fakeId = Int32.MinValue;
            for (int i = 0; i < count; i++)
            {
                hashbins[i] = rand.Next();
                hashtables[i] = i;
                list.Add(new HashBinNeuralHasher(fakeId, hashbins[i], hashtables[i], track.Id));
            }
            _dalManager.InsertHashBin(list);
            foreach (HashBinNeuralHasher item in list)
                Assert.AreNotEqual(fakeId, item.Id);
            Dictionary<Int32, int> result = _dalManager.ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasher(hashbins, hashtables);
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.ContainsKey(track.Id));
            Assert.AreEqual(count, result[track.Id]);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read track by fingerprint test
        /// </summary>
        [TestMethod]
        public void ReadTrackByFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            Album album = new Album(Int32.MinValue, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(Int32.MinValue, name, name, album.Id, 360);
            manager.InsertTrack(track);
            const int fakeId = Int32.MinValue;
            Fingerprint f = new Fingerprint(fakeId, GENERIC_FINGERPRINT, track.Id, Int32.MinValue);
            manager.InsertFingerprint(f);
            Assert.AreNotEqual(fakeId, f.Id);
            List<Track> list = manager.ReadTrackByFingerprint(f.Id);
            Track readT = list.FirstOrDefault(temp => temp.Id == track.Id);
            Assert.AreNotEqual(null, readT);
            Assert.AreEqual(track.Id, readT.Id);
            Assert.AreEqual(track.AlbumId, readT.AlbumId);
            Assert.AreEqual(track.Artist, readT.Artist);
            Assert.AreEqual(track.Title, readT.Title);
            Assert.AreEqual(track.TrackLength, readT.TrackLength);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete tracks test
        /// </summary>
        [TestMethod]
        public void DeleteTrackListTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = new Album(Int32.MinValue, name, 1986);
            _dalManager.InsertAlbum(album);
            List<Track> listTracks = new List<Track>();

            List<Int32> lId = new List<Int32>();
            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(Int32.MinValue, name + i, name + i, album.Id);
                listTracks.Add(a);
            }
            _dalManager.InsertTrack(listTracks);
            List<Fingerprint> fingerprintList = new List<Fingerprint>();
            for (int j = 0; j < 100; j++)
            {
                fingerprintList.Add(new Fingerprint(0, GENERIC_FINGERPRINT, listTracks[j/10].Id, 0));
            }
            _dalManager.InsertFingerprint(fingerprintList);
            List<Track> listOfTracks = _dalManager.ReadTracks();
            int changed = _dalManager.DeleteTrack(listOfTracks);
            if (Bswitch.Enabled)
            {
                Trace.WriteLine("#" + name + ":" + "number of tracks in the database:" + listOfTracks.Count);
                Trace.WriteLine("#" + name + ":" + "number of changed rows: " + changed);
            }
            listOfTracks = _dalManager.ReadTracks();
            Assert.AreEqual(0, (listOfTracks == null) ? 0 : listOfTracks.Count);
            Album ab = _dalManager.ReadAlbumById(album.Id);
            Assert.AreEqual(0, (ab == null) ? 0 : 1);
            List<Fingerprint> list = _dalManager.ReadFingerprints();
            Assert.AreEqual(0, (list == null) ? 0 : list.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete track test
        /// </summary>
        [TestMethod]
        public void DeleteTrackTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = new Album(0, name, 1986);
            _dalManager.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id);
            _dalManager.InsertTrack(track);
            _dalManager.DeleteTrack(track);
            Assert.AreEqual(null, _dalManager.ReadAlbumById(album.Id));
            Assert.AreEqual(null, _dalManager.ReadTrackById(track.Id));

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete track by ID test
        /// </summary>
        [TestMethod]
        public void DeleteTrackByIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);
            const int fakeId = Int32.MinValue;
            Album album = new Album(fakeId, name, 1986);
            _dalManager.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album.Id);
            Track track = new Track(fakeId, name, name, album.Id);
            _dalManager.InsertTrack(track);
            Assert.AreNotEqual(fakeId, track.Id);
            _dalManager.DeleteTrack(track.Id);
            Assert.AreEqual(null, _dalManager.ReadAlbumById(album.Id));
            Assert.AreEqual(null, _dalManager.ReadTrackById(track.Id));

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete a list of tracks test by ID
        /// </summary>
        [TestMethod]
        public void DeleteTrackListOfIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);
            const int fakeId = Int32.MinValue;
            Album album = new Album(fakeId, name, 1986);
            _dalManager.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album.Id);
            List<Track> listTracks = new List<Track>();

            for (int i = 0; i < 10; i++)
            {
                Track a = new Track(fakeId, name + i, name + i, album.Id);
                listTracks.Add(a);
            }

            _dalManager.InsertTrack(listTracks);
            foreach (Track track in listTracks)
                Assert.AreNotEqual(fakeId, track.Id);
            List<Fingerprint> fingerprintList = new List<Fingerprint>();
            for (int j = 0; j < 100; j++)
                fingerprintList.Add(new Fingerprint(fakeId, GENERIC_FINGERPRINT, listTracks[j/10].Id, 0));

            _dalManager.InsertFingerprint(fingerprintList);
            foreach (Fingerprint finger in fingerprintList)
                Assert.AreNotEqual(fakeId, finger.Id);
            List<Track> listOfTracks = _dalManager.ReadTracks();
            List<int> lId = listOfTracks.Select(t => t.Id).ToList();
            int changed = _dalManager.DeleteTrack(lId);
            if (Bswitch.Enabled)
            {
                Trace.WriteLine("#" + name + ":" + "number of tracks in the database:" + listOfTracks.Count);
                Trace.WriteLine("#" + name + ":" + "number of changed rows: " + changed);
            }
            listOfTracks = _dalManager.ReadTracks();
            Assert.AreEqual(0, (listOfTracks == null) ? 0 : listOfTracks.Count);
            Album ab = _dalManager.ReadAlbumById(album.Id);
            Assert.AreEqual(0, (ab == null) ? 0 : 1);
            List<Fingerprint> list = _dalManager.ReadFingerprints();
            Assert.AreEqual(0, (list == null) ? 0 : list.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete null track test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void DeleteTrackNullParamTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track t = null;
            _dalManager.DeleteTrack(t);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete null list of tracks
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void DeleteTrackNullListTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            List<Track> t = null;
            _dalManager.DeleteTrack(t);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete Empty list of tracks
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void DeleteTrackEmptyListTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            List<Track> t = new List<Track>();
            _dalManager.DeleteTrack(t);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Delete a track list which is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void DeleteTrackNullListOfTracksTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            List<Int32> t = null;
            _dalManager.DeleteTrack(t);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        #endregion

        #region Insert/Read/Delete Fingerprint objects tests

        /// <summary>
        ///   Insert and Read operations on Fingerprints Table
        /// </summary>
        [TestMethod]
        public void InsertReadFingerprintTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Fingerprint f = new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0);
            manager.InsertFingerprint(f);
            List<Fingerprint> allFingerprints = manager.ReadFingerprints();
            List<Int32> lGuid = new List<Int32>();
            foreach (Fingerprint temp in allFingerprints)
                lGuid.Add(temp.Id);
            Assert.AreEqual(true, lGuid.Contains(f.Id));

            List<Fingerprint> addList = new List<Fingerprint>();
            for (int i = 0; i < 10; i++)
            {
                addList.Add(new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0));
            }
            manager.InsertFingerprint(addList);
            allFingerprints = manager.ReadFingerprints();
            lGuid.Clear();
            foreach (Fingerprint temp in allFingerprints)
                lGuid.Add(temp.Id);
            foreach (Fingerprint finger in addList)
            {
                if (Bswitch.Enabled)
                    Trace.WriteLine(name + ":" + finger.Id);
                Assert.AreEqual(true, lGuid.Contains(finger.Id));
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read fingerprints by id test
        /// </summary>
        [TestMethod]
        public void ReadFingerprintByIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            const int fakeId = Int32.MinValue;
            Album album = new Album(fakeId, name, 1986);
            manager.InsertAlbum(album);
            Assert.AreNotEqual(fakeId, album);
            Track track = new Track(fakeId, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Assert.AreNotEqual(fakeId, track.Id);
            Fingerprint f = new Fingerprint(fakeId, GENERIC_FINGERPRINT, track.Id, 0);
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

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read Fingerprints from the database by multiple Ids
        /// </summary>
        [TestMethod]
        public void ReadFingerprintsByMultipleIdsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            List<Fingerprint> listOfFingers = new List<Fingerprint>();
            Album album = _dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            _dalManager.InsertTrack(track);
            const int count = 100;
            List<Int32> listOfGuids = new List<Int32>();
            const int fakeId = Int32.MinValue;
            for (int i = 0; i < count; i++)
            {
                listOfFingers.Add(new Fingerprint(fakeId, GENERIC_FINGERPRINT, track.Id, 0));
            }
            _dalManager.InsertFingerprint(listOfFingers);
            listOfGuids.AddRange(listOfFingers.Select((f) => f.Id));
            List<Fingerprint> readFingers = _dalManager.ReadFingerprintById(listOfGuids);
            Assert.AreEqual(readFingers.Count, listOfFingers.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read fingerprints by track id
        /// </summary>
        [TestMethod]
        public void ReadFingerprintByTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            Track track = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track);
            Fingerprint f = new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0);
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

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read Fingerprints by multiple track id
        /// </summary>
        [TestMethod]
        public void ReadFingerprintByMultipleTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            Track track0 = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track0);
            Track track1 = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track1);
            Track track2 = new Track(0, name, name, album.Id, 360);
            manager.InsertTrack(track2);

            Fingerprint f0 = new Fingerprint(0, GENERIC_FINGERPRINT, track0.Id, 0);
            manager.InsertFingerprint(f0);
            Fingerprint f1 = new Fingerprint(0, GENERIC_FINGERPRINT, track0.Id, 1);
            manager.InsertFingerprint(f1);
            Fingerprint f2 = new Fingerprint(0, GENERIC_FINGERPRINT, track1.Id, 2);
            manager.InsertFingerprint(f2);
            Fingerprint f3 = new Fingerprint(0, GENERIC_FINGERPRINT, track1.Id, 3);
            manager.InsertFingerprint(f3);
            Fingerprint f4 = new Fingerprint(0, GENERIC_FINGERPRINT, track2.Id, 4);
            manager.InsertFingerprint(f4);
            Fingerprint f5 = new Fingerprint(0, GENERIC_FINGERPRINT, track2.Id, 5);
            manager.InsertFingerprint(f5);
            Fingerprint f6 = new Fingerprint(0, GENERIC_FINGERPRINT, track0.Id, 6);
            manager.InsertFingerprint(f6);
            Fingerprint f7 = new Fingerprint(0, GENERIC_FINGERPRINT, track1.Id, 7);
            manager.InsertFingerprint(f7);
            Fingerprint f8 = new Fingerprint(0, GENERIC_FINGERPRINT, track2.Id, 8);
            manager.InsertFingerprint(f8);

            Dictionary<Int32, List<Fingerprint>> dict = manager.ReadFingerprintsByMultipleTrackId(
                new List<Track> {track0, track1, track2}, 0);

            Assert.AreNotEqual(null, dict);
            Assert.AreEqual(3, dict.Keys.Count);
            foreach (KeyValuePair<Int32, List<Fingerprint>> item in dict)
            {
                Assert.AreEqual(3, item.Value.Count);
            }
            Assert.AreEqual(true, dict.ContainsKey(track0.Id));
            Assert.AreEqual(true, dict.ContainsKey(track1.Id));
            Assert.AreEqual(true, dict.ContainsKey(track2.Id));

            foreach (KeyValuePair<Int32, List<Fingerprint>> pair in dict)
            {
                Assert.AreEqual(3, pair.Value.Count);
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Read fingerprints by track id concurrent
        /// </summary>
        [TestMethod]
        public void ReadFingerprintByMultipleTrackIdTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway manager = _dalManager;
            Album album = new Album(0, name, 1986);
            manager.InsertAlbum(album);
            const int numberOfTracks = 1153;
            const int numberOfFingerprintsPerTrack = 10;

            List<Track> listTrack = new List<Track>();
            List<Fingerprint> listOfFingerprints = new List<Fingerprint>();
            for (int i = 0; i < numberOfTracks; i++)
            {
                Track track0 = new Track(0, name, name, album.Id, 360);
                listTrack.Add(track0);
                manager.InsertTrack(track0);
                for (int j = 0; j < numberOfFingerprintsPerTrack; j++)
                {
                    Fingerprint f0 = new Fingerprint(0, GENERIC_FINGERPRINT, track0.Id, 0);
                    listOfFingerprints.Add(f0);
                }
            }
            manager.InsertFingerprint(listOfFingerprints);

            Dictionary<Int32, List<Fingerprint>> dict = manager.ReadFingerprintsByMultipleTrackId(listTrack, 0);

            Assert.AreNotEqual(null, dict);
            Assert.AreEqual(numberOfTracks, dict.Keys.Count);
            foreach (Track track in listTrack)
            {
                Assert.AreEqual(true, dict.ContainsKey(track.Id));
                Assert.AreEqual(numberOfFingerprintsPerTrack, dict[track.Id].Count);
            }

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   No such track with such Id in the database
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (SqlException))]
        public void InsertFingerprintWithBadTrackIdForeignKeyreference()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album a = new Album(0, name, 1990);
            Track track = new Track(0, name, name, a.Id);
            Fingerprint f = new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0);
            _dalManager.InsertFingerprint(f);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert null fingerprint in the database
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void InsertFingerprintNullTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Fingerprint a = null;
            _dalManager.InsertFingerprint(a);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert an empty collection in the database Collection.Count = 0
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void InsertEmptyCollectionInFingerprints()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            _dalManager.InsertFingerprint(new List<Fingerprint>());

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        #endregion

        #region Insert/Read/Delete HashBin objects tests

        /// <summary>
        ///   Insert HashBin corresponding to MinHash + LSH KNN approach
        /// </summary>
        [TestMethod]
        public void InsertReadHashBinMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            _dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0);
            _dalManager.InsertFingerprint(finger);
            const long hashbin = 100000;
            const int hashtable = 20;
            HashBinMinHash hashbinminhash = new HashBinMinHash(0, hashbin, hashtable, track.Id, finger.Id);
            _dalManager.InsertHashBin(hashbinminhash);
            Dictionary<Int32, List<HashBinMinHash>> result = _dalManager.ReadFingerprintsByHashBucketAndHashTableLSH(new[] {hashbin}, new[] {hashtable});
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(true, result.ContainsKey(finger.Id));
            Assert.AreEqual(1, result[finger.Id].Count);
            Assert.AreEqual(hashbinminhash.Id, result[finger.Id][0].Id);
            _dalManager.DeleteTrack(track.Id);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert HashBin corresponding to MinHash + LSH KNN approach
        /// </summary>
        [TestMethod]
        public void InsertReadHashBinNeuralHasherTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            _dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0);
            _dalManager.InsertFingerprint(finger);
            const long hashbin = 100000;
            const int hashtable = 20;
            HashBinNeuralHasher hashbinminhash = new HashBinNeuralHasher(0, hashbin, hashtable, track.Id);
            _dalManager.InsertHashBin(hashbinminhash);
            List<Int32> result = _dalManager.ReadTrackIdByHashBinAndHashTableNeuralHasher(hashbin, hashtable);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(track.Id, result[0]);
            _dalManager.DeleteTrack(track.Id);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert multiple MinHashes to collection
        /// </summary>
        [TestMethod]
        public void InsertReadHashBinMinHashesTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            _dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0);
            _dalManager.InsertFingerprint(finger);
            const long hashbin = 100000;
            const int hashtable = 20;
            List<HashBinMinHash> list = new List<HashBinMinHash>();
            const int count = 20;
            for (int i = 0; i < count; i++)
            {
                HashBinMinHash hashbinminhash = new HashBinMinHash(0, hashbin, hashtable, track.Id, finger.Id);
                list.Add(hashbinminhash);
            }

            _dalManager.InsertHashBin(list);
            Dictionary<Int32, List<HashBinMinHash>> result = _dalManager.ReadFingerprintsByHashBucketAndHashTableLSH(new[] {hashbin}, new[] {hashtable});
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(true, result.ContainsKey(finger.Id));
            Assert.AreEqual(count, result[finger.Id].Count);
            _dalManager.DeleteTrack(track.Id);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert multiple HashBinNeuralHasher to collection
        /// </summary>
        [TestMethod]
        public void InsertReadHashBinNeuralHashesTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album album = _dalManager.ReadUnknownAlbum();
            Track track = new Track(0, name, name, album.Id);
            _dalManager.InsertTrack(track);
            Fingerprint finger = new Fingerprint(0, GENERIC_FINGERPRINT, track.Id, 0);
            _dalManager.InsertFingerprint(finger);
            const long hashbin = 100000;
            const int hashtable = 20;
            List<HashBinNeuralHasher> list = new List<HashBinNeuralHasher>();
            const int count = 20;
            for (int i = 0; i < count; i++)
            {
                HashBinNeuralHasher hashbinminhash = new HashBinNeuralHasher(0, hashbin, hashtable, track.Id);
                list.Add(hashbinminhash);
            }
            _dalManager.InsertHashBin(list);
            List<Int32> result = _dalManager.ReadTrackIdByHashBinAndHashTableNeuralHasher(hashbin, hashtable);
            Assert.IsNotNull(result);
            Assert.AreEqual(count, result.Count);
            _dalManager.DeleteTrack(track.Id);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert null hash bin into the database
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void InsertNullHashBinTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            _dalManager.InsertHashBin(null);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Try to insert an empty collection of hash bins into DB
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void InsertEmptyCollectionInHashBinMinHash()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            List<HashBinMinHash> list = new List<HashBinMinHash>();
            _dalManager.InsertHashBin(list);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Insert null track into the database
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void InsertTrackNullTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track t = null;
            _dalManager.InsertTrack(t);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Track Length cannot be less than 0
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (FingerprintEntityException))]
        public void CheckTrackLengthConstraints()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Track track = new Track();
            Assert.AreEqual(0, track.AlbumId);
            Assert.AreEqual(true, String.IsNullOrEmpty(track.Artist));
            Assert.AreEqual(0, track.TrackLength);
            Assert.AreEqual(true, String.IsNullOrEmpty(track.Title));
            track.TrackLength = Int32.MinValue;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   No such Album with such Id in the database
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof (SqlException))]
        public void InsertTrackWithBadAlbumIdForeignKeyReference()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Album a = new Album(Int32.MinValue, name, 1990);
            Track track = new Track(Int32.MinValue, name, name, a.Id);
            _dalManager.InsertTrack(track);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        #endregion

        #region False positive analisys

        ///<summary>
        ///  A test for DaoGateway Constructor
        ///</summary>
        [TestMethod]
        public void DaoGatewayConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Assert.IsNotNull(target);
        }

        ///<summary>
        ///  A test for DeleteTrack. Delete a list of not-existent track, and check if 0 elements have been changed in the DB
        ///</summary>
        [TestMethod]
        public void DeleteTrackFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            IEnumerable<Int32> collection = new List<Int32> {0};
            const int expected = 0;
            int actual = target.DeleteTrack(collection);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for DeleteTrack. Delete a non existent track.
        ///</summary>
        [TestMethod]
        public void DeleteTrackFalseTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Track track = new Track();
            const int expected = 0;
            int actual = target.DeleteTrack(track);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for DeleteTrack. Delete a non existent track
        ///</summary>
        [TestMethod]
        public void DeleteTrackFalseTest2()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            IEnumerable<Track> collection = new List<Track> {new Track()};
            const int expected = 0;
            int actual = target.DeleteTrack(collection);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for DeleteTrack
        ///</summary>
        [TestMethod]
        public void DeleteTrackFalseTest3()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Int32 trackId = 0;
            const int expected = 0;
            int actual = target.DeleteTrack(trackId);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }


        ///<summary>
        ///  A test for ReadAlbumById
        ///</summary>
        [TestMethod]
        public void ReadAlbumByIdFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Int32 id = 0;
            Album expected = null;
            Album actual = target.ReadAlbumById(id);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadAlbumByName
        ///</summary>
        [TestMethod]
        public void ReadAlbumByNameFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Album expected = null;
            Album actual = target.ReadAlbumByName(name);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadFingerprintById
        ///</summary>
        [TestMethod]
        public void ReadFingerprintByIdFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Int32 id = 0;
            Fingerprint expected = null;
            Fingerprint actual = target.ReadFingerprintById(id);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadFingerprintById
        ///</summary>
        [TestMethod]
        public void ReadFingerprintByIdFalseTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            IEnumerable<Int32> ids = new List<Int32> {0};
            List<Fingerprint> expected = null;
            List<Fingerprint> actual = target.ReadFingerprintById(ids);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadFingerprintByIdConcurrent
        ///</summary>
        [TestMethod]
        public void ReadFingerprintByIdConcurrentFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            IEnumerable<Int32> ids = new List<Int32> {0};
            List<Fingerprint> expected = null;
            List<Fingerprint> actual = target.ReadFingerprintByIdConcurrent(ids);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadFingerprintsByHashBucketAndHashTable
        ///</summary>
        [TestMethod]
        public void ReadFingerprintIdByHashBinAndHashTableMinHashFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            long[] hashBins = new long[] {1, 2, 3, 4, 5, 6};
            int[] hashTables = new[] {100, 101, 102, 103, 104, 105};
            Dictionary<Int32, List<HashBinMinHash>> expected = null;
            Dictionary<Int32, List<HashBinMinHash>> actual = target.ReadFingerprintsByHashBucketAndHashTableLSH(hashBins, hashTables);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadFingerprints
        ///</summary>
        [TestMethod]
        public void ReadFingerprintsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);
            DaoGateway target = new DaoGateway(_connectionstring);
            Album album = target.ReadUnknownAlbum();
            Track t = new Track(0, name, name, album.Id);
            target.InsertTrack(t);
            Fingerprint f = new Fingerprint(0, GENERIC_FINGERPRINT, t.Id, 10);
            target.InsertFingerprint(f);
            List<Fingerprint> actual = target.ReadFingerprints();
            Assert.IsTrue(actual.Count >= 1);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadFingerprintsByMultipleTrackId
        ///</summary>
        [TestMethod]
        public void ReadFingerprintsByMultipleTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            List<Track> tracks = new List<Track> {new Track(), new Track(), new Track(), new Track()};
            const int numberOfFingerprintsToRead = 10;
            Dictionary<Int32, List<Fingerprint>> expected = null;
            Dictionary<Int32, List<Fingerprint>> actual = target.ReadFingerprintsByMultipleTrackId(tracks, numberOfFingerprintsToRead);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadFingerprintsByTrackId
        ///</summary>
        [TestMethod]
        public void ReadFingerprintsByTrackIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            const int trackId = 0;
            const int numberOfFingerprintsToRead = 10;
            List<Fingerprint> expected = null;
            List<Fingerprint> actual = target.ReadFingerprintsByTrackId(trackId, numberOfFingerprintsToRead);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadPermutations
        ///</summary>
        [TestMethod]
        public void ReadPermutationsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Dictionary<int, int[]> expected = null;
            IPermutations perms = new DbPermutations(_connectionstring);
            int[][] actual = perms.GetPermutations();
            Assert.AreNotEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }


        ///<summary>
        ///  A test for ReadTrackByArtistAndTitleName
        ///</summary>
        [TestMethod]
        public void ReadTrackByArtistAndTitleNameTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            string artist = name;
            string title = name;
            Track expected = null;
            Track actual = target.ReadTrackByArtistAndTitleName(artist, title);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadTrackByFingerprint
        ///</summary>
        [TestMethod]
        public void ReadTrackByFingerprintFalseTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Int32 id = 0;
            List<Track> expected = null;
            List<Track> actual = target.ReadTrackByFingerprint(id);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadTrackById
        ///</summary>
        [TestMethod]
        public void ReadTrackByIdTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            Int32 id = 0;
            Track expected = null;
            Track actual = target.ReadTrackById(id);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadTrackIdByHashBinAndHashTableNeuralHasher
        ///</summary>
        [TestMethod]
        public void ReadTrackIdByHashBinAndHashTableNeuralHasherTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            const long hashBin = 0;
            const int hashTable = 100;
            List<Int32> expected = null;
            List<Int32> actual = target.ReadTrackIdByHashBinAndHashTableNeuralHasher(hashBin, hashTable);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasher
        ///</summary>
        [TestMethod]
        public void ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasherTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoGateway target = new DaoGateway(_connectionstring);
            long[] hashBins = new long[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            int[] hashTables = new[] {100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110};
            Dictionary<Int32, int> expected = null;
            Dictionary<Int32, int> actual = target.ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasher(hashBins, hashTables);
            Assert.AreEqual(expected, actual);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        #endregion
    }
}