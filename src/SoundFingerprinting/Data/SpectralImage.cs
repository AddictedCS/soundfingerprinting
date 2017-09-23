namespace SoundFingerprinting.Data
{
    internal class SpectralImage
    {
        public SpectralImage(float[] image, int rows, int cols, double startsAt, int sequenceNumber)
        {
            Image = image;
            Rows = rows;
            Cols = cols;
            StartsAt = startsAt;
            SequenceNumber = sequenceNumber;
        }

        public float[] Image { get; private set; }

        public int Rows { get; private set; }

        public int Cols { get; private set; }

        public int SequenceNumber { get; private set; }

        public double StartsAt { get; private set; }
    }
}
