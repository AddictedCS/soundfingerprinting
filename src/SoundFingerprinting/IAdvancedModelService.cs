namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;

    using SoundFingerprinting.Data;

    public interface IAdvancedModelService : IModelService
    {
        void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);

        IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference);
        
        IEnumerable<TrackData> ReadTrackByTitle(string title);
    }
}
