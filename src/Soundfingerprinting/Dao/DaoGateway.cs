namespace Soundfingerprinting.DbStorage
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    using Soundfingerprinting.Dao.Internal;
    using Soundfingerprinting.DbStorage.Entities;

    [SqlClientPermission(SecurityAction.Demand)]
    public class DaoGateway
    {
        #region Constants

        #region Field's name taken from fingerprints database

        private const string FieldAlbumId = "Id";
        private const string FIELD_ALBUM_NAME = "Name";
        private const string FIELD_ALBUM_RELEASE_YEAR = "ReleaseYear";
        private const string FIELD_TRACK_ID = "Id";
        private const string FIELD_TRACK_ARTIST = "Artist";
        private const string FIELD_TRACK_TITLE = "Title";
        private const string FIELD_TRACK_ALBUM_ID = "AlbumId";
        private const string FIELD_TRACK_LENGTH_SEC = "TrackLengthSec";
        private const string FIELD_FINGERPRINT_ID = "Id";
        private const string FIELD_FINGERPRINT_FINGERPRINT = "Fingerprint";
        private const string FIELD_FINGERPRINT_TOTAL_FINGERPRINTS_PER_TRACK = "TotalFingerprintsPerTrack";
        private const string FIELD_FINGERPRINT_TRACK_ID = "TrackId";
        private const string FIELD_FINGERPRINT_SONG_ORDER = "SongOrder";
        private const string FIELD_HASHBIN_NEURALHASHER_ID = "Id";
        private const string FIELD_HASHBIN_NEURALHASHER_HASHBIN = "HashBinNeuralHasher";
        private const string FIELD_HASHBIN_NEURALHASHER_HASHTABLE = "HashTable";
        private const string FIELD_HASHBIN_NEURALHASHER_TRACK_ID = "TrackId";
        private const string FIELD_HASHBIN_MINHASH_ID = "Id";
        private const string FIELD_HASHBIN_MINHASH_HASHBIN = "HashBin";
        private const string FIELD_HASHBIN_MINHASH_HASHTABLE = "HashTable";
        private const string FIELD_HASHBIN_MINHASH_TRACK_ID = "TrackId";
        private const string FIELD_HASHBIN_MINHASH_FINGERPRINT = "FingerprintId";
        private const string FIELD_HASHBIN_MINHASH_HASHBUCKET = "HashBucket";
        private const string FIELD_HASHBIN_MINHASH_HASHBUCKET_COUNT = "HashBucketCount";

        private const string FIELD_DUPLICATES = "Duplicates";
        private const string FIELD_MIN_HASHBIN = "MinHashBin";
        private const string FIELD_MAX_HASHBIN = "MaxHashBin";
        private const string FIELD_VOTES = "Votes";

        #endregion

        #endregion

        /// <summary>
        ///   Database provider factory instance
        /// </summary>
        /// <remarks>
        ///   Singleton instance providing access to methods for creating generic data access objects
        /// </remarks>
        private readonly DbProviderFactory databaseProvider;

        private readonly DaoStoredProcedureBuilder storeProcedureBuilder;

        /// <summary>
        ///   Connection string
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaoGateway"/> class. 
        /// Instantiate data access manager using FingerprintConnectionString read from the App.Config.
        ///   If no such string is found in the App.Config, connection string is set to Empty
        /// </summary>
        /// <param name="connectionString">
        /// The connection String.
        /// </param>
        public DaoGateway(string connectionString)
        {
            databaseProvider = SqlClientFactory.Instance; /*the only field that needs to be changed in case of a DB switch*/
            this.connectionString = connectionString;
            storeProcedureBuilder = new DaoStoredProcedureBuilder(databaseProvider);
        }

        /// <summary>
        ///   Gets the connection to MSSQL Server
        /// </summary>
        /// <returns>SQL Connection object</returns>
        private DbConnection GetConnection()
        {
            DbConnection connection = databaseProvider.CreateConnection();
            if (connection != null)
            {
                connection.ConnectionString = connectionString;
                return connection;
            }
            throw new Exception("Cannot create connection of db provider type " + databaseProvider.GetType());
        }

        #region Utilities

        /// <summary>
        ///   Fills an album with the values taken from the executed data reader
        /// </summary>
        /// <param name = "sqlDr">Data reader with the actual values</param>
        /// <returns>Filled object</returns>
        private static Album FillAlbum(DbDataReader sqlDr)
        {
            Album a = null;
            if (sqlDr.Read()) //positioning the cursor
            {
                a = new Album((Int32) sqlDr[FieldAlbumId],
                    (String) sqlDr[FIELD_ALBUM_NAME],
                    (int) sqlDr[FIELD_ALBUM_RELEASE_YEAR]);
            }
            return a;
        }

        /// <summary>
        ///   Fills a list of albums with the values taken from the executed data reader
        /// </summary>
        /// <param name = "sqlDr">SQL data reader with the corresponding album objects</param>
        /// <returns>List of albums, or null if reader is empty</returns>
        private static List<Album> FillAlbumList(DbDataReader sqlDr)
        {
            List<Album> lAlbum = null;
            if (sqlDr.Read()) //positioning the cursor
            {
                lAlbum = new List<Album>();
                do
                {
                    Album a = new Album((Int32) sqlDr[FieldAlbumId],
                        (String) sqlDr[FIELD_ALBUM_NAME],
                        (int) sqlDr[FIELD_ALBUM_RELEASE_YEAR]);

                    lAlbum.Add(a);
                } while (sqlDr.Read());
            }
            return lAlbum;
        }

        /// <summary>
        ///   Fill track with the data gathered from the underlying data reader
        /// </summary>
        /// <param name = "sqlDr">Data reader with the actual data</param>
        /// <returns>Filled track</returns>
        private static Track FillTrack(DbDataReader sqlDr)
        {
            Track a = null;
            if (sqlDr.Read()) //positioning the cursor
            {
                a = new Track(
                    (Int32) sqlDr[FIELD_TRACK_ID],
                    (String) sqlDr[FIELD_TRACK_ARTIST],
                    (String) sqlDr[FIELD_TRACK_TITLE],
                    (Int32) sqlDr[FIELD_TRACK_ALBUM_ID],
                    (int) sqlDr[FIELD_TRACK_LENGTH_SEC]);
            }
            return a;
        }

        /// <summary>
        ///   Fill track list with the corresponding data fetched from the SQL database
        /// </summary>
        /// <param name = "sqlDr">SQL data reader</param>
        /// <returns>List of track</returns>
        private static List<Track> FillTrackList(DbDataReader sqlDr)
        {
            List<Track> lTrack = null;
            if (sqlDr.Read())
            {
                lTrack = new List<Track>();
                do
                {
                    Track a = new Track((Int32) sqlDr[FIELD_TRACK_ID],
                        (String) sqlDr[FIELD_TRACK_ARTIST],
                        (String) sqlDr[FIELD_TRACK_TITLE],
                        (Int32) sqlDr[FIELD_TRACK_ALBUM_ID],
                        (int) sqlDr[FIELD_TRACK_LENGTH_SEC]);

                    lTrack.Add(a);
                } while (sqlDr.Read());
            }
            return lTrack;
        }

        /// <summary>
        ///   Fill fingerprint with the actual data gathered from the data reader.
        /// </summary>
        /// <param name = "sqlDr">Data reader with the actual data</param>
        /// <returns>Filled object</returns>
        private static Fingerprint FillFingerprint(DbDataReader sqlDr)
        {
            Fingerprint a = null;
            if (sqlDr.Read()) //positioning the cursor
            {
                byte[] bytesignature = (byte[]) sqlDr[FIELD_FINGERPRINT_FINGERPRINT];
                bool[] signature = new bool[bytesignature.Length*8];
                BitArray b = new BitArray(bytesignature);
                b.CopyTo(signature, 0);
                a = new Fingerprint((Int32) sqlDr[FIELD_FINGERPRINT_ID],
                    signature,
                    (Int32) sqlDr[FIELD_FINGERPRINT_TRACK_ID],
                    (int) sqlDr[FIELD_FINGERPRINT_SONG_ORDER],
                    (int) sqlDr[FIELD_FINGERPRINT_TOTAL_FINGERPRINTS_PER_TRACK]);
            }
            return a;
        }

        /// <summary>
        ///   Populate a list with the fingerprints from the data gathered from the database
        /// </summary>
        /// <param name = "sqlDr">SQL data reader</param>
        /// <returns>List of fingerprints</returns>
        private static List<Fingerprint> FillFingerprintList(DbDataReader sqlDr)
        {
            List<Fingerprint> lFingerprint = null;
            if (sqlDr.Read()) //positioning the cursor
            {
                lFingerprint = new List<Fingerprint>();
                do
                {
                    byte[] bytesignature = (byte[]) sqlDr[FIELD_FINGERPRINT_FINGERPRINT];
                    bool[] signature = new bool[bytesignature.Length*8];
                    BitArray b = new BitArray(bytesignature);
                    b.CopyTo(signature, 0);
                    Fingerprint a = new Fingerprint((Int32) sqlDr[FIELD_FINGERPRINT_ID],
                        signature,
                        (Int32) sqlDr[FIELD_FINGERPRINT_TRACK_ID],
                        (int) sqlDr[FIELD_FINGERPRINT_SONG_ORDER],
                        (int) sqlDr[FIELD_FINGERPRINT_TOTAL_FINGERPRINTS_PER_TRACK]);

                    lFingerprint.Add(a);
                } while (sqlDr.Read());
            }
            return lFingerprint;
        }

        #endregion

        #region Insert Methods

        /// <summary>
        ///   Insert Fingerprint into the database
        /// </summary>
        /// <param name = "fingerprint">Fingerprint to insert</param>
        /// <exception cref = "ArgumentException">Fingerprint object cannot be null</exception>
        public void InsertFingerprint(Fingerprint fingerprint)
        {
            if (fingerprint == null)
                throw new ArgumentException("fingerprint cannot be null");
            DbCommand cmd = storeProcedureBuilder.GetInsertFingerprintCommand(fingerprint, GetConnection()); /*Get Command*/
            try
            {
                cmd.Connection.Open(); /*Open Connection*/
                fingerprint.Id = (int) cmd.ExecuteScalar();
            }
            finally
            {
                cmd.Connection.Close(); /*Close connection*/
            }
        }

        /// <summary>
        ///   Insert a collection of Fingerprints in the database
        /// </summary>
        /// <param name = "collection">Collection of fingerprints to insert</param>
        /// <exception cref = "ArgumentException">Collection cannot be empty</exception>
        /// <exception cref = "DalGatewayException">Fingerprint's Int32 cannot be empty</exception>
        public void InsertFingerprint(IEnumerable<Fingerprint> collection)
        {
            if (collection.Count() == 0)
                throw new ArgumentException("collection cannot be empty");
            using (DbConnection con = GetConnection())
            {
                try
                {
                    con.Open(); /*Open the connection*/
                    foreach (Fingerprint f in collection)
                    {
                        DbCommand cmd = storeProcedureBuilder.GetInsertFingerprintCommand(f, con); /*Pass the same connection object for the second query*/
                        f.Id = (int) cmd.ExecuteScalar(); /*Execute command*/
                    }
                }
                finally
                {
                    con.Close(); /*Close connection*/
                }
            }
        }

        /// <summary>
        ///   Insert Track into the database
        /// </summary>
        /// <param name = "track">Track to insert</param>
        /// <exception cref = "ArgumentException">track cannot be null</exception>
        public void InsertTrack(Track track)
        {
            if (track == null)
                throw new ArgumentException("track cannot be null");
            DbCommand cmd = storeProcedureBuilder.GetInsertTrackCommand(track, GetConnection()); /*Get Command*/
            try
            {
                cmd.Connection.Open(); /*Open Connection*/
                track.Id = (int) cmd.ExecuteScalar();
            }
            finally
            {
                cmd.Connection.Close(); /*Close connection*/
            }
        }

        /// <summary>
        ///   Insert a collection of Tracks in the database
        /// </summary>
        /// <param name = "collection">Collection of tracks to insert</param>
        /// <exception cref = "ArgumentException">collection cannot be empty</exception>
        /// <exception cref = "DalGatewayException">track's Int32 cannot be empty</exception>
        public void InsertTrack(IEnumerable<Track> collection)
        {
            if (collection == null || collection.Count() == 0)
                return;
            using (DbConnection con = GetConnection())
            {
                try
                {
                    con.Open(); /*Open the connection*/
                    foreach (Track t in collection)
                    {
                        DbCommand cmd = storeProcedureBuilder.GetInsertTrackCommand(t, con); /*Pass the same connection object for the second query*/
                        t.Id = (int) cmd.ExecuteScalar();
                    }
                }
                finally
                {
                    con.Close(); /*Close connection*/
                }
            }
        }

        /// <summary>
        ///   Insert Album in the database
        /// </summary>
        /// <param name = "album">Album to insert</param>
        /// <exception cref = "ArgumentException">album parameter cannot be null</exception>
        public void InsertAlbum(Album album)
        {
            if (album == null)
                throw new ArgumentException("album cannot be null");
            DbCommand cmd = storeProcedureBuilder.GetInsertAlbumCommand(album, GetConnection()); /*Get Command*/
            try
            {
                cmd.Connection.Open(); /*Open Connection*/
                album.Id = (int) cmd.ExecuteScalar();
            }
            finally
            {
                cmd.Connection.Close(); /*Close connection*/
            }
        }

        /// <summary>
        ///   Insert a collection of Albums in the database
        /// </summary>
        /// <param name = "collection">Collection of albums to insert</param>
        /// <exception cref = "ArgumentException">collection cannot be empty</exception>
        /// <exception cref = "DalGatewayException">album's Int32 cannot be empty</exception>
        public virtual void InsertAlbum(IEnumerable<Album> collection)
        {
            if (collection.Count() == 0)
                throw new ArgumentException("collection cannot be empty");
            using (DbConnection con = GetConnection()) /*Get Connection*/
            {
                try
                {
                    con.Open(); /*Open the connection*/
                    foreach (Album a in collection)
                    {
                        DbCommand cmd = storeProcedureBuilder.GetInsertAlbumCommand(a, con); /*Pass the same connection object for the second query*/
                        a.Id = (int) cmd.ExecuteScalar(); /*Execute command*/
                    }
                }
                finally
                {
                    con.Close(); /*Close connection*/
                }
            }
        }

        /// <summary>
        ///   Insert HashBin into the database
        /// </summary>
        /// <param name = "hashBin">HashBin to insert</param>
        /// <exception cref = "ArgumentException">Hash bins cannot be null</exception>
        public void InsertHashBin(HashBin hashBin)
        {
            if (hashBin == null)
                throw new ArgumentException("hashBin cannot be null");
            DbCommand cmd = null;
            if (hashBin is HashBinNeuralHasher)
                cmd = storeProcedureBuilder.GetInsertHashBinNeuralHasherCommand(hashBin as HashBinNeuralHasher, GetConnection()); /*Get Command*/
            else if (hashBin is HashBinMinHash)
                cmd = storeProcedureBuilder.GetInsertHashBinMinHashCommand(hashBin as HashBinMinHash, GetConnection()); /*Get Command*/
            else
                throw new ArgumentException("hashBin is of unknown type");
            try
            {
                cmd.Connection.Open(); /*Open Connection*/
                hashBin.Id = (int) cmd.ExecuteScalar();
            }
            finally
            {
                cmd.Connection.Close(); /*Close connection*/
            }
        }

        /// <summary>
        ///   Insert a collection of HashBins in the database
        /// </summary>
        /// <param name = "collection">Collection of hash bins to insert</param>
        /// <exception cref = "ArgumentException">collection cannot be empty</exception>
        /// <exception cref = "DalGatewayException">hash bin's Int32 cannot be empty</exception>
        public void InsertHashBin<T>(IEnumerable<T> collection)
        {
            if (collection.Count() == 0)
                throw new ArgumentException("collection cannot be empty");
            if (collection is IEnumerable<HashBinMinHash>)
            {
                using (DbConnection con = GetConnection())
                {
                    try
                    {
                        con.Open(); /*Open the connection*/
                        foreach (T h in collection)
                        {
                            HashBinMinHash hash = h as HashBinMinHash;
                            if (hash != null)
                            {
                                DbCommand cmd = storeProcedureBuilder.GetInsertHashBinMinHashCommand(hash, con); /*Pass the same connection object for the second query*/
                                hash.Id = (int) cmd.ExecuteScalar();
                            }
                        }
                    }
                    finally
                    {
                        con.Close(); /*Close connection*/
                    }
                }
            }
            else if (collection is IEnumerable<HashBinNeuralHasher>)
            {
                using (DbConnection con = GetConnection())
                {
                    try
                    {
                        con.Open(); /*Open the connection*/
                        foreach (T h in collection)
                        {
                            HashBinNeuralHasher hash = h as HashBinNeuralHasher;
                            if (hash != null)
                            {
                                DbCommand cmd = storeProcedureBuilder.GetInsertHashBinNeuralHasherCommand(hash, con); /*Pass the same connection object for the second query*/
                                hash.Id = (int) cmd.ExecuteScalar();
                            }
                        }
                    }
                    finally
                    {
                        con.Close(); /*Close connection*/
                    }
                }
            }
        }

        #endregion

        #region Read Methods

        /// <summary>
        ///   Read duplicated tracks from the database
        /// </summary>
        /// <returns>KeyValuePaur collection with tracks and number of occurrences of duplicates</returns>
        public Dictionary<Track, int> ReadDuplicatedTracks()
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadDuplicatedTracks(GetConnection());
            DbDataReader reader = null;
            Dictionary<Track, int> result = new Dictionary<Track, int>();
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                    while (reader.Read())
                    {
                        Track duplicate = new Track((Int32) reader[FIELD_TRACK_ID],
                            (String) reader[FIELD_TRACK_ARTIST],
                            (String) reader[FIELD_TRACK_TITLE], -1);

                        int numberOfDuplicates = (int) reader[FIELD_DUPLICATES];
                        result.Add(duplicate, numberOfDuplicates);
                    }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return result;
        }

        /// <summary>
        ///   Read all fingerprints from the database
        /// </summary>
        /// <returns>List of fingerprints which are available in the database</returns>
        public List<Fingerprint> ReadFingerprints()
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadFingerprintsCommand(GetConnection());
            List<Fingerprint> listOfFingerprints = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    listOfFingerprints = FillFingerprintList(reader);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return listOfFingerprints;
        }

        /// <summary>
        ///   Read Fingerprints by Track Id
        /// </summary>
        /// <param name = "trackId">Track id of the fingerprints to read</param>
        /// <returns>List of fingerprints</returns>
        /// <param name = "numberOfFingerprintsToRead">Number of fingerprints to read</param>
        /// <exception cref = "ArgumentException">trackId cannot by empty</exception>
        public List<Fingerprint> ReadFingerprintsByTrackId(Int32 trackId, int numberOfFingerprintsToRead)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadFingerprintByTrackIdCommand(trackId, numberOfFingerprintsToRead, GetConnection());
            List<Fingerprint> listOfFingerprints = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    listOfFingerprints = FillFingerprintList(reader);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }

            return listOfFingerprints;
        }

        /// <summary>
        ///   Read fingerprints by multiple track id
        /// </summary>
        /// <param name = "tracks">List of tracks</param>
        /// <param name = "numberOfFingerprintsToRead">Number of fingerprints to read</param>
        /// <returns>Dictionary with track id/fingerprints</returns>
        public Dictionary<Int32, List<Fingerprint>> ReadFingerprintsByMultipleTrackId(List<Track> tracks, int numberOfFingerprintsToRead)
        {
            DbDataReader reader = null;
            DbConnection sqlConnection = GetConnection();
            Dictionary<Int32, List<Fingerprint>> returnResult = null;
            try
            {
                sqlConnection.Open();
                foreach (Track track in tracks)
                {
                    DbCommand sqlCmd = storeProcedureBuilder.GetReadFingerprintByTrackIdCommand(track.Id, numberOfFingerprintsToRead, sqlConnection);
                    List<Fingerprint> listOfFingerprints = null;
                    reader = sqlCmd.ExecuteReader();
                    if (reader != null)
                    {
                        listOfFingerprints = FillFingerprintList(reader);
                        if (listOfFingerprints != null && listOfFingerprints.Count > 0)
                        {
                            if (returnResult == null)
                                returnResult = new Dictionary<Int32, List<Fingerprint>>();
                            returnResult.Add(track.Id, listOfFingerprints);
                        }
                        reader.Close();
                    }
                }
            }
            finally
            {
                sqlConnection.Close();
            }
            return returnResult;
        }

        /// <summary>
        ///   Read Fingerprint from the database according to it's GUID
        /// </summary>
        /// <param name = "id">Int32 of the Fingerprint to read</param>
        /// <returns>Fingerprint object if such, otherwise null</returns>
        public Fingerprint ReadFingerprintById(Int32 id)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadFingerprintByIdCommand(id, GetConnection());
            Fingerprint retVal = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    retVal = FillFingerprint(reader); /*FILL THE OBJECT WITH THE RESULT*/
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return retVal;
        }

        /// <summary>
        ///   Read fingerprints by id (concurrent read)
        /// </summary>
        /// <param name = "ids">Ids to be read</param>
        /// <returns>List of corresponding fingerprints</returns>
        public List<Fingerprint> ReadFingerprintByIdConcurrent(IEnumerable<Int32> ids)
        {
            const int threads = 4; /*Number of threads that will perform concurrent read*/
            if (ids.Count() < threads*25) /*Check whether it worth's reading in concurrent mode*/
                return ReadFingerprintById(ids);
            List<Int32>[] idsPerThread = new List<Int32>[threads];
            List<Int32> listOfIds = ids.ToList();
            int totalCount = ids.Count();
            int elementsInArray, startIndex = 0;
            for (int i = 0; i < threads; i++) /*Divide the list into Threads*/
            {
                elementsInArray = (i == threads - 1) ? totalCount/threads + totalCount%threads : totalCount/threads;
                idsPerThread[i] = listOfIds.GetRange(startIndex, elementsInArray);
                startIndex += elementsInArray;
            }

            Semaphore turnstyle = new Semaphore(0, 1);
            int endedOperation = 0;
            List<Fingerprint> returnResult = new List<Fingerprint>(); /*Return list*/
            for (int i = 0; i < threads; i++)
            {
                Func<IEnumerable<Int32>, List<Fingerprint>> function = ReadFingerprintById; /*Synchronous counter part*/
                function.BeginInvoke(idsPerThread[i], (result) =>
                                                      {
                                                          List<Fingerprint> items = function.EndInvoke(result);
                                                          lock (this)
                                                          {
                                                              if (items != null)
                                                                  returnResult.AddRange(items);
                                                              endedOperation++;
                                                              if (endedOperation == threads)
                                                                  turnstyle.Release();
                                                          }
                                                      }, function);
            }
            turnstyle.WaitOne();
            turnstyle.Dispose();
            return returnResult;
        }

        /// <summary>
        ///   Read Fingerprint from the database according to it's GUID
        /// </summary>
        /// <param name = "ids">Int32 of the Fingerprint to read</param>
        /// <returns>Fingerprint object if such, otherwise null</returns>
        public List<Fingerprint> ReadFingerprintById(IEnumerable<Int32> ids)
        {
            if (ids == null)
                throw new ArgumentException("ids cannot be empty");
            DbConnection connection = GetConnection();
            List<Fingerprint> retVal = null;
            DbDataReader reader = null;
            try
            {
                connection.Open();
                foreach (Int32 id in ids)
                {
                    DbCommand sqlCmd = storeProcedureBuilder.GetReadFingerprintByIdCommand(id, connection);
                    reader = sqlCmd.ExecuteReader();
                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            if (retVal == null)
                                retVal = new List<Fingerprint>();
                            byte[] bytesignature = (byte[]) reader[FIELD_FINGERPRINT_FINGERPRINT];
                            bool[] signature = new bool[bytesignature.Length*8];
                            BitArray b = new BitArray(bytesignature);
                            b.CopyTo(signature, 0);
                            Fingerprint f = new Fingerprint((Int32) reader[FIELD_FINGERPRINT_ID],
                                signature,
                                (Int32) reader[FIELD_FINGERPRINT_TRACK_ID],
                                (int) reader[FIELD_FINGERPRINT_SONG_ORDER],
                                (int) reader[FIELD_FINGERPRINT_TOTAL_FINGERPRINTS_PER_TRACK]);
                            retVal.Add(f);
                        }
                        reader.Close();
                    }
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                connection.Close();
            }
            return retVal;
        }

        /// <summary>
        ///   Read all tracks from the database
        /// </summary>
        /// <returns>List of tracks</returns>
        public virtual List<Track> ReadTracks()
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadTracksCommand(GetConnection());
            List<Track> listOfTracks = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    listOfTracks = FillTrackList(reader);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return listOfTracks;
        }


        /// <summary>
        ///   Read track from the database according to it's Int32
        /// </summary>
        /// <param name = "id">Id of the track to read</param>
        /// <returns>Track object if such, otherwise null</returns>
        public Track ReadTrackById(Int32 id)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadTracksByIdCommand(id, GetConnection());
            Track retVal = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    retVal = FillTrack(reader); /*FILL THE OBJECT WITH THE RESULT*/
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return retVal;
        }


        /// <summary>
        ///   Read track from the database according to it's Int32
        /// </summary>
        /// <param name = "artist">Artist name</param>
        /// <param name = "title">Title name</param>
        /// <returns>Track object if such, otherwise null</returns>
        public Track ReadTrackByArtistAndTitleName(string artist, string title)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadTrackByArtistAndTitleNameCommand(artist, title, GetConnection());
            Track retVal = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    retVal = FillTrack(reader); /*FILL THE OBJECT WITH THE RESULT*/
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return retVal;
        }

        /// <summary>
        ///   Read all available albums from the database
        /// </summary>
        /// <returns>All albums</returns>
        public List<Album> ReadAlbums()
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadAlbumsCommand(GetConnection());
            List<Album> listOfAlbums = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    listOfAlbums = FillAlbumList(reader);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return listOfAlbums;
        }

        /// <summary>
        ///   Read Unknown Album from the database
        /// </summary>
        /// <returns></returns>
        public Album ReadUnknownAlbum()
        {
            const string unknownName = "UNKNOWN";
            DbCommand sqlCmd = storeProcedureBuilder.GetReadUnknownAlbum(unknownName, GetConnection());
            Album album = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    album = FillAlbum(reader);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return album;
        }

        /// <summary>
        ///   Read Album by name from the database
        /// </summary>
        /// <returns></returns>
        public Album ReadAlbumByName(string name)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadUnknownAlbum(name, GetConnection());
            Album album = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    album = FillAlbum(reader);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return album;
        }

        /// <summary>
        ///   Read album from the database according to it's Id
        /// </summary>
        /// <param name = "id">Id of the object to read</param>
        /// <returns>Album object if such, otherwise null</returns>
        public Album ReadAlbumById(Int32 id)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadAlbumsByIdCommand(id, GetConnection());
            Album retVal = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    retVal = FillAlbum(reader); /*FILL THE OBJECT WITH THE RESULT*/
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return retVal;
        }

        /// <summary>
        ///   Read tracks from the database which correspond to a specific fingerprint id
        /// </summary>
        /// <param name = "id">Id of the fingerprint</param>
        /// <returns>List of corresponding tracks.</returns>
        public List<Track> ReadTrackByFingerprint(Int32 id)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadTrackByFingerprintCommand(id, GetConnection());
            List<Track> listOfTracks = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    listOfTracks = FillTrackList(reader);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return listOfTracks;
        }

        /// <summary>
        ///   Get a list of Track Id's which correspond to a specific hash bin and hash table
        ///   <param name = "hashBin">Hash bin</param>
        ///   <param name = "hashTable">Hash table</param>
        /// </summary>
        /// <remarks>
        ///   The following SQL Command is executed:
        ///   SELECT TrackId FROM HashBins WHERE HashBin = @HashBin AND HashTable = @HashTable
        /// </remarks>
        public List<Int32> ReadTrackIdByHashBinAndHashTableNeuralHasher(long hashBin, int hashTable)
        {
            DbCommand sqlCmd = storeProcedureBuilder.GetReadTrackIdByHashBinAndHashTableNeuralHasher(hashBin, hashTable, GetConnection());
            List<Int32> listOfTracks = null;
            DbDataReader reader = null;
            try
            {
                sqlCmd.Connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        listOfTracks = new List<Int32>();
                        do
                        {
                            Int32 id = (Int32) reader[FIELD_HASHBIN_NEURALHASHER_TRACK_ID];
                            listOfTracks.Add(id);
                        } while (reader.Read());
                    }
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCmd.Connection.Close();
            }
            return listOfTracks;
        }

        /// <summary>
        ///   Read tracks id from the database which correspond to a specific fingerprint id
        ///   <param name = "hashBins">Hash bins to check</param>
        ///   <param name = "hashTables">Hash tables to check</param>
        /// </summary>
        /// <remarks>
        ///   The following SQL Command is executed:
        ///   SELECT TrackId FROM HashBins WHERE HashBin = @HashBin AND HashTable = @HashTable
        ///   The candidates are grouped by track id.
        /// </remarks>
        public Dictionary<Int32, int> ReadTrackIdCandidatesByHashBinAndHashTableNeuralHasher(long[] hashBins, int[] hashTables)
        {
            if (hashBins.Length != hashTables.Length)
                throw new ArgumentException("hashBucket and hashTables have different sizes");
            DbConnection connection = GetConnection();
            Dictionary<Int32, int> dictOfTracks = null;
            DbDataReader reader = null;
            try
            {
                connection.Open();
                for (int i = 0; i < hashTables.Length; i++)
                {
                    DbCommand sqlCmd = storeProcedureBuilder.GetReadTrackIdByHashBinAndHashTableNeuralHasher(hashBins[i], hashTables[i], connection);
                    reader = sqlCmd.ExecuteReader();
                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            if (dictOfTracks == null)
                                dictOfTracks = new Dictionary<Int32, int>();
                            do
                            {
                                Int32 id = (Int32) reader[FIELD_HASHBIN_NEURALHASHER_TRACK_ID];
                                if (!dictOfTracks.ContainsKey(id))
                                    dictOfTracks.Add(id, 0);
                                dictOfTracks[id]++;
                            } while (reader.Read());
                        }
                        reader.Close();
                    }
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                connection.Close();
            }
            return dictOfTracks;
        }


        /// <summary>
        ///   Read tracks id from the database which correspond to a specific fingerprint id
        ///   <param name = "hashBucket">Hash bins to check</param>
        ///   <param name = "hashTables">Hash tables to check</param>
        /// </summary>
        public Dictionary<Int32, List<HashBinMinHash>> ReadFingerprintsByHashBucketAndHashTableLSH(long[] hashBucket, int[] hashTables)
        {
            if (hashBucket.Length != hashTables.Length)
                throw new ArgumentException("hashBucket and hashTables have different sizes");
            DbConnection connection = GetConnection();
            Dictionary<Int32, List<HashBinMinHash>> dictOfFingers = null;
            DbDataReader reader = null;
            try
            {
                connection.Open();
                for (int i = 0; i < hashTables.Length; i++)
                {
                    DbCommand sqlCmd = storeProcedureBuilder.GetReadHashBinsByHashBucketAndHashTableLSH(hashBucket[i], hashTables[i], connection);
                    reader = sqlCmd.ExecuteReader();
                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            if (dictOfFingers == null)
                                dictOfFingers = new Dictionary<Int32, List<HashBinMinHash>>();
                            do
                            {
                                Int32 hashId = (Int32) reader[FIELD_HASHBIN_MINHASH_ID];
                                Int32 id = (Int32) reader[FIELD_HASHBIN_MINHASH_TRACK_ID];
                                Int32 fingId = (Int32) reader[FIELD_HASHBIN_MINHASH_FINGERPRINT];
                                HashBinMinHash hash = new HashBinMinHash(hashId, hashBucket[i], hashTables[i], id, fingId);
                                if (!dictOfFingers.ContainsKey(fingId))
                                    dictOfFingers.Add(fingId, new List<HashBinMinHash>());
                                dictOfFingers[fingId].Add(hash);
                            } while (reader.Read());
                        }
                        reader.Close();
                    }
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                connection.Close();
            }
            return dictOfFingers;
        }

        /// <summary>
        ///   Read collection of Fingerprint Id and corresponding hash bucket, according only to hash bin value
        ///   <param name = "hashBucket">Hash bins to check</param>
        /// </summary>
        public Dictionary<Int32, List<HashBinMinHash>> ReadFingerprintsByHashBucketLSH(long[] hashBucket)
        {
            DbConnection connection = GetConnection();
            Dictionary<Int32, List<HashBinMinHash>> dictOfFingers = null;
            DbDataReader reader = null;
            try
            {
                connection.Open();
                foreach (long t in hashBucket)
                {
                    DbCommand sqlCmd = storeProcedureBuilder.GetReadHashBinsByHashBinsMinHashLSH(t, connection);
                    reader = sqlCmd.ExecuteReader();
                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            if (dictOfFingers == null)
                                dictOfFingers = new Dictionary<Int32, List<HashBinMinHash>>();
                            do
                            {
                                Int32 hashId = (Int32) reader[FIELD_HASHBIN_MINHASH_ID];
                                Int32 id = (Int32) reader[FIELD_HASHBIN_MINHASH_TRACK_ID];
                                Int32 fingId = (Int32) reader[FIELD_HASHBIN_MINHASH_FINGERPRINT];
                                int hashtable = (int) reader[FIELD_HASHBIN_MINHASH_HASHTABLE];
                                HashBinMinHash hash = new HashBinMinHash(hashId, t, hashtable, id, fingId);

                                if (!dictOfFingers.ContainsKey(fingId))
                                    dictOfFingers.Add(fingId, new List<HashBinMinHash>());
                                dictOfFingers[fingId].Add(hash);
                            } while (reader.Read());
                        }
                        reader.Close();
                    }
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                connection.Close();
            }
            return dictOfFingers;
        }

        /// <summary>
        ///   Read collection of Fingerprint Id and corresponding hash bucket, according only to hash bin value, and a specific threshold
        ///   <param name = "hashBucket">Hash bins to check</param>
        ///   <param name = "threshold">Threshold</param>
        /// </summary>
        public Dictionary<Int32, Tuple<Int32, int>> ReadFingerprintsByHashBucketLSH(long[] hashBucket, int threshold)
        {
            DbConnection connection = GetConnection();
            Dictionary<Int32, Tuple<Int32, int>> dictOfFingers = null;
            DbDataReader reader = null;
            const string delimiter = ";";
            StringBuilder builder = new StringBuilder();
            foreach (long item in hashBucket)
                builder.Append(item + delimiter);
            string hashbins = builder.ToString();
            try
            {
                DbCommand sqlCmd = storeProcedureBuilder.GetReadFingerprintsByThresholdAndHashBinsMinHash(hashbins, delimiter, threshold, connection);
                connection.Open();
                reader = sqlCmd.ExecuteReader();
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        dictOfFingers = new Dictionary<Int32, Tuple<Int32, int>>();
                        do
                        {
                            Int32 fingId = (Int32) reader[FIELD_HASHBIN_MINHASH_FINGERPRINT];
                            Int32 trackId = (Int32) reader[FIELD_HASHBIN_MINHASH_TRACK_ID];
                            int votes = (int) reader[FIELD_VOTES];
                            dictOfFingers[fingId] = new Tuple<Int32, int>(trackId, votes);
                        } while (reader.Read());
                    }
                    reader.Close();
                }
            }

            finally
            {
                if (reader != null) reader.Close();
                connection.Close();
            }
            return dictOfFingers;
        }

        #endregion

        #region Delete Methods

        /// <summary>
        ///   Delete a track from the database
        /// </summary>
        /// <param name = "trackId">Track Id</param>
        /// <returns>Number of changed rows</returns>
        /// <exception cref = "ArgumentException">trackId cannot be empty</exception>
        public int DeleteTrack(Int32 trackId)
        {
            DbCommand cmd = storeProcedureBuilder.GetDeleteTrackCommand(trackId, GetConnection());
            int retVal = 0;
            try
            {
                cmd.Connection.Open();
                retVal = cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Connection.Close();
            }
            return retVal;
        }

        /// <summary>
        ///   Delete a track from the database
        /// </summary>
        /// <param name = "track">Track to delete</param>
        /// <returns>Number of changed rows</returns>
        /// <exception cref = "ArgumentException">track cannot be null</exception>
        public int DeleteTrack(Track track)
        {
            if (track == null)
                throw new ArgumentException("track cannot be null");
            return DeleteTrack(track.Id);
        }

        /// <summary>
        ///   Delete a collection of tracks from the database
        /// </summary>
        /// <param name = "collection">Collection of Int32 to delete</param>
        /// <returns>Number of modified rows</returns>
        public int DeleteTrack(IEnumerable<Int32> collection)
        {
            if (collection == null)
                throw new ArgumentException("collection cannot be null");
            if (collection.Count() == 0)
                throw new ArgumentException("collection cannot be empty");
            int mRows = 0;
            using (DbConnection con = GetConnection())
            {
                try
                {
                    con.Open();
                    mRows = collection.Select(id => storeProcedureBuilder.GetDeleteTrackCommand(id, con)).Select(cmd => cmd.ExecuteNonQuery()).Sum();
                }
                finally
                {
                    con.Close();
                }
            }
            return mRows;
        }

        /// <summary>
        ///   Delete tracks from the database
        /// </summary>
        /// <param name = "collection">Collection of tracks to delete</param>
        /// <returns>Number of modified rows</returns>
        public int DeleteTrack(IEnumerable<Track> collection)
        {
            if (collection == null)
                throw new ArgumentException("collection cannot be null");
            if (collection.Count() == 0)
                throw new ArgumentException("collection cannot be empty");
            int mRows = 0;
            using (DbConnection con = GetConnection())
            {
                try
                {
                    con.Open();
                    foreach (Track track in collection)
                    {
                        DbCommand cmd = storeProcedureBuilder.GetDeleteTrackCommand(track.Id, con);
                        mRows += cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    con.Close();
                }
            }
            return mRows;
        }

        #endregion

        /// <summary>
        ///   Sets the connection string for Data Access Layer
        /// </summary>
        /// <param name = "connString">Connection String</param>
        public void SetConnectionString(string connString)
        {
            this.connectionString = connString;
        }
    }
}