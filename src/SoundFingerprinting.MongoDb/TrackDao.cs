namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;

    using MongoDB.Driver.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;
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
            var collection = GetCollection<Track>(Tracks);
            var track = Track.FromTrackData(trackData);
            collection.Insert(track);
            return trackData.TrackReference = new MongoModelReference(track.Id);
        }

        public TrackData ReadTrack(IModelReference trackReference)
        {
            var collection = GetCollection<Track>(Tracks);
            return collection.AsQueryable().Where(t => t.Id.Equals(trackReference.Id))
                                    .Select(t => GetTrackData(t))
                                    .FirstOrDefault();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            var collection = GetCollection<Track>(Tracks);
            var query = MongoDB.Driver.Builders.Query<Track>.EQ(e => e.Id, trackReference.Id);
            collection.Remove(query);
            return 1;
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            var collection = GetCollection<Track>(Tracks);
            return collection.AsQueryable().Where(track => track.Artist.Equals(artist) && track.Title.Equals(title))
                                    .Select(track => GetTrackData(track))
                                    .ToList();
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            var collection = GetCollection<Track>(Tracks);
            return collection.AsQueryable().Where(track => track.ISRC.Equals(isrc))
                                    .Select(track => GetTrackData(track))
                                    .FirstOrDefault();
        }

        public IList<TrackData> ReadAll()
        {
            var collection = GetCollection<Track>(Tracks);
            return collection.AsQueryable().Select(track => GetTrackData(track)).ToList();
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
    }
}
