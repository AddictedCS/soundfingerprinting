﻿namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using Data;

    public interface ISpectralImageDao
    {
        void InsertSpectralImages(IEnumerable<SpectralImageData> spectralImages);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);
    }
}
