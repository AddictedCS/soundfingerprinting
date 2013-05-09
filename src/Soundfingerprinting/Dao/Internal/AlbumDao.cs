namespace Soundfingerprinting.Dao.Internal
{
    using System.Collections.Generic;

    using Soundfingerprinting.Dao.Entities;

    internal class AlbumDao : AbstractDao
    {
        private const string UknownAlbumName = "UNKNOWN";

        private const string SpInsertAlbum = "sp_InsertAlbum";
        private const string SpReadAlbums = "sp_ReadAlbums";
        private const string SpReadUnknownAlbums = "sp_ReadAlbumUnknown";
        private const string SpReadAlbumById = "sp_ReadAlbumById";

        public AlbumDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public void Insert(Album album)
        {
            album.Id = PrepareStoredProcedure(SpInsertAlbum)
                        .WithParametersFromModel(album)
                        .Execute()
                        .AsScalar<int>();
        }
         
        public void Insert(IEnumerable<Album> collection)
        {
            foreach (var album in collection)
            {
                Insert(album);
            }
        }

        public IList<Album> Read()
        {
            return PrepareStoredProcedure(SpReadAlbums)
                    .Execute()
                    .AsListOfModel<Album>();
        }

        public Album ReadAlbumByName(string name)
        {
            return PrepareStoredProcedure(SpReadUnknownAlbums)
                    .WithParameter("Name", name)
                    .Execute()
                    .AsModel<Album>();
        }

        public Album ReadUnknownAlbum()
        {
            return PrepareStoredProcedure(SpReadUnknownAlbums)
                    .WithParameter("Name", UknownAlbumName)
                    .Execute()
                    .AsModel<Album>();
        }

        public Album ReadById(int id)
        {
            return PrepareStoredProcedure(SpReadAlbumById)
                    .WithParameter("Id", id)
                    .Execute()
                    .AsModel<Album>();
        }
    }
}
