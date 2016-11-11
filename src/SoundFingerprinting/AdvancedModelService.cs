namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    public abstract class AdvancedModelService : ModelService, IAdvancedModelService 
    {
        private readonly IFingerprintDao fingerprintDao;

        private readonly ISpectralImageDao spectralImageDao;

        protected AdvancedModelService(
            ITrackDao trackDao,
            ISubFingerprintDao subFingerprintDao,
            IFingerprintDao fingerprintDao,
            ISpectralImageDao spectralImageDao)
            : base(trackDao, subFingerprintDao)
        {
            this.fingerprintDao = fingerprintDao;
            this.spectralImageDao = spectralImageDao;
        }

        public virtual void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            this.spectralImageDao.InsertSpectralImages(spectralImages, trackReference);
        }

        public virtual List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference)
        {
            return this.spectralImageDao.GetSpectralImagesByTrackId(trackReference);
        }

        public virtual IModelReference InsertFingerprint(FingerprintData fingerprint)
        {
            return this.fingerprintDao.InsertFingerprint(fingerprint);
        }

        public virtual IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            return this.fingerprintDao.ReadFingerprintsByTrackReference(trackReference);
        }
    }
}
