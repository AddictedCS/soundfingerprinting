namespace SoundFingerprinting.SQL
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    internal class SpectralImageDao : ISpectralImageDao 
    {
        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }
    }
}
