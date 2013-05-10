namespace Soundfingerprinting.Hashing.MinHash
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    public class DbPermutations : IPermutations
    {
        private const int COMMAND_TIMEOUT = 60*10; /*600 sec*/
        private const string SP_READ_ALL_PERMUTATIONS = "sp_ReadPermutations";
        private const string FIELD_PERMUTATIONS_ID = "Id";
        private const string FIELD_PERMUTATIONS_PERMUTATION = "Permutation";
        private readonly string _connString;

        /// <summary>
        ///   Cached permutations
        /// </summary>
        private int[][] _cachedPermutations;

        /// <summary>
        ///   Instantiate data access manager using FingerprintConnectionString read from the App.Config.
        ///   If no such string is found in the App.Config, connection string is set to Empty
        /// </summary>
        public DbPermutations(string connectionString)
        {
            this._connString = connectionString;
            this._connection = new SqlConnection(this._connString);
        }

        #region Connection Management

        private readonly IDbConnection _connection;

        /// <summary>
        ///   Gets the connection to MSSQL Server
        /// </summary>
        /// <returns>SQL Connection object</returns>
        private IDbConnection GetConnection()
        {
            return this._connection;
        }

        #endregion

        #region IPermutations Members

        /// <summary>
        ///   Get permutations
        /// </summary>
        /// <returns></returns>
        public int[][] GetPermutations()
        {
            return this.ReadPermutations();
        }

        #endregion

        /// <summary>
        ///   Gets stored procedure with the assigned name
        /// </summary>
        /// <param name = "cmdTxt">Name of the stored procedure</param>
        /// <returns>SQL command to be executed</returns>
        /// <param name = "connection">Connection object to the database</param>
        private static SqlCommand GetStoredProcedureCommand(string cmdTxt, IDbConnection connection)
        {
            if (String.IsNullOrEmpty(cmdTxt)) throw new ArgumentException("cmdTest parameter is null or empty");
            if (!(connection is SqlConnection)) throw new ArgumentException("connection is not of type SqlConnection");

            SqlCommand sqlComm = new SqlCommand(cmdTxt)
                                 {
                                     CommandType = CommandType.StoredProcedure,
                                     CommandTimeout = COMMAND_TIMEOUT,
                                     Connection = (SqlConnection) connection
                                 };
            return sqlComm;
        }

        /// <summary>
        ///   Get a command to read all the permutations from the database
        /// </summary>
        /// <param name = "connection">SQL connection used to execute the command</param>
        /// <returns>SQL command to be executed</returns>
        public IDbCommand GetReadPermutations(IDbConnection connection)
        {
            SqlCommand sqlCommand = GetStoredProcedureCommand(SP_READ_ALL_PERMUTATIONS, connection);
            return sqlCommand;
        }


        /// <summary>
        ///   Read all permutations from the database
        /// </summary>
        /// <returns>Dictionary of permutations</returns>
        public int[][] ReadPermutations()
        {
            if (this._cachedPermutations != null)
                return this._cachedPermutations;
            IDbCommand sqlCommand = this.GetReadPermutations(this.GetConnection());
            IDataReader reader = null;
            Dictionary<int, int[]> result = null;
            try
            {
                sqlCommand.Connection.Open();
                reader = sqlCommand.ExecuteReader();
                if (reader != null)
                {
                    result = new Dictionary<int, int[]>();
                    while (reader.Read())
                    {
                        int id = (int) reader[FIELD_PERMUTATIONS_ID];
                        string permutation = (string) reader[FIELD_PERMUTATIONS_PERMUTATION];
                        string[] elementsOfPermutation = permutation.Split(new char[1] {';'},
                            StringSplitOptions.RemoveEmptyEntries);
                        int[] arrayOfPermutations = new int[elementsOfPermutation.Length];
                        for (int i = 0; i < elementsOfPermutation.Length; i++)
                            arrayOfPermutations[i] = Convert.ToInt32(elementsOfPermutation[i]);
                        result.Add(id, arrayOfPermutations);
                    }
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                sqlCommand.Connection.Close();
            }

            if (result != null)
            {
                this._cachedPermutations = new int[result.Count][];
                for (int i = 0; i < result.Count; i++)
                {
                    this._cachedPermutations[i] = result.ElementAt(i).Value;
                }
            }
            return this._cachedPermutations;
        }
    }
}