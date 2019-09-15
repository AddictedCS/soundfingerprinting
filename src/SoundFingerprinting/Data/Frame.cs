namespace SoundFingerprinting.Data
{
    using SoundFingerprinting.Image;

    public class Frame
    {
        private static readonly IImageService ImageService = new ImageService();
        
        public Frame(float[][] image, float startsAt, uint sequenceNumber) : this(ImageService.Image2RowCols(image), (ushort)image.Length, (ushort)image[0].Length, startsAt, sequenceNumber)
        {
        }
        
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

        public int Length => ImageRowCols.Length;
    }
}
