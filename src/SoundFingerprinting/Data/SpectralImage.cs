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

        public float[] Image { get; }

        public ushort Rows { get; }

        public ushort Cols { get; }

        public uint SequenceNumber { get; }

        public float StartsAt { get; }
    }
}
