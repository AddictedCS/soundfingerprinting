namespace SoundFingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal class TrackDao : AbstractDao, ITrackDao
    {
        private const string SpInsertTrack = "sp_InsertTrack";
        private const string SpReadTracks = "sp_ReadTracks";
        private const string SpReadTrackById = "sp_ReadTrackById";
        private const string SpReadTrackByArtistSongName = "sp_ReadTrackByArtistAndSongName";
        private const string SpDeleteTrack = "sp_DeleteTrack";
        private const string SpReadTrackByISRC = "sp_ReadTrackISRC";

        private readonly Action<TrackData, IReader> trackReferenceReader = (item, reader) => { item.TrackReference = new ModelReference<int>(reader.GetInt32("Id")); };

        public TrackDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public int Insert(TrackData track)
        {
            int id = PrepareStoredProcedure(SpInsertTrack)
                            .WithParametersFromModel(track)
                            .Execute()
                            .AsScalar<int>();
            var trackReference = new ModelReference<int>(id);
            track.TrackReference = trackReference;
            return id;
        }

        public IList<TrackData> ReadAll()
        {
            return PrepareStoredProcedure(SpReadTracks)
                        .Execute()
                        .AsListOfComplexModel(trackReferenceReader);
        }

        public TrackData ReadById(int id)
        {
            return PrepareStoredProcedure(SpReadTrackById)
                        .WithParameter("Id", id)
                        .Execute()
                        .AsComplexModel(trackReferenceReader);
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return PrepareStoredProcedure(SpReadTrackByArtistSongName)
                        .WithParameter("Artist", artist)
                        .WithParameter("Title", title)
                        .Execute()
                        .AsListOfComplexModel(trackReferenceReader);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return PrepareStoredProcedure(SpReadTrackByISRC)
                        .WithParameter("ISRC", isrc)
                        .Execute()
                        .AsComplexModel(trackReferenceReader);
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
