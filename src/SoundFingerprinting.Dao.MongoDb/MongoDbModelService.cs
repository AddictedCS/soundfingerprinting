namespace SoundFingerprinting.Dao.MongoDb
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public class MongoDbModelService : IModelService
    {
        public IModelReference InsertFingerprint(FingerprintData fingerprintData)
        {
            throw new System.NotImplementedException();
        }

        public IModelReference InsertTrack(TrackData track)
        {
            throw new System.NotImplementedException();
        }

        public void InsertHashDataForTrack(IEnumerable<HashData> hashes, IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IList<HashData> ReadHashDataByTrack(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IList<TrackData> ReadAllTracks()
        {
            throw new System.NotImplementedException();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            throw new System.NotImplementedException();
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public TrackData ReadTrackByReference(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            throw new System.NotImplementedException();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            throw new System.NotImplementedException();
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] buckets, int threshold, string trackGroupId)
        {
            throw new System.NotImplementedException();
        }
    }
}
