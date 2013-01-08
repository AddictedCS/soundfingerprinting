namespace Soundfingerprinting.Dao.Internal
{
    using System.Data;

    using Soundfingerprinting.DbStorage.Entities;

    internal abstract class AbstractDao
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

    internal class AlbumDao : AbstractDao
    {
        private const string SpInsertAlbum = "sp_InsertAlbum";
        private const string SpReadAlbums = "sp_ReadAlbums";



        public AlbumDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public void InsertAlbum(Album album)
        {
            album.Id = PrepareStoredProcedure(SpInsertAlbum)
                        .WithParametersFromModel(album)
                        .Execute()
                        .AsScalar<int>();
        }
    }
}
