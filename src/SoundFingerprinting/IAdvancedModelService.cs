namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;

    public interface IAdvancedModelService : IModelService
    {
        IModelReference InsertFingerprint(FingerprintData fingerprint);

        IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference);

        void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);
    }
}
