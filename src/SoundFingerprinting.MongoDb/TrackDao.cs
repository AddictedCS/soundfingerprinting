namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;

    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.DAO;
    using SoundFingerprinting.MongoDb.Entity;

    internal class TrackDao : AbstractDao, ITrackDao
    {
        public const string Tracks = "Tracks";

        public TrackDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public IModelReference InsertTrack(TrackData trackData)
        {
            var track = new Track
                {
                    Album = trackData.Album,
                    Artist = trackData.Artist,
                    GroupId = trackData.GroupId,
                    ISRC = trackData.ISRC,
                    ReleaseYear = trackData.ReleaseYear,
                    Title = trackData.Title,
                    TrackLengthSec = trackData.TrackLengthSec
                };

            var result = GetCollection<Track>(Tracks).Insert(track);
            if (!result.Ok)
            {
                return null;
            }

            return trackData.TrackReference = new MongoModelReference(track.Id);
        }

        public TrackData ReadTrack(IModelReference trackReference)
        {
            return GetCollection<Track>(Tracks).AsQueryable().Where(track => track.Id.Equals(trackReference.Id))
                                    .Select(track => GetTrackData(track))
                                    .FirstOrDefault();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            var deleteTracksResult = DeleteTracks(trackReference);
            var deleteSubFingerprintsResult = DeleteSubFingerprints(trackReference);
            var deleteHashResult = DeleteHashBins(trackReference);
            var deleteFingerprintsResult = DeleteFingerprints(trackReference);

            return
                (int)
                (deleteTracksResult.DocumentsAffected + deleteSubFingerprintsResult.DocumentsAffected
                 + deleteHashResult.DocumentsAffected + deleteFingerprintsResult.DocumentsAffected);
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return GetCollection<Track>(Tracks)
                                    .AsQueryable()
                                    .Where(track => track.Artist.Equals(artist) && track.Title.Equals(title))
                                    .Select(track => GetTrackData(track))
                                    .ToList();
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return GetCollection<Track>(Tracks).AsQueryable()
                                    .Where(track => track.ISRC.Equals(isrc))
                                    .Select(track => GetTrackData(track))
                                    .FirstOrDefault();
        }

        public IList<TrackData> ReadAll()
        {
            return GetCollection<Track>(Tracks).AsQueryable().Select(track => GetTrackData(track)).ToList();
        }

        private TrackData GetTrackData(Track track)
        {
            return new TrackData(
                track.ISRC,
                track.Artist,
                track.Title,
                track.Album,
                track.ReleaseYear,
                track.TrackLengthSec,
                new MongoModelReference(track.Id)) 
                {
                    GroupId = track.GroupId 
                };
        }

        private WriteConcernResult DeleteFingerprints(IModelReference trackReference)
        {
            var deleteFingerprintsQuery = MongoDB.Driver.Builders.Query<Fingerprint>.EQ(e => e.TrackId, trackReference.Id);
            return GetCollection<Fingerprint>(FingerprintDao.Fingerprints).Remove(deleteFingerprintsQuery);
        }

        private WriteConcernResult DeleteHashBins(IModelReference trackReference)
        {
            var deleteHashBinsQuery = MongoDB.Driver.Builders.Query<Hash>.EQ(e => e.TrackId, trackReference.Id);
            return GetCollection<Hash>(HashBinDao.HashBins).Remove(deleteHashBinsQuery);
        }

        private WriteConcernResult DeleteSubFingerprints(IModelReference trackReference)
        {
            var deleteSubFingerprintsQuery = MongoDB.Driver.Builders.Query<SubFingerprint>.EQ(e => e.TrackId, trackReference.Id);
            return GetCollection<SubFingerprint>(SubFingerprintDao.SubFingerprints).Remove(deleteSubFingerprintsQuery);
        }

        private WriteConcernResult DeleteTracks(IModelReference trackReference)
        {
            var query = MongoDB.Driver.Builders.Query<Track>.EQ(e => e.Id, trackReference.Id);
            return GetCollection<Track>(Tracks).Remove(query);
        }
    }
}
