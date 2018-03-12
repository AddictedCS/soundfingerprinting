namespace SoundFingerprinting.DAO.Data
{
    using System;

    using DAO;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class SpectralImageData
    {
        public SpectralImageData(float[] image, int orderNumber, IModelReference trackReference) : this()
        {
            Image = image;
            TrackReference = trackReference;
            OrderNumber = orderNumber;
        }

        public SpectralImageData(float[] image, int orderNumber, IModelReference spectralImageReference, IModelReference trackReference) : this(image, orderNumber, trackReference)
        {
            SpectralImageReference = spectralImageReference;
        }

        public SpectralImageData()
        {
        }

        [ProtoMember(1)]
        public float[] Image { get; internal set; }

        [ProtoMember(2)]
        public int OrderNumber { get; internal set; }

        [IgnoreBinding]
        [ProtoMember(3)]
        public IModelReference TrackReference { get; internal set; }

        [IgnoreBinding]
        [ProtoMember(4)]
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
