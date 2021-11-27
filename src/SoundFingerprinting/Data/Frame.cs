namespace SoundFingerprinting.Data
{
    using System;
    using System.Linq;
    using SoundFingerprinting.Image;

    /// <summary>
    ///  Class that holds image data (either spectrogram image or any generic image that has to be fingerprinted).
    /// </summary>
    /// <remarks>
    ///  Since we store the image in a 2D array, only single channel images are allowed (grayscale).
    /// </remarks>
    public class Frame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="image">2D representation of the image.</param>
        /// <param name="startsAt">Starts at reference point (measured in seconds).</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        public Frame(float[][] image, float startsAt, uint sequenceNumber) : this(ImageService.Image2RowCols(image), (ushort)image.Length, (ushort)image[0].Length, startsAt, sequenceNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="imageRowCols">Row/Cols representation of the image.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="startsAt">Starts at reference point (measured in seconds).</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        public Frame(float[] imageRowCols, ushort rows, ushort cols, float startsAt, uint sequenceNumber)
        {
            ImageRowCols = imageRowCols;
            Rows = rows;
            Cols = cols;
            StartsAt = startsAt;
            SequenceNumber = sequenceNumber;
        }

        /// <summary>
        ///  Gets encoded 2D image in row/cols format.
        /// </summary>
        public float[] ImageRowCols { get; }

        /// <summary>
        ///  Gets number of rows in the image.
        /// </summary>
        public ushort Rows { get; }

        /// <summary>
        ///  Gets number of cols in the image.
        /// </summary>
        public ushort Cols { get; }

        /// <summary>
        /// Gets sequence number.
        /// </summary>
        public uint SequenceNumber { get; }

        /// <summary>
        ///  Gets starts at reference point (measured in seconds).
        /// </summary>
        public float StartsAt { get; }

        /// <summary>
        ///  Gets Rows * Cols size of the image.
        /// </summary>
        public int Length => ImageRowCols.Length;

        /// <summary>
        ///  Gets a deep copy of current image.
        /// </summary>
        /// <returns>New array copied from <see cref="ImageRowCols"/>.</returns>
        public float[] GetImageRowColsCopy()
        {
            float[] copy = new float[ImageRowCols.Length];
            Buffer.BlockCopy(ImageRowCols, 0, copy, 0, ImageRowCols.Length * sizeof(float));
            return copy;
        }

        /// <summary>
        ///  Gets quantized copy of the image in [0-255] range.
        /// </summary>
        /// <returns>Gets quantized copy of the image.</returns>
        public byte[] GetQuantizedCopy()
        {
#if DEBUG
            if (ImageRowCols.Any(f => f > 1.0 || f < 0))
            {
                throw new NotSupportedException("Frame contains entries outside of allowed interval [0, 1]");
            }
#endif

            return ImageRowCols.Select(f => (byte)(f * byte.MaxValue)).ToArray();
        }
    }
}