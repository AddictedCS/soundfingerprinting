namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    public interface IAdvancedModelService : IModelService
    {
        IModelReference InsertFingerprint(FingerprintData fingerprint);

        IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference);

        void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference);
    }
}
