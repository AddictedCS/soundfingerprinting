namespace SoundFingerprinting.Dao
{
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    using SoundFingerprinting.Infrastructure;

    public class MsSqlDatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly IConnectionStringFactory connectionStringFactory;

        private readonly DbProviderFactory databaseProvider;

        public MsSqlDatabaseProviderFactory()
            : this(DependencyResolver.Current.Get<IConnectionStringFactory>())
        {
        }

        public MsSqlDatabaseProviderFactory(IConnectionStringFactory connectionStringFactory)
        {
            this.connectionStringFactory = connectionStringFactory;
            databaseProvider = SqlClientFactory.Instance;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection connection = databaseProvider.CreateConnection();
            if (connection != null)
            {
                connection.ConnectionString = connectionStringFactory.GetConnectionString();
                return connection;
            }

            return null;
        }
    }
}
