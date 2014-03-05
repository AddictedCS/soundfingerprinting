namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO;
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
            return hashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, threshold)
                             .ToList();
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] buckets, int threshold, string trackGroupId)
        {
            return hashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(buckets, threshold, trackGroupId)
                             .ToList();
        }

        public IModelReference InsertFingerprint(FingerprintData fingerprint)
        {
            return fingerprintDao.InsertFingerprint(fingerprint);
        }

        public IModelReference InsertTrack(TrackData track)
        {
            return trackDao.InsertTrack(track);
        }

        public void InsertHashDataForTrack(IEnumerable<HashData> hashes, IModelReference trackReference)
        {
            foreach (var hashData in hashes)
            {
                var subFingerprintReference = subFingerprintDao.InsertSubFingerprint(hashData.SubFingerprint, trackReference);
                hashBinDao.InsertHashBins(hashData.HashBins, subFingerprintReference, trackReference);
            }
        }

        public IList<HashData> ReadHashDataByTrack(IModelReference trackReference)
        {
            return hashBinDao.ReadHashDataByTrackReference(trackReference);
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
            return fingerprintDao.ReadFingerprintsByTrackReference(trackReference);
        }

        public TrackData ReadTrackByReference(IModelReference trackReference)
        {
            return trackDao.ReadTrack(trackReference);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return trackDao.ReadTrackByISRC(isrc);
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            return trackDao.DeleteTrack(trackReference);
        }
    }
}
