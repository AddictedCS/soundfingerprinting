namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using DAO.Data;

    public interface IAdvancedModelService : IModelService
    {
        void InsertSpectralImages(IEnumerable<float[]> spectralImages, string trackId);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackId(string trackId);

        IEnumerable<TrackData> ReadTrackByTitle(string title);
    }
}
