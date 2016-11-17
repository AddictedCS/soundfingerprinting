namespace SoundFingerprinting.Data
{
    public class SpectralImage
    {
        public SpectralImage(float[][] image, double timestamp, int sequenceNumber)
        {
            Image = image;
            Timestamp = timestamp;
            SequenceNumber = sequenceNumber;
        }

        /// <summary>
        /// Gets or sets spectral image
        /// </summary>
        public float[][] Image { get; private set; }

        /// <summary>
        /// Gets or sets the sequence number of the spectral image. Possible values [1, N]
        /// </summary>
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// Gets or sets the timestamp details of the spectral image. Possible values [0, N]
        /// </summary>
        public double Timestamp { get; private set; }
    }
}
