namespace SoundFingerprinting.Dao.Internal
{
    using System.Collections.Generic;

    using SoundFingerprinting.Dao.Entities;

    internal class TrackDao : AbstractDao
    {
        private const string SpInsertTrack = "sp_InsertTrack";
        private const string SpReadTracks = "sp_ReadTracks";
        private const string SpReadTrackById = "sp_ReadTrackById";
        private const string SpReadTrackByArtistSongName = "sp_ReadTrackByArtistAndSongName";
        private const string SpDeleteTrack = "sp_DeleteTrack";
        private const string SpReadTrackByISRC = "sp_ReadTrackISRC";
        
        public TrackDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public void Insert(Track track)
        {
            track.Id = PrepareStoredProcedure(SpInsertTrack)
                            .WithParametersFromModel(track)
                            .Execute()
                            .AsScalar<int>();
        }

        public IList<Track> Read()
        {
            return PrepareStoredProcedure(SpReadTracks)
                        .Execute()
                        .AsListOfModel<Track>();
        }

        public Track ReadById(int id)
        {
            return PrepareStoredProcedure(SpReadTrackById)
                        .WithParameter("Id", id)
                        .Execute()
                        .AsModel<Track>();
        }

        public Track ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return PrepareStoredProcedure(SpReadTrackByArtistSongName)
                        .WithParameter("Artist", artist)
                        .WithParameter("Title", title)
                        .Execute().AsModel<Track>();
        }

        public Track ReadTrackByISRC(string isrc)
        {
            return PrepareStoredProcedure(SpReadTrackByISRC)
                        .WithParameter("ISRC", isrc)
                        .Execute().AsModel<Track>();
        }

        public int DeleteTrack(int trackId)
        {
            return PrepareStoredProcedure(SpDeleteTrack)
                        .WithParameter("Id", trackId)
                        .Execute()
                        .AsNonQuery();
        }
    }
}
