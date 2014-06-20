namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface ISpectralImageDao
    {
        void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference);
    }
}
