namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;

    /// <summary>
    ///  Interface definition for experimental features outside of regular <see cref="IModelService"/>.
    /// </summary>
    public interface IAdvancedModelService : IModelService
    {
        /// <summary>
        ///  Inserts spectral images into model service.
        /// </summary>
        /// <param name="spectralImages">Spectral images to insert.</param>
        /// <param name="trackId">Track ID corresponding to the spectral images.</param>
        void InsertSpectralImages(IEnumerable<float[]> spectralImages, string trackId);

        /// <summary>
        ///  Gets spectral images.
        /// </summary>
        /// <param name="trackId">Track ID for which to get spectral images.</param>
        /// <returns>List of spectral image objects.</returns>
        IEnumerable<SpectralImageData> GetSpectralImagesByTrackId(string trackId);
    }
}
