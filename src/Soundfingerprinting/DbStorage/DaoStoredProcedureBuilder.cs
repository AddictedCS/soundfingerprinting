// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Soundfingerprinting.DbStorage.Entities;
using Soundfingerprinting.DbStorage.Utils;

namespace Soundfingerprinting.DbStorage
{
// ReSharper disable InconsistentNaming
    public class DaoStoredProcedureBuilder
// ReSharper restore InconsistentNaming
    {
        private const int COMMAND_TIMEOUT = 60*10; /*600 sec*/

        /// <summary>
        ///   Database provider factory
        /// </summary>
        private readonly DbProviderFactory _dbFactory;

        /// <summary>
        ///   Default database provider factory is SQL Client
        /// </summary>
        public DaoStoredProcedureBuilder()
        {
            _dbFactory = SqlClientFactory.Instance;
        }

        /// <summary>
        ///   Database provider factory
        /// </summary>
        /// <param name = "factory"></param>
        public DaoStoredProcedureBuilder(DbProviderFactory factory)
        {
            _dbFactory = factory;
        }

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

        private const string SP_READ_PERMUTATION_BY_ID = "sp_ReadPermutationById";
        private const string SP_READ_DUPLICATED_TRACKS = "sp_ReadDuplicatedTracks";
        private const string SP_READ_MIN_MAX_HASHBIN_MINHASH = "sp_ReadMaxMinHashBin";
        private const string SP_READ_HASHBINMINHASH_RANGE = "sp_HashBinMinHashRange";
        private const string SP_READ_HASHBIN_HASHBUCKET_HASHTABLE = "sp_HashBinReadByHashBucketHashTable";
        private const string SP_READ_HASHBINS_BY_HASH_BIN = "sp_ReadHashBinsByHashBinsMinHash";
        private const string SP_UPDATE_HASHBUCKET_MINHASH = "sp_HashBinUpdateHashBucket";
        private const string SP_UPDATE_HASHBUCKET_SINGLE_MINHASH = "sp_HashBinUpdateSingleHashBucket";
        private const string SP_UPDATE_HASHBUCKET_COUNT = "sp_HashBinUpdateHashBucketCount";
        private const string SP_READ_HASH_BUCKETCOUNT_BY_HASHBUCKET = "sp_ReadHashBucketCountPerHashBucket";
        private const string SP_UPDATE_ALL_HASHBUCKET_COUNT_MINHASH = "sp_UpdateAllHashBucketCountMinHash";
        private const string SP_READ_ALL_FINGERPRINTID_BYHASHTABLES_HASHBUCKETS = "sp_HashBinReadAllByHashBucketHashTable";
        private const string SP_DELETE_TRACK = "sp_DeleteTrack";
        private const string SP_READ_FINGERPRINTS_BY_HASHBIN_THRESHOLD = "sp_ReadFingerprintsByHashBinAndThreshold";

        #endregion

        #region Parameteres name

        private const string PARAM_ALBUM_ID = "@Id";
        private const string PARAM_ALBUM_NAME = "@Name";
        private const string PARAM_ALBUM_RELEASE_YEAR = "@ReleaseYear";
        private const string PARAM_TRACK_ID = "@Id";
        private const string PARAM_TRACK_ARTIST = "@Artist";
        private const string PARAM_TRACK_TITLE = "@Title";
        private const string PARAM_TRACK_ALBUM_ID = "@AlbumId";
        private const string PARAM_TRACK_LENGTH_SEC = "@TrackLengthSec";
        private const string PARAM_FINGERPRINT_ID = "@Id";
        private const string PARAM_FINGERPRINT_FINGERPRINT = "@Fingerprint";
        private const string PARAM_FINGERPRINT_TOTAL_FINGERPRINTS_PER_TRACK = "@TotalFingerprintsPerTrack";
        private const string PARAM_FINGERPRINT_TRACK_ID = "@TrackId";
        private const string PARAM_FINGERPRINT_SONG_ORDER = "@SongOrder";
        private const string PARAM_HASHBIN_NEURALHASHER_ID = "@Id";
        private const string PARAM_HASHBIN_NEURALHASHER_HASHBIN = "@HashBin";
        private const string PARAM_HASHBIN_NEURALHASHER_HASHTABLE = "@HashTable";
        private const string PARAM_HASHBIN_NEURALHASHER_TRACK_ID = "@TrackId";
        private const string PARAM_HASHBIN_MINHASH_ID = "@Id";
        private const string PARAM_HASHBIN_MINHASH_HASHBIN = "@HashBin";
        private const string PARAM_HASHBIN_MINHASH_HASHBUCKET = "@HashBucket";
        private const string PARAM_HASHBIN_MINHASH_HASHBUCKET_COUNT = "@HashBucketCount";
        private const string PARAM_HASHBIN_MINHASH_HASHTABLE = "@HashTable";
        private const string PARAM_HASHBIN_MINHASH_TRACK_ID = "@TrackId";
        private const string PARAM_HASHBIN_MINHASH_FINGERPRINT = "@FingerprintId";
        private const string PARAM_NUMBER_OF_FINGERPRINTS_TO_READ = "@NumberOfFingerprintsToRead";
        private const string PARAM_PERMUTATIONS_ID = "@Id";
        private const string PARAM_READ_MIN_MAX_IGNORE = "@Ignore";
        private const string PARAM_READ_HASHBIN_MINHASH_RANGE_MIN = "@Min";
        private const string PARAM_READ_HASHBIN_MINHASH_RANGE_MAX = "@Max";
        private const string PARAM_UPDATE_HASHBUCKET_MINHASH_MIN = "@MinHashBinInclusive";
        private const string PARAM_UPDATE_HASHBUCKET_MINHASH_MAX = "@MaxHashBinExclusive";
        private const string PARAM_CONCAT_HASHTABLES = "@ConcatHashTable";
        private const string PARAM_CONCAT_HASHBUCKETS = "@ConcatHashBucket";
        private const string PARAM_THRESHOLD_VOTE = "@Threshold";
        private const string PARAM_DELIMITER = "@Delimiter";

        #endregion

        #region Size of VARCHAR rows

        private const int SIZE_ALBUM_AUTHOR = 100;
        private const int SIZE_TRACK_ARTIST = 100;
        private const int SIZE_TRACK_TITLE = 100;
        private const int SIZE_CONCAT_TABLES = 4000;
        private const int SIZE_CONCAT_HASHES = 4000;
        private const int SIZE_DELIMITER = 10;

        #endregion

        /// <summary>
        ///   Gets stored procedure with the assigned name
        /// </summary>
        /// <param name = "cmdTxt">Name of the stored procedure</param>
        /// <returns>Command to be executed</returns>
        /// <param name = "connection">Connection object to the database</param>
        private DbCommand GetStoredProcedureCommand(string cmdTxt, DbConnection connection)
        {
            if (String.IsNullOrEmpty(cmdTxt)) throw new ArgumentException("cmdTest parameter is null or empty");
            DbCommand cmd = _dbFactory.CreateCommand();
            cmd.CommandText = cmdTxt;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = COMMAND_TIMEOUT;
            cmd.Connection = connection;
            return cmd;
        }

        /// <summary>
        ///   Get a command to insert an item to the database
        /// </summary>
        /// <param name = "album">Album to be inserted</param>
        /// <param name = "connection">Connection used to insert the album</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetInsertAlbumCommand(Album album, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_INSERT_ALBUM, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_ALBUM_NAME;
            param.DbType = DbType.String;
            param.Value = album.Name;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_ALBUM_RELEASE_YEAR;
            param.DbType = DbType.Int32;
            param.Value = album.ReleaseYear;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to insert track in the database
        /// </summary>
        /// <param name = "track">Track to be inserted</param>
        /// <param name = "connection">Connection used to insert the track</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetInsertTrackCommand(Track track, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_INSERT_TRACK, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_ARTIST;
            param.DbType = DbType.String;
            param.Value = track.Artist;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_TITLE;
            param.DbType = DbType.String;
            param.Value = track.Title;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_ALBUM_ID;
            param.DbType = DbType.Int32;
            param.Value = track.AlbumId;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_LENGTH_SEC;
            param.DbType = DbType.Int32;
            param.Value = track.TrackLength;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to inset a fingerprint into the database
        /// </summary>
        /// <param name = "fingerprint">Fingerprint to be inserted</param>
        /// <param name = "connection">Connection used to insert the fingerprint</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetInsertFingerprintCommand(Fingerprint fingerprint, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_INSERT_FINGERPRINT, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_FINGERPRINT_FINGERPRINT;
            param.DbType = DbType.Binary;
            param.Value = ArrayUtils.GetByteArrayFromBool(fingerprint.Signature);
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_FINGERPRINT_TRACK_ID;
            param.DbType = DbType.Int32;
            param.Value = fingerprint.TrackId;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_FINGERPRINT_SONG_ORDER;
            param.DbType = DbType.Int32;
            param.Value = fingerprint.SongOrder;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_FINGERPRINT_TOTAL_FINGERPRINTS_PER_TRACK;
            param.DbType = DbType.Int32;
            param.Value = fingerprint.TotalFingerprintsPerTrack;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to insert a HashBin created by Neural Hasher
        /// </summary>
        /// <param name = "hashBinNeuralHasher">Hash to be inserted</param>
        /// <param name = "connection">Connection used in insertion operation</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetInsertHashBinNeuralHasherCommand(HashBinNeuralHasher hashBinNeuralHasher, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_INSERT_NEURALHASHER_HASHBIN, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_NEURALHASHER_HASHBIN;
            param.DbType = DbType.Int64;
            param.Value = hashBinNeuralHasher.Hashbin;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_NEURALHASHER_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashBinNeuralHasher.HashTable;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_NEURALHASHER_TRACK_ID;
            param.DbType = DbType.Int32;
            param.Value = hashBinNeuralHasher.TrackId;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to insert a HashBin created by Min-Hash + LSH schema into the database
        /// </summary>
        /// <param name = "hashBinMinHash">Hash to be inserted</param>
        /// <param name = "connection">Connection used to insert the hash</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetInsertHashBinMinHashCommand(HashBinMinHash hashBinMinHash, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_INSERT_MINHASH_HASHBIN, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBIN;
            param.DbType = DbType.Int64;
            param.Value = hashBinMinHash.Hashbin;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashBinMinHash.HashTable;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_TRACK_ID;
            param.DbType = DbType.Int32;
            param.Value = hashBinMinHash.TrackId;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_FINGERPRINT;
            param.DbType = DbType.Int32;
            param.Value = hashBinMinHash.HashedFingerprint;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command which will allow to detect track duplicates in the database
        /// </summary>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed.</returns>
        public DbCommand GetReadDuplicatedTracks(DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_DUPLICATED_TRACKS, connection);
            return command;
        }

        /// <summary>
        ///   Get a command to read all the albums from the database
        /// </summary>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadAlbumsCommand(DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_ALBUMS, connection);
            return command;
        }

        /// <summary>
        ///   Get a command to read a track from the database according to the HashBin and HashTable parameters
        /// </summary>
        /// <param name = "hashBin">HashBin created by the neural hasher</param>
        /// <param name = "hashTable">Hash table from which to extract</param>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadTrackIdByHashBinAndHashTableNeuralHasher(long hashBin, int hashTable, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_TRACKID_BY_HASHBIN_HASHTABLE_NEURALHASHER, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_NEURALHASHER_HASHBIN;
            param.DbType = DbType.Int64;
            param.Value = hashBin;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_NEURALHASHER_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashTable;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read all hash bins according to the specified hash bin generated by the Min-Hash + LSH schema
        /// </summary>
        /// <param name = "hashBin">Hash bin to be found in the database</param>
        /// <param name = "hashTable">Hash table in which the hash bin should be found</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        /// <remarks>
        ///   Parameters are generated by the Query manager. In order to identify the song these
        ///   parameters should be found in the underlying database
        /// </remarks>
        public DbCommand GetReadHashBinsByHashBucketAndHashTableLSH(long hashBin, int hashTable, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_HASHBINS_BY_HASHBIN_HASHTABLE_MINHASH, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBIN;
            param.DbType = DbType.Int64;
            param.Value = hashBin;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashTable;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command which allows one to find an album in the database according to its ID
        /// </summary>
        /// <param name = "id">Albums ID</param>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadAlbumsByIdCommand(Int32 id, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_ALBUM_BY_ID, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_ALBUM_ID;
            param.DbType = DbType.Int32;
            param.Value = id;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read the Unknown album from the database
        /// </summary>
        /// <param name = "unknownName">Name of the unknown album</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        /// <remarks>
        ///   Unknown album is important in order to specify it to those songs which are not 
        ///   labeled in their tags with a correct album name
        /// </remarks>
        public DbCommand GetReadUnknownAlbum(string unknownName, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_UNKNOWN_ALBUMS, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_ALBUM_NAME;
            param.DbType = DbType.String;
            param.Value = unknownName;
            command.Parameters.Add(param);
            return command;
        }

        /// <summary>
        ///   Get a command to read all tracks from the database
        /// </summary>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadTracksCommand(DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_TRACKS, connection);
            return command;
        }

        /// <summary>
        ///   Get a command to read a track from the database according to the parameters
        /// </summary>
        /// <param name = "artist">Artist name</param>
        /// <param name = "title">Title of the song</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadTrackByArtistAndTitleNameCommand(string artist, string title, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_TRACK_BY_ARTIST_SONG_NAME, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_ARTIST;
            param.DbType = DbType.String;
            param.Value = artist;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_TITLE;
            param.DbType = DbType.String;
            param.Value = title;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read a track according to it's id
        /// </summary>
        /// <param name = "id">Id of the track to be found in the database</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadTracksByIdCommand(Int32 id, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_TRACK_BY_ID, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_ID;
            param.DbType = DbType.Int32;
            param.Value = id;
            command.Parameters.Add(param);
            return command;
        }

        /// <summary>
        ///   Get a command to read all fingerprints from the database
        /// </summary>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        /// <remarks>
        ///   Do not use this method if there are plenty of songs in the database, or there is not enough RAM to hold every fingerprint.
        ///   Reminder: 1 fingerprint object is equal to 4096 + 16 + 16 + 32 Bytes
        /// </remarks>
        public DbCommand GetReadFingerprintsCommand(DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_FINGERPRINTS, connection);
            return command;
        }

        /// <summary>
        ///   Get a command to read a fingerprint from the database according to the specified ID
        /// </summary>
        /// <param name = "id">Id of the fingerprint to be found</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadFingerprintByIdCommand(Int32 id, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_FINGERPRINT_BY_ID, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_FINGERPRINT_ID;
            param.DbType = DbType.Int32;
            param.Value = id;
            command.Parameters.Add(param);
            return command;
        }

        /// <summary>
        ///   Get a command to read all fingerprints from a specific song
        /// </summary>
        /// <param name = "id">Id of the track to read the fingerprints from</param>
        /// <param name = "numberOfFingerprintsToRead">
        ///   Number of subsequent fingerprints to read for a specific song
        ///   If a parameter less than 0 is specified - all the fingerprints are read.
        /// </param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadFingerprintByTrackIdCommand(Int32 id, int numberOfFingerprintsToRead, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_FINGERPRINT_BY_TRACK_ID, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_ID;
            param.DbType = DbType.Int32;
            param.Value = id;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_NUMBER_OF_FINGERPRINTS_TO_READ;
            param.DbType = DbType.Int32;
            param.Value = numberOfFingerprintsToRead;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read a track according to the fingerprint id
        /// </summary>
        /// <param name = "fingerprintId">ID of the fingerprint</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadTrackByFingerprintCommand(Int32 fingerprintId, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_TRACK_BY_FINGERPRINT, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_FINGERPRINT_ID;
            param.DbType = DbType.Int32;
            param.Value = fingerprintId;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to delete a track according to Id
        /// </summary>
        /// <param name = "trackId">Track Id to be delete</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetDeleteTrackCommand(Int32 trackId, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_DELETE_TRACK, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_TRACK_ID;
            param.DbType = DbType.Int32;
            param.Value = trackId;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read fingerprint candidates according to hash bin and hash table
        /// </summary>
        /// <param name = "hashBin">Hash bin</param>
        /// <param name = "hashTable">Hash table</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadFingerprintCadidatesByHashbinHashtable(long hashBin, int hashTable, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_FINGERPRINT_CANDIDATES_BY_HASHBIN_HASHTABLE_MINHASH, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBIN;
            param.DbType = DbType.Int64;
            param.Value = hashBin;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashTable;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Read hash bins by hash bin itself and return fingerprint id, track id, and table to which it belongs
        /// </summary>
        /// <param name = "hashBin">Hash bin [query point q]</param>
        /// <param name = "connection">Connection to the database</param>
        /// <returns>SQL Command to be executed</returns>
        public DbCommand GetReadHashBinsByHashBinsMinHashLSH(long hashBin, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_HASHBINS_BY_HASH_BIN, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBIN;
            param.DbType = DbType.Int64;
            param.Value = hashBin;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read minimal/maximal HashBins. This is useful when finding the range of values in the
        ///   hash buckets (histogram) of the HashBinMinHash table.
        /// </summary>
        /// <param name = "ignore">
        ///   Hash bin to be ignored.
        ///   Normally it is the maximum value when NO 1's have been found in the fingerprint signature.
        ///   E.g. 5 keys per table results in <c>2^5*sizeof(byte) - 1</c> value
        /// </param>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadMinMaxHashBinMinHash(long ignore, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_MIN_MAX_HASHBIN_MINHASH, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_READ_MIN_MAX_IGNORE;
            param.DbType = DbType.Int64;
            param.Value = ignore;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read a range of HashBins according to some range.
        /// </summary>
        /// <param name = "startInclusive">Start hash bin</param>
        /// <param name = "endInclusive">End hash bin</param>
        /// <param name = "hashTable">Hash table</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadHashBinMinhashRange(long startInclusive, long endInclusive, int hashTable, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_HASHBINMINHASH_RANGE, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_READ_HASHBIN_MINHASH_RANGE_MIN;
            param.DbType = DbType.Int64;
            param.Value = startInclusive;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_READ_HASHBIN_MINHASH_RANGE_MAX;
            param.DbType = DbType.Int64;
            param.Value = endInclusive;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashTable;
            command.Parameters.Add(param);

            return command;
        }


        /// <summary>
        ///   Updates the values of the hash bucket according to the start, end index
        /// </summary>
        /// <param name = "hashBucket">Hash bucket</param>
        /// <param name = "startInclusive">Start inclusive index</param>
        /// <param name = "endExclusive">End exclusive index</param>
        /// <param name = "hashTable">Hash table</param>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns></returns>
        public DbCommand GetUpdateHashBucket(int hashBucket, long startInclusive, long endExclusive, int hashTable, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_UPDATE_HASHBUCKET_MINHASH, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBUCKET;
            param.DbType = DbType.Int32;
            param.Value = hashBucket;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_UPDATE_HASHBUCKET_MINHASH_MIN;
            param.DbType = DbType.Int64;
            param.Value = startInclusive;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_UPDATE_HASHBUCKET_MINHASH_MAX;
            param.DbType = DbType.Int64;
            param.Value = endExclusive;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashTable;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to update a single hash bucket of a specific hash bin
        /// </summary>
        /// <param name = "hashBucket">New hash bucket value</param>
        /// <param name = "hashBin">Hash bin to be updated</param>
        /// <param name = "hashTable">Hash table in which one should look for the hash bin</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetUpdateHashBucketSingle(int hashBucket, long hashBin, int hashTable, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_UPDATE_HASHBUCKET_SINGLE_MINHASH, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBUCKET;
            param.DbType = DbType.Int32;
            param.Value = hashBucket;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBIN;
            param.DbType = DbType.Int64;
            param.Value = hashBin;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashTable;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read all hash bins in a specific hash bucket and hash table
        /// </summary>
        /// <param name = "hashBucket">Hash bucket</param>
        /// <param name = "hashTable">Hash table</param>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadHashBinByHashBucketHashTable(int hashBucket, int hashTable, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_HASHBIN_HASHBUCKET_HASHTABLE, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBUCKET;
            param.DbType = DbType.Int32;
            param.Value = hashBucket;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHTABLE;
            param.DbType = DbType.Int32;
            param.Value = hashTable;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to update a hash bucket count parameter
        /// </summary>
        /// <param name = "hashBucket">Hash bucket to be updated</param>
        /// <param name = "hashBucketCount">New count value</param>
        /// <param name = "connection">Connection used to executed the command</param>
        /// <returns>Command to be executed</returns>
        /// <remarks>
        ///   Be careful while using this method. It updates the count parameter according to
        ///   the SUM of Grouped BY hash buckets (meaning that no Hash tables are taken into
        ///   account while summing the buckets).
        /// </remarks>
        public DbCommand GetUpdateHashBucketCount(int hashBucket, int hashBucketCount, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_UPDATE_HASHBUCKET_COUNT, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBUCKET;
            param.DbType = DbType.Int32;
            param.Value = hashBucket;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_HASHBIN_MINHASH_HASHBUCKET_COUNT;
            param.DbType = DbType.Int32;
            param.Value = hashBucketCount;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Get a command to read hash buckets count grouped by hash buckets (no hash tables are taken into account)
        /// </summary>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadHashBucketCountByHashBucket(DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_HASH_BUCKETCOUNT_BY_HASHBUCKET, connection);
            return command;
        }

        /// <summary>
        ///   Get a command to update the hash buckets count according to the hash bucket/hash table
        /// </summary>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>SWL command to be executed</returns>
        [Obsolete("The method is very slow. Use another way of updating the values")]
        public DbCommand GetUpdateAllHashBucketCountMinHash(DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_UPDATE_ALL_HASHBUCKET_COUNT_MINHASH, connection);
            return command;
        }

        /// <summary>
        ///   Get a command to read Votes/FingerprintId according to all concatenated hash tables and hash buckets
        /// </summary>
        /// <param name = "hashtables">Concatenated hash tables [E.g. '0,1,2,3,4,5,...20']</param>
        /// <param name = "hashbuckets">Concatenated hash buckets [E.g. '10,458,125...,20056']</param>
        /// <param name = "delimiter">Delimiter of the concatenated strings</param>
        /// <param name = "threshold">Threshold vote</param>
        /// <param name = "connection">Connection used to execute the command</param>
        /// <returns>Command to be executed</returns>
        /// <remarks>
        ///   Number of elements in concatenated hash tables and concatenated hash buckets should be the same.
        ///   One to one mapping is done
        /// </remarks>
        public DbCommand GetReadAllFingerprintIdByHashTablesHashBucketsMinHash(string hashtables, string hashbuckets, string delimiter, int threshold, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_ALL_FINGERPRINTID_BYHASHTABLES_HASHBUCKETS, connection);

            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_CONCAT_HASHTABLES;
            param.DbType = DbType.String;
            param.Value = hashtables;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_CONCAT_HASHBUCKETS;
            param.DbType = DbType.String;
            param.Value = hashbuckets;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_DELIMITER;
            param.DbType = DbType.String;
            param.Value = delimiter;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_THRESHOLD_VOTE;
            param.DbType = DbType.String;
            param.Value = delimiter;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_THRESHOLD_VOTE;
            param.DbType = DbType.Int32;
            param.Value = threshold;
            command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        ///   Read fingerprints by supplied hash bins and a specific threshold
        /// </summary>
        /// <param name = "hashbins">Hash bins to be read</param>
        /// <param name = "delimiter">Delimiter between the hash bins</param>
        /// <param name = "threshold">Threshold value</param>
        /// <param name = "connection">Connection object to the database</param>
        /// <returns>Command to be executed</returns>
        public DbCommand GetReadFingerprintsByThresholdAndHashBinsMinHash(string hashbins, string delimiter, int threshold, DbConnection connection)
        {
            DbCommand command = GetStoredProcedureCommand(SP_READ_FINGERPRINTS_BY_HASHBIN_THRESHOLD, connection);
            DbParameter param = command.CreateParameter();
            param.ParameterName = PARAM_CONCAT_HASHBUCKETS;
            param.DbType = DbType.String;
            param.Value = hashbins;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_THRESHOLD_VOTE;
            param.DbType = DbType.String;
            param.Value = delimiter;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = PARAM_THRESHOLD_VOTE;
            param.DbType = DbType.Int32;
            param.Value = threshold;
            command.Parameters.Add(param);
            return command;
        }
    }
}