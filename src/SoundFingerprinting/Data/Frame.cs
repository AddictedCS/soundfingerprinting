namespace SoundFingerprinting.Data
{
    public class Frame
    {
        public Frame(float[] imageRowCols, ushort rows, ushort cols, float startsAt, uint sequenceNumber)
        {
            ImageRowCols = imageRowCols;
            Rows = rows;
            Cols = cols;
            StartsAt = startsAt;
            SequenceNumber = sequenceNumber;
        }

        public float[] ImageRowCols { get; }

        public ushort Rows { get; }

        public ushort Cols { get; }

        public uint SequenceNumber { get; }

        public float StartsAt { get; }
    }
}
