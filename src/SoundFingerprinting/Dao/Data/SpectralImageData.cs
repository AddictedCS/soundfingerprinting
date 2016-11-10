namespace SoundFingerprinting.DAO.Data
{
    using System;

    using SoundFingerprinting.DAO;

    [Serializable]
    public class SpectralImageData
    {
        public SpectralImageData(float[] image, int orderNumber, IModelReference trackReference)
        {
            Image = image;
            TrackReference = trackReference;
            OrderNumber = orderNumber;
        }

        public float[] Image { get; set; }

        public int OrderNumber { get; set; }

        [IgnoreBinding]
        public IModelReference TrackReference { get; set; }
    }
}
