namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;

    public interface IAdvancedModelService : IModelService
    {
        void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);
    }
}
