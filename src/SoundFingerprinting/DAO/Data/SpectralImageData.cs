namespace SoundFingerprinting.DAO.Data
{
    using System;

    using DAO;

    [Serializable]
    public class SpectralImageData
    {
        public SpectralImageData(float[] image, int orderNumber, IModelReference trackReference)
        {
            Image = image;
            TrackReference = trackReference;
            OrderNumber = orderNumber;
        }

        public SpectralImageData(float[] image, int orderNumber, IModelReference spectralImageReference, IModelReference trackReference)
        {
            Image = image;
            TrackReference = trackReference;
            OrderNumber = orderNumber;
            SpectralImageReference = spectralImageReference;
        }

        internal SpectralImageData()
        {
        }

        public float[] Image { get; internal set; }

        public int OrderNumber { get; internal set; }

        [IgnoreBinding]
        public IModelReference TrackReference { get; internal set; }

        [IgnoreBinding]
        public IModelReference SpectralImageReference { get; internal set; }

        public override bool Equals(object obj)
        {
            if (!(obj is SpectralImageData))
            {
                return false;
            }

            return ((SpectralImageData)obj).SpectralImageReference.Equals(SpectralImageReference);
        }

        public override int GetHashCode()
        {
            return SpectralImageReference.GetHashCode();
        }
    }
}
