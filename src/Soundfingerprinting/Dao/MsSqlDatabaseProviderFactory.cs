namespace Soundfingerprinting.Dao
{
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    public class MsSqlDatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly IConnectionStringFactory connectionStringFactory;

        private readonly DbProviderFactory databaseProvider;

        public MsSqlDatabaseProviderFactory(IConnectionStringFactory connectionStringFactory)
        {
            this.connectionStringFactory = connectionStringFactory;
            databaseProvider = SqlClientFactory.Instance;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection connection = databaseProvider.CreateConnection();
            connection.ConnectionString = connectionStringFactory.GetConnectionString();
            return connection;
        }
    }
}
