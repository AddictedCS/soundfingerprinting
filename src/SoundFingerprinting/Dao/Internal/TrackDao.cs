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
        private const string SpReadTrackByFingerprint = "sp_ReadTrackByFingerprint";
        private const string SpDeleteTrack = "sp_DeleteTrack";
        private const string SpReadDuplicatedTracks = "sp_ReadDuplicatedTracks";

        public TrackDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public void Insert(Track track)
        {
            track.Id = PrepareStoredProcedure(SpInsertTrack).WithParametersFromModel(track).Execute().AsScalar<int>();
        }

        public void Insert(IEnumerable<Track> collection)
        {
            foreach (var track in collection)
            {
                Insert(track);
            }
        }

        public IList<Track> Read()
        {
            return PrepareStoredProcedure(SpReadTracks).Execute().AsListOfModel<Track>();
        }

        public Track ReadById(int id)
        {
            return PrepareStoredProcedure(SpReadTrackById).WithParameter("Id", id).Execute().AsModel<Track>();
        }

        public Track ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return PrepareStoredProcedure(SpReadTrackByArtistSongName)
                        .WithParameter("Artist", artist)
                        .WithParameter("Title", title)
                        .Execute().AsModel<Track>();
        }

        public IList<Track> ReadTrackByFingerprintId(int id)
        {
            return
                PrepareStoredProcedure(SpReadTrackByFingerprint).WithParameter("Id", id).Execute().AsListOfModel<Track>();
        }

        public IDictionary<Track, int> ReadDuplicatedTracks()
        {
            return
                PrepareStoredProcedure(SpReadDuplicatedTracks).Execute().AsDictionary(
                    reader =>
                    new Track(reader.GetString("Artist"), reader.GetString("Title"))
                        {
                            Id = reader.GetInt32("Id")
                        },
                    reader => reader.GetInt32("Duplicates"));
        }

        public int DeleteTrack(int trackId)
        {
            return PrepareStoredProcedure(SpDeleteTrack).WithParameter("Id", trackId).Execute().AsNonQuery();
        }
    }
}