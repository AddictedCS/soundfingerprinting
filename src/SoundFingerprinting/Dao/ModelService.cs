namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;

    public abstract class ModelService : IModelService
    {
        private readonly ITrackDao trackDao;

        private readonly IHashBinDao hashBinDao;

        private readonly ISubFingerprintDao subFingerprintDao;

        private readonly IFingerprintDao fingerprintDao;

        protected ModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao)
        {
            this.trackDao = trackDao;
            this.hashBinDao = hashBinDao;
            this.subFingerprintDao = subFingerprintDao;
            this.fingerprintDao = fingerprintDao;
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            return hashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, threshold).ToList();
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] buckets, int threshold, string trackGroupId)
        {
            return
                hashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(buckets, threshold, trackGroupId).ToList();
        }

        public IModelReference InsertFingerprint(FingerprintData fingerprintData)
        {
            int fingerprintId = fingerprintDao.Insert(fingerprintData.Signature, ((ModelReference<int>)fingerprintData.TrackReference).Id);
            return fingerprintData.FingerprintReference = new ModelReference<int>(fingerprintId);
        }

        public IModelReference InsertTrack(TrackData track)
        {
            trackDao.Insert(track);
            return track.TrackReference;
        }

        public void InsertHashDataForTrack(IEnumerable<HashData> hashes, IModelReference trackReference)
        {
            int trackId = ((ModelReference<int>)trackReference).Id;
            foreach (var hashData in hashes)
            {
                long subFingerprintId = subFingerprintDao.Insert(hashData.SubFingerprint, trackId);
                hashBinDao.Insert(hashData.HashBins, subFingerprintId);
            }
        }

        public IList<HashData> ReadHashDataByTrack(IModelReference trackReference)
        {
            return hashBinDao.ReadHashDataByTrackId(((ModelReference<int>)trackReference).Id);
        }

        public IList<TrackData> ReadAllTracks()
        {
            return trackDao.ReadAll();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return trackDao.ReadTrackByArtistAndTitleName(artist, title);
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            return fingerprintDao.ReadFingerprintsByTrackId(((ModelReference<int>)trackReference).Id);
        }

        public TrackData ReadTrackByReference(IModelReference trackReference)
        {
            return trackDao.ReadById(((ModelReference<int>)trackReference).Id);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return trackDao.ReadTrackByISRC(isrc);
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            return trackDao.DeleteTrack(((ModelReference<int>)trackReference).Id);
        }
    }
}
