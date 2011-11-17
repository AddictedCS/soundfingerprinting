// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.DbStorage.Entities;

namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    ///<summary>
    ///  This is a test class for MSDaoStoredProcedureBuilderTest and is intended
    ///  to contain all MSDaoStoredProcedureBuilderTest Unit Tests
    ///</summary>
    [TestClass]
// ReSharper disable InconsistentNaming
    public class MSDaoStoredProcedureBuilderTest : BaseTest
// ReSharper restore InconsistentNaming
    {
        #region Stored Procedures Name

        private const string SP_INSERT_ALBUM = "sp_InsertAlbum";
        private const string SP_INSERT_TRACK = "sp_InsertTrack";
        private const string SP_INSERT_FINGERPRINT = "sp_InsertFingerprint";
        private const string SP_INSERT_NEURALHASHER_HASHBIN = "sp_InsertHashBin";
        private const string SP_INSERT_MINHASH_HASHBIN = "sp_InsertHashBinMinHash";
        private const string SP_READ_UNKNOWN_ALBUMS = "sp_ReadAlbumUnknown";
        private const string SP_READ_ALBUMS = "sp_ReadAlbums";
        private const string SP_READ_TRACKS = "sp_ReadTracks";
        private const string SP_READ_FINGERPRINTS = "sp_ReadFingerprints";
        private const string SP_READ_ALBUM_BY_ID = "sp_ReadAlbumById";
        private const string SP_READ_TRACK_BY_ID = "sp_ReadTrackById";
        private const string SP_READ_FINGERPRINT_BY_ID = "sp_ReadFingerprintById";
        private const string SP_READ_FINGERPRINT_BY_TRACK_ID = "sp_ReadFingerprintByTrackId";
        private const string SP_READ_TRACK_BY_FINGERPRINT = "sp_ReadTrackByFingerprint";
        private const string SP_READ_TRACK_BY_ARTIST_SONG_NAME = "sp_ReadTrackByArtistAndSongName";
        private const string SP_READ_TRACKID_BY_HASHBIN_HASHTABLE_NEURALHASHER = "sp_ReadHashBinsByHashBinAndHashTable";
        private const string SP_READ_HASHBINS_BY_HASHBIN_HASHTABLE_MINHASH = "sp_ReadHashBinsByHashBinAndHashTableMinHash";
        private const string SP_READ_FINGERPRINT_CANDIDATES_BY_HASHBIN_HASHTABLE_MINHASH = "sp_ReadFingerprintCandidatesByHashBinHashTableMinHash";
        private const string SP_READ_ALL_PERMUTATIONS = "sp_ReadPermutations";
        private const string SP_READ_PERMUTATION_BY_ID = "sp_ReadPermutationById";
        private const string SP_READ_DUPLICATED_TRACKS = "sp_ReadDuplicatedTracks";
        private const string SP_READ_MIN_MAX_HASHBIN_MINHASH = "sp_ReadMaxMinHashBin";
        private const string SP_READ_HASHBINMINHASH_RANGE = "sp_HashBinMinHashRange";
        private const string SP_READ_HASHBIN_HASHBUCKET_HASHTABLE = "sp_HashBinReadByHashBucketHashTable";
        private const string SP_UPDATE_HASHBUCKET_MINHASH = "sp_HashBinUpdateHashBucket";
        private const string SP_UPDATE_HASHBUCKET_SINGLE_MINHASH = "sp_HashBinUpdateSingleHashBucket";
        private const string SP_UPDATE_HASHBUCKET_COUNT = "sp_HashBinUpdateHashBucketCount";
        private const string SP_READ_HASH_BUCKETCOUNT_BY_HASHBUCKET = "sp_ReadHashBucketCountPerHashBucket";
        private const string SP_UPDATE_ALL_HASHBUCKET_COUNT_MINHASH = "sp_UpdateAllHashBucketCountMinHash";
        private const string SP_READ_ALL_FINGERPRINTID_BYHASHTABLES_HASHBUCKETS = "sp_HashBinReadAllByHashBucketHashTable";
        private const string SP_DELETE_TRACK = "sp_DeleteTrack";

        #endregion

        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        ///<summary>
        ///  A test for DaoStoredProcedureBuilder Constructor
        ///</summary>
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void MSDaoStoredProcedureBuilderConstructorTest()
// ReSharper restore InconsistentNaming
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Assert.IsNotNull(target);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetDeleteTrackCommand
        ///</summary>
        [TestMethod]
        public void GetDeleteTrackCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Int32 trackId = 0;

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetDeleteTrackCommand(trackId, connection);

            Assert.AreEqual(SP_DELETE_TRACK, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetInsertAlbumCommand
        ///</summary>
        [TestMethod]
        public void GetInsertAlbumCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Album album = new Album(0, name, 1998);
            DbConnection connection = Dbf.CreateConnection();
            connection.ConnectionString = ConnectionString;
            IDbCommand actual = target.GetInsertAlbumCommand(album, connection);
            Assert.AreEqual(SP_INSERT_ALBUM, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);


            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetInsertFingerprintCommand
        ///</summary>
        [TestMethod]
        public void GetInsertFingerprintCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder(); // TODO: Initialize to an appropriate value
            Fingerprint fingerprint = new Fingerprint(0, GENERIC_FINGERPRINT, 0, 64, 100);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertFingerprintCommand(fingerprint, connection);

            Assert.AreEqual(SP_INSERT_FINGERPRINT, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetInsertHashBinMinHashCommand
        ///</summary>
        [TestMethod]
        public void GetInsertHashBinMinHashCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            HashBinMinHash hashBinMinHash = new HashBinMinHash(0, 125, 10, 0, 0);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertHashBinMinHashCommand(hashBinMinHash, connection);

            Assert.AreEqual(SP_INSERT_MINHASH_HASHBIN, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetInsertHashBinNeuralHasherCommand
        ///</summary>
        [TestMethod]
        public void GetInsertHashBinNeuralHasherCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            HashBinNeuralHasher hashBinNeuralHasher = new HashBinNeuralHasher(0, 45, 12, 0);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertHashBinNeuralHasherCommand(hashBinNeuralHasher, connection);

            Assert.AreEqual(SP_INSERT_NEURALHASHER_HASHBIN, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(3, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetInsertTrackCommand
        ///</summary>
        [TestMethod]
        public void GetInsertTrackCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Track track = new Track(0, name, name, 0, 230);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertTrackCommand(track, connection);

            Assert.AreEqual(SP_INSERT_TRACK, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadAlbumsByIdCommand
        ///</summary>
        [TestMethod]
        public void GetReadAlbumsByIdCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Int32 id = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadAlbumsByIdCommand(id, connection);

            Assert.AreEqual(SP_READ_ALBUM_BY_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadAlbumsCommand
        ///</summary>
        [TestMethod]
        public void GetReadAlbumsCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadAlbumsCommand(connection);

            Assert.AreEqual(SP_READ_ALBUMS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadAllFingerprintIdByHashTablesHashBucketsMinHash
        ///</summary>
        [TestMethod]
        public void GetReadAllFingerprintIdByHashTablesHashBucketsMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            string hashtables = name;
            string hashbuckets = name;
            string delimiter = name;
            const int threshold = 2;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadAllFingerprintIdByHashTablesHashBucketsMinHash(hashtables, hashbuckets, delimiter, threshold, connection);

            Assert.AreEqual(SP_READ_ALL_FINGERPRINTID_BYHASHTABLES_HASHBUCKETS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(5, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadDuplicatedTracks
        ///</summary>
        [TestMethod]
        public void GetReadDuplicatedTracksTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadDuplicatedTracks(connection);

            Assert.AreEqual(SP_READ_DUPLICATED_TRACKS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadFingerprintByIdCommand
        ///</summary>
        [TestMethod]
        public void GetReadFingerprintByIdCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Int32 id = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintByIdCommand(id, connection);

            Assert.AreEqual(SP_READ_FINGERPRINT_BY_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadFingerprintByTrackIdCommand
        ///</summary>
        [TestMethod]
        public void GetReadFingerprintByTrackIdCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Int32 id = 0;
            const int numberOfFingerprintsToRead = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintByTrackIdCommand(id, numberOfFingerprintsToRead, connection);

            Assert.AreEqual(SP_READ_FINGERPRINT_BY_TRACK_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadFingerprintCadidatesByHashbinHashtable
        ///</summary>
        [TestMethod]
        public void GetReadFingerprintCadidatesByHashbinHashtableTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long hashBin = 10;
            const int hashTable = 250;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintCadidatesByHashbinHashtable(hashBin, hashTable, connection);

            Assert.AreEqual(SP_READ_FINGERPRINT_CANDIDATES_BY_HASHBIN_HASHTABLE_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadFingerprintsCommand
        ///</summary>
        [TestMethod]
        public void GetReadFingerprintsCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintsCommand(connection);

            Assert.AreEqual(SP_READ_FINGERPRINTS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadHashBinByHashBucketHashTable
        ///</summary>
        [TestMethod]
        public void GetReadHashBinByHashBucketHashTableTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int hashBucket = 0;
            const int hashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBinByHashBucketHashTable(hashBucket, hashTable, connection);

            Assert.AreEqual(SP_READ_HASHBIN_HASHBUCKET_HASHTABLE, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadHashBinMinhashRange
        ///</summary>
        [TestMethod]
        public void GetReadHashBinMinhashRangeTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long startInclusive = 0;
            const long endInclusive = 0;
            const int hashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBinMinhashRange(startInclusive, endInclusive, hashTable, connection);

            Assert.AreEqual(SP_READ_HASHBINMINHASH_RANGE, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(3, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadHashBinsByHashBucketAndHashTableLSH
        ///</summary>
        [TestMethod]
        public void GetReadHashBinsByHashBinAndHashTableMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long hashBin = 0;
            const int hashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBinsByHashBucketAndHashTableLSH(hashBin, hashTable, connection);

            Assert.AreEqual(SP_READ_HASHBINS_BY_HASHBIN_HASHTABLE_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadHashBucketCountByHashBucket
        ///</summary>
        [TestMethod]
        public void GetReadHashBucketCountByHashBucketTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBucketCountByHashBucket(connection);

            Assert.AreEqual(SP_READ_HASH_BUCKETCOUNT_BY_HASHBUCKET, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadMinMaxHashBinMinHash
        ///</summary>
        [TestMethod]
        public void GetReadMinMaxHashBinMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long ignore = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadMinMaxHashBinMinHash(ignore, connection);

            Assert.AreEqual(SP_READ_MIN_MAX_HASHBIN_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadTrackByArtistAndTitleNameCommand
        ///</summary>
        [TestMethod]
        public void GetReadTrackByArtistAndTitleNameCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            string artist = name;
            string title = name;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTrackByArtistAndTitleNameCommand(artist, title, connection);

            Assert.AreEqual(SP_READ_TRACK_BY_ARTIST_SONG_NAME, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadTrackByFingerprintCommand
        ///</summary>
        [TestMethod]
        public void GetReadTrackByFingerprintCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Int32 fingerprintId = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTrackByFingerprintCommand(fingerprintId, connection);
            Assert.AreEqual(SP_READ_TRACK_BY_FINGERPRINT, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadTrackIdByHashBinAndHashTableNeuralHasher
        ///</summary>
        [TestMethod]
        public void GetReadTrackIdByHashBinAndHashTableNeuralHasherTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long hashBin = 0;
            const int hashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTrackIdByHashBinAndHashTableNeuralHasher(hashBin, hashTable, connection);
            Assert.AreEqual(SP_READ_TRACKID_BY_HASHBIN_HASHTABLE_NEURALHASHER, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadTracksByIdCommand
        ///</summary>
        [TestMethod]
        public void GetReadTracksByIdCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Int32 id = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTracksByIdCommand(id, connection);
            Assert.AreEqual(SP_READ_TRACK_BY_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadTracksCommand
        ///</summary>
        [TestMethod]
        public void GetReadTracksCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTracksCommand(connection);

            Assert.AreEqual(SP_READ_TRACKS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetReadUnknownAlbum
        ///</summary>
        [TestMethod]
        public void GetReadUnknownAlbumTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            string unknownName = name;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadUnknownAlbum(unknownName, connection);
            Assert.AreEqual(SP_READ_UNKNOWN_ALBUMS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetUpdateAllHashBucketCountMinHash
        ///</summary>
        [TestMethod]
        public void GetUpdateAllHashBucketCountMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateAllHashBucketCountMinHash(connection);

            Assert.AreEqual(SP_UPDATE_ALL_HASHBUCKET_COUNT_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetUpdateHashBucket
        ///</summary>
        [TestMethod]
        public void GetUpdateHashBucketTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int hashBucket = 10;
            const long startInclusive = 20;
            const long endExclusive = 30;
            const int hashTable = 10;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateHashBucket(hashBucket, startInclusive, endExclusive, hashTable, connection);

            Assert.AreEqual(SP_UPDATE_HASHBUCKET_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetUpdateHashBucketCount
        ///</summary>
        [TestMethod]
        public void GetUpdateHashBucketCountTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int hashBucket = 10;
            const int hashBucketCount = 20;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateHashBucketCount(hashBucket, hashBucketCount, connection);

            Assert.AreEqual(SP_UPDATE_HASHBUCKET_COUNT, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for GetUpdateHashBucketSingle
        ///</summary>
        [TestMethod]
        public void GetUpdateHashBucketSingleTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);


            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int hashBucket = 20;
            const long hashBin = 10;
            const int hashTable = 30;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateHashBucketSingle(hashBucket, hashBin, hashTable, connection);

            Assert.AreEqual(SP_UPDATE_HASHBUCKET_SINGLE_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(3, actual.Parameters.Count);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}