namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using Data;

    public interface ISpectralImageDao
    {
        void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);
    }
}
