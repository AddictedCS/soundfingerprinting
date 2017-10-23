namespace SoundFingerprinting.Data
{
    internal class SpectralImage
    {
        public SpectralImage(float[] image, ushort rows, ushort cols, float startsAt, uint sequenceNumber)
        {
            Image = image;
            Rows = rows;
            Cols = cols;
            StartsAt = startsAt;
            SequenceNumber = sequenceNumber;
        }

        public float[] Image { get; private set; }

        public ushort Rows { get; private set; }

        public ushort Cols { get; private set; }

        public uint SequenceNumber { get; private set; }

        public float StartsAt { get; private set; }
    }
}
