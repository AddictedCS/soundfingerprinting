namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public abstract class AdvancedModelService : ModelService, IAdvancedModelService 
    {
        private readonly ISpectralImageDao spectralImageDao;

        protected AdvancedModelService(
            string id,
            ITrackDao trackDao,
            ISubFingerprintDao subFingerprintDao,
            ISpectralImageDao spectralImageDao)
            : base(id, trackDao, subFingerprintDao)
        {
            this.spectralImageDao = spectralImageDao;
        }

        public abstract IEnumerable<TrackData> ReadTrackByTitle(string title);
        
        public virtual void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            spectralImageDao.InsertSpectralImages(spectralImages, trackReference);
        }

        public virtual IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference)
        {
            return spectralImageDao.GetSpectralImagesByTrackReference(trackReference);
        }

        public IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference)
        {
            return SubFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference)
                                    .Select(subFingerprint => new HashedFingerprint(
                                                subFingerprint.Hashes,
                                                subFingerprint.SequenceNumber,
                                                subFingerprint.SequenceAt))
                                    .ToList();
        }
    }
}
