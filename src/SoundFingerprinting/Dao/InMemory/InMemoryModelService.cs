namespace SoundFingerprinting.Dao.InMemory
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public class InMemoryModelService : IModelService
    {
        private readonly TrackStorageDao trackStorageDao;

        public InMemoryModelService()
        {
            trackStorageDao = new TrackStorageDao();
        }

        public IModelReference InsertFingerprint(FingerprintData fingerprintData)
        {
            throw new NotImplementedException();
        }

        public IModelReference InsertTrack(TrackData track)
        {
            trackStorageDao.Insert(track);
            return track.TrackReference;
        }

        public void InsertHashDataForTrack(IEnumerable<HashData> hashes, IModelReference trackReference)
        {
            throw new NotImplementedException();
        }

        public IList<TrackData> ReadAllTracks()
        {
            return trackStorageDao.ReadAll();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return trackStorageDao.ReadTrackByArtistAndTitleName(artist, title);
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            throw new NotImplementedException();
        }

        public TrackData ReadTrackByReference(IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot read from the underlying source with specified track reference");
            }

            return trackStorageDao.ReadById(((ModelReference<int>)trackReference).Id);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return trackStorageDao.ReadByISRC(isrc);
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            throw new NotImplementedException();
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            throw new NotImplementedException();
        }
    }
}
