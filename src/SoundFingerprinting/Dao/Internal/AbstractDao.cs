namespace SoundFingerprinting.Dao.Internal
{
    using System.Data;

    internal class AbstractDao
    {
        private const int CommandTimeoutInSeconds = 600;

        private readonly IDatabaseProviderFactory databaseProvider;

        private readonly IModelBinderFactory modelBinderFactory;

        protected AbstractDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
        {
            this.databaseProvider = databaseProvider;
            this.modelBinderFactory = modelBinderFactory;
        }

        public IParameterBinder PrepareStoredProcedure(string nameOfStoredProcedure)
        {
            IDbConnection connection = databaseProvider.CreateConnection();
            IDbCommand databaseCommand = connection.CreateCommand();
            databaseCommand.CommandText = nameOfStoredProcedure;
            databaseCommand.CommandType = CommandType.StoredProcedure;
            databaseCommand.CommandTimeout = CommandTimeoutInSeconds;
            return new ParameterBinder(connection, databaseCommand, modelBinderFactory);
        }
    }
}