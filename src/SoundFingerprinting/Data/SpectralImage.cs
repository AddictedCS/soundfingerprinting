namespace SoundFingerprinting.Data
{
    internal class SpectralImage
    {
        public SpectralImage(float[][] image, double startsAt, int sequenceNumber)
        {
            Image = image;
            StartsAt = startsAt;
            SequenceNumber = sequenceNumber;
        }

        public float[][] Image { get; private set; }

        public int SequenceNumber { get; private set; }

        public double StartsAt { get; private set; }
    }
}
