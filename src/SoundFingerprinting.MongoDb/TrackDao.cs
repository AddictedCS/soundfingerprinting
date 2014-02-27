namespace SoundFingerprinting.MongoDb
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.MongoDb.Connection;
    using SoundFingerprinting.MongoDb.Data;

    internal class TrackDao : AbstractDao, ITrackDao
    {
        public TrackDao()
            : base(DependencyResolver.Current.Get<IMongoDatabaseProviderFactory>())
        {
            // no op
        }

        public IModelReference InsertTrack(TrackData trackData)
        {
            var collection = Database.GetCollection<Track>(TracksCollection);
            var track = Track.FromTrackData(trackData);
            collection.Insert(track);
            return new MongoModelReference(track.Id);
        }

        public TrackData ReadTrack(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            throw new System.NotImplementedException();
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            throw new System.NotImplementedException();
        }

        public IList<TrackData> ReadAll()
        {
            throw new System.NotImplementedException();
        }
    }
}
