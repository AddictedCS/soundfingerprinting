namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public abstract class ModelService : IModelService
    {
        private readonly ITrackDao trackDao;
        private readonly IHashBinDao hashBinDao;
        private readonly ISubFingerprintDao subFingerprintDao;
        private readonly IFingerprintDao fingerprintDao;
        private readonly ISpectralImageDao spectralImageDao;

        protected ModelService(ITrackDao trackDao, IHashBinDao hashBinDao, ISubFingerprintDao subFingerprintDao, IFingerprintDao fingerprintDao, ISpectralImageDao spectralImageDao)
        {
            this.trackDao = trackDao;
            this.hashBinDao = hashBinDao;
            this.subFingerprintDao = subFingerprintDao;
            this.fingerprintDao = fingerprintDao;
            this.spectralImageDao = spectralImageDao;
        }

        public virtual IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            return hashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, threshold)
                             .ToList();
        }

        public virtual IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] buckets, int threshold, string trackGroupId)
        {
            return hashBinDao.ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(buckets, threshold, trackGroupId)
                             .ToList();
        }

        public virtual void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
           spectralImageDao.InsertSpectralImages(spectralImages, trackReference); 
        }

        public virtual List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference)
        {
            return spectralImageDao.GetSpectralImagesByTrackId(trackReference);
        }

        public virtual IModelReference InsertFingerprint(FingerprintData fingerprint)
        {
            return fingerprintDao.InsertFingerprint(fingerprint);
        }

        public virtual IModelReference InsertTrack(TrackData track)
        {
            return trackDao.InsertTrack(track);
        }

        public virtual void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            foreach (var hashData in hashes)
            {
                var subFingerprintReference = subFingerprintDao.InsertSubFingerprint(hashData.SubFingerprint, hashData.SequenceNumber, hashData.Timestamp, trackReference);
                hashBinDao.InsertHashBins(hashData.HashBins, subFingerprintReference, trackReference);
            }
        }

        public virtual IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference)
        {
            return hashBinDao.ReadHashedFingerprintsByTrackReference(trackReference);
        }

        public virtual IList<TrackData> ReadAllTracks()
        {
            return trackDao.ReadAll();
        }

        public virtual IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return trackDao.ReadTrackByArtistAndTitleName(artist, title);
        }

        public virtual IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            return fingerprintDao.ReadFingerprintsByTrackReference(trackReference);
        }

        public virtual TrackData ReadTrackByReference(IModelReference trackReference)
        {
            return trackDao.ReadTrack(trackReference);
        }

        public virtual TrackData ReadTrackByISRC(string isrc)
        {
            return trackDao.ReadTrackByISRC(isrc);
        }

        public virtual int DeleteTrack(IModelReference trackReference)
        {
            return trackDao.DeleteTrack(trackReference);
        }
    }
}
