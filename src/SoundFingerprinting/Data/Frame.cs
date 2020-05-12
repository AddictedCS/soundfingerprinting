namespace SoundFingerprinting.Data
{
    using System;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.Image;

    [Serializable]
    [ProtoContract]
    public class Frame
    {
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

        private Frame()
        {
            // no op, left for proto-buf
        }

        [ProtoMember(1)]
        public float[] ImageRowCols { get; }

        [ProtoMember(2)]
        public ushort Rows { get; }

        [ProtoMember(3)]
        public ushort Cols { get; }

        public uint SequenceNumber { get; }

        public float StartsAt { get; }

        public int Length => ImageRowCols.Length;

        public float[] GetImageRowColsCopy()
        {
            float[] copy = new float[ImageRowCols.Length];
            Buffer.BlockCopy(ImageRowCols, 0, copy, 0, ImageRowCols.Length * sizeof(float));
            return copy;
        }

        public byte[] GetQuantizedCopy()
        {
#if DEBUG
            if (ImageRowCols.Any(f => f > 1.0 || f < 0))
            {
                throw new NotSupportedException("Frame contains entries outside of allowed interval [0, 1]");
            }
#endif         

            return ImageRowCols.Select(f => (byte) (f * byte.MaxValue)).ToArray();
        }
    }
}
