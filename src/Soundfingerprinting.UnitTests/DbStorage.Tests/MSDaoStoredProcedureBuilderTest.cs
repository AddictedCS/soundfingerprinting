// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com

namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;

    [TestClass]
    public class MsDaoStoredProcedureBuilderTest : BaseTest
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

        [TestMethod]
        public void GetDeleteTrackCommandTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int TrackId = 0;

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetDeleteTrackCommand(TrackId, connection);

            Assert.AreEqual(SP_DELETE_TRACK, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetInsertAlbumCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
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
        }

        [TestMethod]
        public void GetInsertFingerprintCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
                // TODO: Initialize to an appropriate value
            Fingerprint fingerprint = new Fingerprint(0, GenericFingerprint, 0, 64, 100);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertFingerprintCommand(fingerprint, connection);

            Assert.AreEqual(SP_INSERT_FINGERPRINT, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetInsertHashBinMinHashCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
        
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            HashBinMinHash hashBinMinHash = new HashBinMinHash(0, 125, 10, 0, 0);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertHashBinMinHashCommand(hashBinMinHash, connection);

            Assert.AreEqual(SP_INSERT_MINHASH_HASHBIN, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetInsertHashBinNeuralHasherCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            HashBinNeuralHasher hashBinNeuralHasher = new HashBinNeuralHasher(0, 45, 12, 0);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertHashBinNeuralHasherCommand(hashBinNeuralHasher, connection);

            Assert.AreEqual(SP_INSERT_NEURALHASHER_HASHBIN, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(3, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetInsertTrackCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            Track track = new Track(0, name, name, 0, 230);

            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetInsertTrackCommand(track, connection);

            Assert.AreEqual(SP_INSERT_TRACK, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadAlbumsByIdCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            var id = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadAlbumsByIdCommand(id, connection);

            Assert.AreEqual(SP_READ_ALBUM_BY_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadAlbumsCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadAlbumsCommand(connection);

            Assert.AreEqual(SP_READ_ALBUMS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadAllFingerprintIdByHashTablesHashBucketsMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            string hashtables = name;
            string hashbuckets = name;
            string delimiter = name;
            const int Threshold = 2;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadAllFingerprintIdByHashTablesHashBucketsMinHash(
                hashtables, hashbuckets, delimiter, Threshold, connection);

            Assert.AreEqual(SP_READ_ALL_FINGERPRINTID_BYHASHTABLES_HASHBUCKETS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(5, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadDuplicatedTracksTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadDuplicatedTracks(connection);

            Assert.AreEqual(SP_READ_DUPLICATED_TRACKS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadFingerprintByIdCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            var id = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintByIdCommand(id, connection);

            Assert.AreEqual(SP_READ_FINGERPRINT_BY_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadFingerprintByTrackIdCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int Id = 0;
            const int NumberOfFingerprintsToRead = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintByTrackIdCommand(Id, NumberOfFingerprintsToRead, connection);

            Assert.AreEqual(SP_READ_FINGERPRINT_BY_TRACK_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadFingerprintCadidatesByHashbinHashtableTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long HashBin = 10;
            const int HashTable = 250;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintCadidatesByHashbinHashtable(HashBin, HashTable, connection);

            Assert.AreEqual(SP_READ_FINGERPRINT_CANDIDATES_BY_HASHBIN_HASHTABLE_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadFingerprintsCommandTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadFingerprintsCommand(connection);

            Assert.AreEqual(SP_READ_FINGERPRINTS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadHashBinByHashBucketHashTableTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int HashBucket = 0;
            const int HashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBinByHashBucketHashTable(HashBucket, HashTable, connection);

            Assert.AreEqual(SP_READ_HASHBIN_HASHBUCKET_HASHTABLE, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadHashBinMinhashRangeTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long StartInclusive = 0;
            const long EndInclusive = 0;
            const int HashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBinMinhashRange(StartInclusive, EndInclusive, HashTable, connection);

            Assert.AreEqual(SP_READ_HASHBINMINHASH_RANGE, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(3, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadHashBinsByHashBinAndHashTableMinHashTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long HashBin = 0;
            const int HashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBinsByHashBucketAndHashTableLSH(HashBin, HashTable, connection);

            Assert.AreEqual(SP_READ_HASHBINS_BY_HASHBIN_HASHTABLE_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadHashBucketCountByHashBucketTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadHashBucketCountByHashBucket(connection);

            Assert.AreEqual(SP_READ_HASH_BUCKETCOUNT_BY_HASHBUCKET, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadMinMaxHashBinMinHashTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long Ignore = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadMinMaxHashBinMinHash(Ignore, connection);

            Assert.AreEqual(SP_READ_MIN_MAX_HASHBIN_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadTrackByArtistAndTitleNameCommandTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;

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
        }

        [TestMethod]
        public void GetReadTrackByFingerprintCommandTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int FingerprintId = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTrackByFingerprintCommand(FingerprintId, connection);
            Assert.AreEqual(SP_READ_TRACK_BY_FINGERPRINT, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadTrackIdByHashBinAndHashTableNeuralHasherTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const long HashBin = 0;
            const int HashTable = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTrackIdByHashBinAndHashTableNeuralHasher(HashBin, HashTable, connection);
            Assert.AreEqual(SP_READ_TRACKID_BY_HASHBIN_HASHTABLE_NEURALHASHER, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadTracksByIdCommandTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int Id = 0;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTracksByIdCommand(Id, connection);
            Assert.AreEqual(SP_READ_TRACK_BY_ID, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadTracksCommandTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadTracksCommand(connection);

            Assert.AreEqual(SP_READ_TRACKS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetReadUnknownAlbumTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            string unknownName = name;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetReadUnknownAlbum(unknownName, connection);
            Assert.AreEqual(SP_READ_UNKNOWN_ALBUMS, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(1, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetUpdateAllHashBucketCountMinHashTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateAllHashBucketCountMinHash(connection);

            Assert.AreEqual(SP_UPDATE_ALL_HASHBUCKET_COUNT_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(0, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetUpdateHashBucketTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int HashBucket = 10;
            const long StartInclusive = 20;
            const long EndExclusive = 30;
            const int HashTable = 10;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateHashBucket(
                HashBucket, StartInclusive, EndExclusive, HashTable, connection);

            Assert.AreEqual(SP_UPDATE_HASHBUCKET_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(4, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetUpdateHashBucketCountTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int HashBucket = 10;
            const int HashBucketCount = 20;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateHashBucketCount(HashBucket, HashBucketCount, connection);

            Assert.AreEqual(SP_UPDATE_HASHBUCKET_COUNT, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(2, actual.Parameters.Count);
        }

        [TestMethod]
        public void GetUpdateHashBucketSingleTest()
        {
            DaoStoredProcedureBuilder target = new DaoStoredProcedureBuilder();
            const int hashBucket = 20;
            const long HashBin = 10;
            const int HashTable = 30;
            DbConnection connection = Dbf.CreateConnection();
            IDbCommand actual = target.GetUpdateHashBucketSingle(hashBucket, HashBin, HashTable, connection);

            Assert.AreEqual(SP_UPDATE_HASHBUCKET_SINGLE_MINHASH, actual.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, actual.CommandType);
            Assert.IsNotNull(actual.Connection);
            Assert.AreEqual(ConnectionState.Closed, actual.Connection.State);
            Assert.AreEqual(3, actual.Parameters.Count);
        }
    }
}